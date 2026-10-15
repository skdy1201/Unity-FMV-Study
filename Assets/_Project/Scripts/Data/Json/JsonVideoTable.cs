using UnityEngine;

namespace FMV
{
    /// <summary>
    /// videos.json 전체. 최상위 "videos" 배열을 받는다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 최상위 배열을 바로 읽지 못해서 배열을 감싸는 클래스를 둔다
    /// </remarks>
    [System.Serializable]
    public class JsonVideoTable
    {
        #region Serialized Fields

        [SerializeField] private JsonVideoInfo[] videos;

        #endregion

        #region Properties

        /// <summary>
        /// JSON에 적힌 순서대로의 영상 목록
        /// </summary>
        public JsonVideoInfo[] Videos => videos;

        #endregion
    }
}
