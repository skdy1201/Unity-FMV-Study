using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 엔딩 분기 하나의 정보. endings.json의 항목 하나에 해당한다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 필드 이름으로 JSON 키를 찾으므로, 필드 이름을 바꾸면 JSON 키도 같이 바꿔야 한다.
    /// 막다른 분기는 모두 endings.json에 적는다. 체크포인트로 이어지는 배드 엔딩도 엔딩으로 기록하기 위함이다
    /// </remarks>
    [System.Serializable]
    public class JsonEndingInfo
    {
        #region Serialized Fields

        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private bool isFinal;

        #endregion

        #region Properties

        /// <summary>
        /// 엔딩 분기 id. 영상 id와 같은 값을 쓴다
        /// </summary>
        public string ID => id;

        /// <summary>
        /// 엔딩 이름
        /// </summary>
        public string Title => title;

        /// <summary>
        /// 엔딩 설명
        /// </summary>
        public string Description => description;

        /// <summary>
        /// 게임이 완전히 끝나는 엔딩이면 true. false면 최근 선택지 분기로 되돌아가 이어서 진행한다
        /// </summary>
        public bool IsFinal => isFinal;

        #endregion
    }
}
