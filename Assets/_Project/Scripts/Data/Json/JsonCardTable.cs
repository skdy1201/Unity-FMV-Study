using UnityEngine;

namespace FMV
{
    /// <summary>
    /// cards.json 전체. 최상위 "cards" 배열을 받는다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 최상위 배열을 바로 읽지 못해서 배열을 감싸는 클래스를 둔다
    /// </remarks>
    [System.Serializable]
    public class JsonCardTable
    {
        #region Serialized Fields

        [SerializeField] private JsonCardInfo[] cards;

        #endregion

        #region Properties

        /// <summary>
        /// JSON에 적힌 순서대로의 카드 목록
        /// </summary>
        public JsonCardInfo[] Cards => cards;

        #endregion
    }
}
