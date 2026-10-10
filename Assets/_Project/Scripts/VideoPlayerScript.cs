using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

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
    private int playIndex = 0;
    private float playSpeed = 1f;
    private bool isLooping = false;
    private Vector2 loop = Vector2.zero;

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
    /// 플레이어 슬롯에 클립을 넣고 미리 준비
    /// </summary>
    /// <remarks>
    /// 지금 재생 중인 슬롯에 넣으면 재생 중인 영상이 바뀐다
    /// </remarks>
    /// <param name="idx"> 플레이어 슬롯 번호 </param>
    /// <param name="clip"> 준비할 클립 </param>
    public void SetConnectedClip(int idx, VideoClip clip)
    {
        if (idx < 0 || idx >= videoPlayers.Count)
        {
            Debug.LogError($"Invalid player index: {idx}");
            return;
        }

        videoPlayers[idx].clip = clip;
        videoPlayers[idx].Prepare();
    }

    #endregion

    #region Playback

    /// <summary>
    /// 이전 플레이어를 멈추고 해당 슬롯의 클립을 재생
    /// </summary>
    /// <remarks>
    /// 구간 반복은 해제되므로 필요하면 재생 후 SetLoopTime을 다시 호출한다
    /// </remarks>
    /// <param name="idx"> 재생할 플레이어 슬롯 번호 </param>
    public void PlayClip(int idx)
    {
        if (idx < 0 || idx >= videoPlayers.Count)
        {
            Debug.LogError($"Invalid player index: {idx}");
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
    /// 재생 속도 변경. 이후 재생하는 클립에도 유지된다
    /// </summary>
    /// <param name="speed"> 재생 배속 </param>
    public void ChangePlaySpeed(float speed)
    {
        playSpeed = speed;
        videoPlayers[playIndex].playbackSpeed = playSpeed;
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
