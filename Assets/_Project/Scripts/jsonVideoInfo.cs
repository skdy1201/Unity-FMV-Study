using UnityEngine;

/// <summary>
/// 영상 하나의 id와 VideoClip 이름. videos.json의 항목 하나에 해당한다
/// </summary>
/// <remarks>
/// JsonUtility는 필드 이름으로 JSON 키를 찾으므로, 필드 이름을 바꾸면 JSON 키도 같이 바꿔야 한다
/// </remarks>
[System.Serializable]
public class jsonVideoInfo
{
    #region Serialized Fields

    [SerializeField] private string id;
    [SerializeField] private string clip;

    #endregion

    #region Properties

    /// <summary>
    /// 영상 id. 분기 id와 같은 값을 쓴다
    /// </summary>
    public string ID => id;

    /// <summary>
    /// 확장자를 뺀 VideoClip 이름
    /// </summary>
    public string Clip => clip;

    #endregion
}
