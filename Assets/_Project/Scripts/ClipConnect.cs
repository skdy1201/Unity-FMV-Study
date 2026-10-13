using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using FMV;

/// <summary>
/// 인스펙터에 등록한 VideoClip을 클립 이름으로 찾아 주는 조회 테이블
/// </summary>
/// <remarks>
/// 키는 VideoClip 에셋 이름(clip.name)이다. 이름이 겹치면 목록에서 뒤에 있는 클립이 남는다
/// </remarks>
[System.Serializable]
public class ClipConnect : ISetting
{
    #region Serialized Fields

    [SerializeField] private List<VideoClip> clips = new List<VideoClip>();

    #endregion

    #region Private Fields

    private Dictionary<string, VideoClip> clipMap = new Dictionary<string, VideoClip>();

    #endregion

    #region ISetting

    public void Init()
    {
        foreach (VideoClip clip in clips)
        {
            clipMap[clip.name] = clip;
        }
    }

    public void End()
    {
    }

    #endregion

    #region Lookup

    /// <summary>
    /// 클립 이름에 해당하는 VideoClip을 반환
    /// </summary>
    /// <remarks>
    /// 분기 id("2a")가 아니라 VideoClip 에셋 이름("2a_Bargainburg_FailToStart")을 받는다.
    /// id를 넣으면 에러 없이 null이 나오니, id로 찾을 때는 GameController.GetClipById를 쓴다.
    /// null을 넣으면 예외가 난다
    /// </remarks>
    /// <param name="clipName"> 찾을 VideoClip 에셋 이름 </param>
    /// <returns> 찾은 클립, 없으면 null </returns>
    public VideoClip GetClip(string clipName)
    {
        return clipMap.TryGetValue(clipName, out VideoClip clip) ? clip : null;
    }

    #endregion
}
