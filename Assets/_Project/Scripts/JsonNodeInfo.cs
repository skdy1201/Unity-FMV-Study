using UnityEngine;

namespace FMV
{
    /// <summary>
    /// Action이 있는 분기 하나의 정보. nodes.json의 항목 하나에 해당한다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 필드 이름으로 JSON 키를 찾으므로, 필드 이름을 바꾸면 JSON 키도 같이 바꿔야 한다.
    /// nodes.json에 없는 분기는 Action 없이 클립이 끝나면 다음 분기로 넘어간다
    /// </remarks>
    [System.Serializable]
    public class JsonNodeInfo
    {
        #region Serialized Fields

        [SerializeField] private string id;
        [SerializeField] private string type;
        [SerializeField] private JsonNodeTime timing;
        [SerializeField] private NodeResult timeoutResult;
        [SerializeField] private JsonNodeOption[] options;

        #endregion

        #region Properties

        /// <summary>
        /// 분기 id. 영상 id와 같은 값을 쓴다
        /// </summary>
        public string ID => id;

        /// <summary>
        /// Action 종류. "Choice" 또는 "QTE"
        /// </summary>
        /// <remarks>
        /// JsonUtility는 enum을 숫자로만 읽으므로 문자열로 받는다
        /// </remarks>
        public string Type => type;

        /// <summary>
        /// Action 시작·반복 구간·제한 시간
        /// </summary>
        public JsonNodeTime Timing => timing;

        /// <summary>
        /// 제한 시간이 지났을 때 넘길 결과
        /// </summary>
        /// <remarks>
        /// Timing.TimeLimit이 0이면 쓰지 않는다
        /// </remarks>
        public NodeResult TimeoutResult => timeoutResult;

        /// <summary>
        /// JSON에 적힌 순서대로의 선택지 목록
        /// </summary>
        public JsonNodeOption[] Options => options;

        #endregion
    }
}
