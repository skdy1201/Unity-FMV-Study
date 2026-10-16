using UnityEngine;

namespace FMV
{
    /// <summary>
    /// endings.json 전체. 최상위 "endings" 배열을 받는다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 최상위 배열을 바로 읽지 못해서 배열을 감싸는 클래스를 둔다
    /// </remarks>
    [System.Serializable]
    public class JsonEndingTable
    {
        #region Serialized Fields

        [SerializeField] private JsonEndingInfo[] endings;

        #endregion

        #region Properties

        /// <summary>
        /// JSON에 적힌 순서대로의 엔딩 목록
        /// </summary>
        public JsonEndingInfo[] Endings => endings;

        #endregion
    }
}
