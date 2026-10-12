using UnityEngine;

namespace FMV
{
    /// <summary>
    /// nodes.json 전체. 최상위 "nodes" 배열을 받는다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 최상위 배열을 바로 읽지 못해서 배열을 감싸는 클래스를 둔다
    /// </remarks>
    [System.Serializable]
    public class JsonNodeTable
    {
        #region Serialized Fields

        [SerializeField] private JsonNodeInfo[] nodes;

        #endregion

        #region Properties

        /// <summary>
        /// JSON에 적힌 순서대로의 분기 목록
        /// </summary>
        public JsonNodeInfo[] Nodes => nodes;

        #endregion
    }
}
