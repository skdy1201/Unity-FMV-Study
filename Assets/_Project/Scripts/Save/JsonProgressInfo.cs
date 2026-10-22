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

        #region Properties

        /// <summary>
        /// 지금까지 들어간 분기 id 목록
        /// </summary>
        /// <remarks>
        /// 목록 자체를 바꾸면 null이 들어갈 수 있어 set을 두지 않는다. 내용은 Add로 바꾼다
        /// </remarks>
        public List<string> Visited => visited;

        /// <summary>
        /// 누적 실패 횟수
        /// </summary>
        public int FallCount
        {
            get => fallCount;
            set => fallCount = value;
        }

        /// <summary>
        /// 마지막으로 도달한 분기 id. 기록이 없으면 빈 문자열
        /// </summary>
        public string NewestId
        {
            get => newestId;
            set => newestId = value;
        }

        /// <summary>
        /// 최근에 들어간 Action(선택지·QTE) 분기 id. 배드 엔딩으로 끝나면 이 분기로 되돌아간다
        /// </summary>
        public string LastSelect
        {
            get => lastSelect;
            set => lastSelect = value;
        }

        /// <summary>
        /// 지금까지 가장 멀리 간 분기 id. 기록이 없으면 빈 문자열
        /// </summary>
        public string MaxGradeId
        {
            get => maxGradeId;
            set => maxGradeId = value;
        }

        #endregion
    }
}
