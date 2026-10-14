using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace FMV
{
    /// <summary>
    /// VideoPlayer 여러 개를 하나의 RenderTexture에 연결해, 다음 분기 클립을 미리 준비해 두고 바꿔 가며 재생
    /// </summary>
    /// <remarks>
    /// 모든 플레이어가 같은 RenderTexture에 그리므로 한 번에 하나만 재생해야 한다
    /// </remarks>
    public class VideoPlayerScript : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private RenderTexture renderTexture;

        #endregion

        #region Private Fields

        private const int MaxVideoCount = 4;

        private readonly List<VideoPlayer> videoPlayers = new List<VideoPlayer>();
        private readonly string[] slotIds = new string[MaxVideoCount];
        private int playIndex = 0;
        private float playSpeed = 1f;
        private bool isLooping = false;
        private Vector2 loop = Vector2.zero;

        #endregion

        #region Events

        /// <summary>
        /// 재생 중인 클립이 끝까지 재생되면 결과를 넘겨 호출된다
        /// </summary>
        public event Action<NodeResult> ClipEnd;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (renderTexture == null)
            {
                Debug.LogError("Render Texture empty!");
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

        private void Update()
        {
            if (isLooping && videoPlayers[playIndex].isPlaying && videoPlayers[playIndex].time >= loop.y)
            {
                videoPlayers[playIndex].time = loop.x;
            }
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
        /// 구간 반복은 해제되므로 필요하면 재생 후 SetLoopTime을 다시 호출한다
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
        /// 클립이 끝까지 재생되면 ClipEnd로 다시 알림
        /// </summary>
        /// <param name="source"> 재생이 끝난 플레이어 </param>
        private void OnLoopPointReached(VideoPlayer source)
        {
            ClipEnd?.Invoke(NodeResult.Finished);
        }

        #endregion

        #region Loop

        /// <summary>
        /// 현재 클립의 구간 반복 설정
        /// </summary>
        /// <param name="loopTime"> x = 반복 시작 시간, y = 반복 끝 시간(초) </param>
        public void SetLoopTime(Vector2 loopTime)
        {
            loop = loopTime;
            isLooping = true;
        }

        /// <summary>
        /// 구간 반복 해제
        /// </summary>
        public void ResetLoop()
        {
            loop = Vector2.zero;
            isLooping = false;
        }

        #endregion
    }
}
