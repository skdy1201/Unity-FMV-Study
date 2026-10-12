using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 선택지 하나. 고르면 넘길 결과와 표시할 문구를 가진다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 필드 이름으로 JSON 키를 찾으므로, 필드 이름을 바꾸면 JSON 키도 같이 바꿔야 한다
    /// </remarks>
    [System.Serializable]
    public class JsonNodeOption
    {
        #region Serialized Fields

        [SerializeField] private NodeResult result;
        [SerializeField] private string label;
        // TODO: QTE(id 3)의 anchor는 임의 값이다. 화면에 직접 배치해 보면서 위치를 정한다
        [SerializeField] private Vector2 anchor;

        #endregion

        #region Properties

        /// <summary>
        /// 이 선택지를 고르면 넘길 결과
        /// </summary>
        /// <remarks>
        /// JSON에는 NodeResult의 숫자 값으로 적는다
        /// </remarks>
        public NodeResult Result => result;

        /// <summary>
        /// 버튼에 표시할 문구
        /// </summary>
        public string Label => label;

        /// <summary>
        /// QTE 입력을 띄울 화면 비율 좌표. (0, 0)이 왼쪽 아래, (1, 1)이 오른쪽 위
        /// </summary>
        /// <remarks>
        /// QTE 분기에서만 쓴다. Choice는 통일 UI에 배치하므로 무시한다
        /// </remarks>
        public Vector2 Anchor => anchor;

        #endregion
    }
}
