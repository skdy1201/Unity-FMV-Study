using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace FMV
{
    /// <summary>
    /// VideoPlayer 여러 개를 하나의 RenderTexture에 연결해, 다음 분기 클립을 미리 준비해 두고 바꿔 가며 재생
    /// </summary>
    /// <remarks>
    /// 모든 플레이어가 같은 RenderTexture에 그리므로 한 번에 하나만 재생해야 한다.
    /// 영상 화면은 screenGroup의 CanvasGroup으로 켜고 끈다. 플레이어와 코루틴은 이 오브젝트에 있으므로, 이 오브젝트를 끄지 말고 화면만 숨긴다.
    /// 화면 Canvas 자체에 붙일 때는 같은 오브젝트의 CanvasGroup을 screenGroup에 넣는다
    /// </remarks>
    public class VideoPlayerController : MonoBehaviour, IUIVisible
    {
        #region Serialized Fields

        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private CanvasGroup screenGroup;

        #endregion

        #region Private Fields

        private const int MaxVideoCount = 4;

        private readonly List<VideoPlayer> videoPlayers = new List<VideoPlayer>();
        private readonly string[] slotIds = new string[MaxVideoCount];
        private int playIndex = 0;
        private float playSpeed = 1f;
        private Vector2 loop = Vector2.zero;

        private Coroutine loopRoutine;
        private Coroutine actionRoutine;

        #endregion

        #region Properties

        /// <summary>
        /// 영상 화면이 보이는 중인지
        /// </summary>
        /// <remarks>
        /// Show/Hide가 CanvasGroup의 alpha를 바꾸므로 그 값으로 판단한다
        /// </remarks>
        public bool IsVisible => screenGroup.alpha > 0f;

        #endregion

        #region Events

        /// <summary>
        /// 재생 중인 클립이 끝까지 재생되면 결과를 넘겨 호출된다
        /// </summary>
        public event Action<NodeResult> ClipEnd;

        /// <summary>
        /// 재생 중인 클립이 WaitActionTime으로 정한 시간에 도달하면 호출된다
        /// </summary>
        public event Action OnActionReached;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (renderTexture == null)
            {
                Debug.LogError("Render Texture empty!");
            }
            else
            {
                // RenderTexture는 에셋이라 지난 실행의 마지막 프레임이 남아 있으므로 비우고 시작한다
                ClearScreen();
            }

            if (screenGroup == null)
            {
                Debug.LogError("Screen CanvasGroup empty!");
            }

            for (int i = 0; i < MaxVideoCount; ++i)
            {
                VideoPlayer player = gameObject.AddComponent<VideoPlayer>();
                player.playOnAwake = false;
                player.renderMode = VideoRenderMode.RenderTexture;
                player.targetTexture = renderTexture;
                videoPlayers.Add(player);

                player.loopPointReached += OnLoopPointReached;
            }
        }

        #endregion

        #region IUIVisible

        /// <summary>
        /// 영상 화면을 보이고 입력을 받게 한다
        /// </summary>
        /// <remarks>
        /// 화면만 켜고 재생은 하지 않는다. 재생은 Show 뒤에 PlayClip으로 시작한다
        /// </remarks>
        public void Show()
        {
            SetGroupVisible(true);
        }

        /// <summary>
        /// 영상 화면을 숨기고 입력을 막는다. 현재 플레이어는 Stop으로 처음으로 되돌리고 화면을 비운다
        /// </summary>
        /// <remarks>
        /// 숨기는 건 분기도로 나갈 때라, 같은 분기를 다시 고르면 처음부터 재생되어야 한다.
        /// PlayClip은 같은 슬롯이면 Stop 없이 Play만 하므로, 일시정지로 두면 멈춘 시점부터 이어진다.
        /// 재생 버튼으로 일시정지해 둔 영상도 되돌려야 하므로 재생 중인지와 상관없이 Stop한다.
        /// Stop은 준비 상태도 해제하므로 같은 클립을 다시 재생하면 Prepare부터 한다.
        /// 그 사이 나가기 직전 장면이 보이지 않도록 RenderTexture를 비운다.
        /// 구간 반복·Action 대기는 다음 PlayClip이 해제한다
        /// </remarks>
        public void Hide()
        {
            SetGroupVisible(false);

            videoPlayers[playIndex].Stop();
            ClearScreen();
        }

        #endregion

        #region Clip Setup

        /// <summary>
        /// 분기 id별 클립을 빈 슬롯에 넣고 미리 준비
        /// </summary>
        /// <remarks>
        /// 이미 준비된 id는 슬롯을 그대로 두어 다시 Prepare하지 않는다.
        /// 목록에 없는 id의 슬롯은 비워서 재사용하므로, 지금 재생할 클립도 목록에 넣어야 한다.
        /// 같은 id가 여러 번 들어오면 처음 것만 쓴다
        /// </remarks>
        /// <param name="clips"> 준비할 (분기 id, 클립) 목록 </param>
        public void PrepareNextClips(IReadOnlyList<(string id, VideoClip clip)> clips)
        {
            HashSet<string> keepIds = new HashSet<string>();
            foreach ((string id, VideoClip) item in clips)
            {
                keepIds.Add(item.id);
            }

            for (int i = 0; i < slotIds.Length; ++i)
            {
                if (slotIds[i] != null && !keepIds.Contains(slotIds[i]))
                {
                    slotIds[i] = null;
                }
            }

            foreach ((string id, VideoClip clip) in clips)
            {
                if (string.IsNullOrEmpty(id) || clip == null)
                {
                    Debug.LogError($"Invalid clip: id = {id}, clip = {clip}");
                    continue;
                }

                if (Array.IndexOf(slotIds, id) >= 0)
                {
                    continue;
                }

                int emptySlot = Array.IndexOf(slotIds, null);
                if (emptySlot < 0)
                {
                    Debug.LogError($"No empty player slot for {id} (max {MaxVideoCount})");
                    continue;
                }

                SetConnectedClip(emptySlot, id, clip);
            }
        }

        private void SetConnectedClip(int idx, string id, VideoClip clip)
        {
            slotIds[idx] = id;
            videoPlayers[idx].clip = clip;
            videoPlayers[idx].Prepare();
        }

        #endregion

        #region Playback

        /// <summary>
        /// 이전 플레이어를 멈추고 해당 분기의 클립을 재생
        /// </summary>
        /// <remarks>
        /// PrepareNextClips로 준비된 id만 재생할 수 있다.
        /// 구간 반복과 Action 시간 대기는 해제되므로 필요하면 재생 후 SetLoopTime·WaitActionTime을 다시 호출한다
        /// </remarks>
        /// <param name="id"> 재생할 분기 id </param>
        public void PlayClip(string id)
        {
            int idx = Array.IndexOf(slotIds, id);
            if (idx < 0)
            {
                Debug.LogError($"Clip not prepared: {id}");
                return;
            }

            // Stop은 준비 상태까지 해제하므로, 같은 슬롯이면 멈추지 않는다
            if (idx != playIndex)
            {
                videoPlayers[playIndex].Stop();
            }

            playIndex = idx;
            ResetLoop();
            StopWaitAction();
            videoPlayers[playIndex].playbackSpeed = playSpeed;
            videoPlayers[playIndex].Play();
        }

        /// <summary>
        /// 현재 플레이어가 재생 중이면 일시정지, 멈춰 있으면 이어서 재생
        /// </summary>
        public void RenewPlay()
        {
            VideoPlayer player = videoPlayers[playIndex];
            if (player == null)
            {
                Debug.LogError($"Video player is null: index = {playIndex}");
                return;
            }

            if (player.isPlaying)
            {
                player.Pause();
            }
            else
            {
                player.Play();
            }
        }

        /// <summary>
        /// 재생 속도 변경. 이후 재생하는 클립에도 유지된다
        /// </summary>
        /// <param name="speed"> 재생 배속 </param>
        public void ChangePlaySpeed(float speed)
        {
            playSpeed = speed;
            videoPlayers[playIndex].playbackSpeed = playSpeed;
        }

        /// <summary>
        /// 현재 클립에만 Action 중 재생 속도를 건다
        /// </summary>
        /// <remarks>
        /// 반복 구간이 없는 QTE처럼 SetLoopTime을 타지 않는 Action에서 쓴다.
        /// playSpeed는 바꾸지 않으므로, 다음 클립의 PlayClip에서 원래 속도로 돌아간다
        /// </remarks>
        /// <param name="speed"> Action 중 재생 배속 </param>
        public void SetActionSpeed(float speed)
        {
            videoPlayers[playIndex].playbackSpeed = speed;
        }

        /// <summary>
        /// 클립이 끝까지 재생되면 ClipEnd로 다시 알림
        /// </summary>
        /// <param name="source"> 재생이 끝난 플레이어 </param>
        private void OnLoopPointReached(VideoPlayer source)
        {
            ClipEnd?.Invoke(NodeResult.Finished);
        }

        #endregion

        #region Action Time

        /// <summary>
        /// 현재 클립이 지정한 시간에 도달하면 OnActionReached로 알린다
        /// </summary>
        /// <remarks>
        /// 이미 기다리는 중이면 이전 대기를 버리고 새 시간으로 바꾼다.
        /// 어떤 Action인지는 모르고 시간만 알린다. 판단은 받는 쪽이 한다
        /// </remarks>
        /// <param name="time"> 알릴 클립 시간(초) </param>
        public void WaitActionTime(float time)
        {
            StopWaitAction();
            actionRoutine = StartCoroutine(WaitActionRoutine(time));
        }

        private void StopWaitAction()
        {
            if (actionRoutine != null)
            {
                StopCoroutine(actionRoutine);
                actionRoutine = null;
            }
        }

        private IEnumerator WaitActionRoutine(float time)
        {
            while (videoPlayers[playIndex].time < time)
            {
                yield return null;
            }

            actionRoutine = null;
            OnActionReached?.Invoke();
        }

        #endregion

        #region Loop

        /// <summary>
        /// 현재 클립의 구간 반복과 반복 중 재생 속도 설정
        /// </summary>
        /// <remarks>
        /// 반복 중일 때만 코루틴이 돌고, ResetLoop나 PlayClip에서 멈춘다.
        /// 속도는 현재 플레이어에만 적용하고 playSpeed는 바꾸지 않으므로, 반복이 풀리면 원래 속도로 돌아간다
        /// </remarks>
        /// <param name="loopTime"> x = 반복 시작 시간, y = 반복 끝 시간(초) </param>
        /// <param name="speed"> 반복 중 재생 배속 </param>
        public void SetLoopTime(Vector2 loopTime, float speed = 1f)
        {
            ResetLoop();
            loop = loopTime;
            videoPlayers[playIndex].playbackSpeed = speed;
            loopRoutine = StartCoroutine(LoopRoutine());
        }

        /// <summary>
        /// 구간 반복 해제. 재생 속도도 playSpeed로 되돌린다
        /// </summary>
        public void ResetLoop()
        {
            if (loopRoutine != null)
            {
                StopCoroutine(loopRoutine);
                loopRoutine = null;
            }

            loop = Vector2.zero;
            videoPlayers[playIndex].playbackSpeed = playSpeed;
        }

        private IEnumerator LoopRoutine()
        {
            while (true)
            {
                VideoPlayer player = videoPlayers[playIndex];
                if (player.isPlaying && player.time >= loop.y)
                {
                    player.time = loop.x;

                    // time 설정은 비동기 탐색이라 끝나기 전까지 time은 이전 값을 돌려준다.
                    // 그 사이 다시 설정하면 탐색이 끝나지 못하므로, 구간 안으로 돌아올 때까지 기다린다
                    while (player.time >= loop.y)
                    {
                        yield return null;
                    }
                }

                yield return null;
            }
        }

        #endregion

        #region Helpers

        /// <remarks>
        /// CanvasGroup으로 켜고 끄므로, 보임·입력·레이캐스트를 한 번에 맞춘다
        /// </remarks>
        private void SetGroupVisible(bool visible)
        {
            screenGroup.alpha = visible ? 1f : 0f;
            screenGroup.interactable = visible;
            screenGroup.blocksRaycasts = visible;
        }

        /// <remarks>
        /// RenderTexture는 다시 그려지기 전까지 마지막 프레임을 유지하므로, 남은 장면을 검은색으로 지운다.
        /// 분기 사이 전환에서는 이전 프레임이 틈을 메우므로 부르지 않는다
        /// </remarks>
        private void ClearScreen()
        {
            // active는 지금 GPU가 그리는 대상이고, GL.Clear는 이 대상을 지운다.
            // 다른 렌더링에 영향을 주지 않도록 원래 대상을 previous에 담아 두었다가 지운 뒤 되돌린다
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = previous;
        }

        #endregion
    }
}
