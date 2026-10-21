using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 플레이어의 진행 기록. save.json에 저장한다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 필드 이름으로 JSON 키를 찾으므로, 필드 이름을 바꾸면 기존 save.json의 값을 읽지 못한다.
    /// 분기는 모두 분기 id("2a" 같은 값)로 들고 있어 Graph·GameData와 같은 값을 쓴다
    /// </remarks>
    [System.Serializable]
    public class JsonProgressInfo
    {
        #region Serialized Fields

        [SerializeField] private List<string> visited = new List<string>();
        [SerializeField] private int fallCount = 0;
        [SerializeField] private string newestId = string.Empty;
        [SerializeField] private string lastSelect = string.Empty;
        [SerializeField] private string maxGradeId = string.Empty;

        #endregion
    }
}
