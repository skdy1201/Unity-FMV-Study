using System;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기 연결 하나. "startID가 result로 끝나면 nextID로 간다"
    /// </summary>
    /// <remarks>
    /// Cladogram 인스펙터 입력용이다. 필드 이름을 바꾸면 에셋에 저장된 값이 끊기니 FormerlySerializedAs를 같이 쓴다
    /// </remarks>
    [Serializable]
    public class Edge
    {
        #region Serialized Fields

        [SerializeField] private NodeResult result;
        [SerializeField] private string startID;
        [SerializeField] private string nextID;

        #endregion

        #region Properties

        /// <summary>
        /// 출발 분기가 끝난 결과
        /// </summary>
        public NodeResult Result => result;

        /// <summary>
        /// 출발 분기 id
        /// </summary>
        public string StartID => startID;

        /// <summary>
        /// 도착 분기 id
        /// </summary>
        public string NextID => nextID;

        #endregion
    }
}
