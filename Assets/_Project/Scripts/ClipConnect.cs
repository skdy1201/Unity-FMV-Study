using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// 인스펙터에 등록한 VideoClip을 클립 이름으로 찾아 주는 조회 테이블
/// </summary>
/// <remarks>
/// 키는 VideoClip 에셋 이름(clip.name)이다. 이름이 겹치면 목록에서 뒤에 있는 클립이 남는다
/// </remarks>
public class ClipConnect : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private List<VideoClip> clips = new List<VideoClip>();

    #endregion

    #region Private Fields

    private Dictionary<string, VideoClip> clipMap = new Dictionary<string, VideoClip>();

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        foreach (VideoClip clip in clips)
        {
            clipMap[clip.name] = clip;
        }
    }

    #endregion

    #region Lookup

    /// <summary>
    /// 클립 이름에 해당하는 VideoClip을 반환
    /// </summary>
    /// <param name="clipName"> 찾을 VideoClip 에셋 이름 </param>
    /// <returns> 찾은 클립, 없으면 null </returns>
    public VideoClip GetClip(string clipName)
    {
        return clipMap.TryGetValue(clipName, out VideoClip clip) ? clip : null;
    }

    #endregion
}
