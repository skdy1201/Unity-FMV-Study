using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기도 원본 데이터. 인스펙터에서 Edge 목록으로 입력한다
    /// </summary>
    /// <remarks>
    /// 실행 시에는 new Graph(cladogram)으로 변환해서 쓴다.
    /// 에디터에서는 플레이 중에 바꾼 ScriptableObject 값이 그대로 남으므로, 외부에는 읽기 전용으로만 노출한다
    /// </remarks>
    [CreateAssetMenu(fileName = "Cladogram", menuName = "FMV/Cladogram")]
    public class Cladogram : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private string startID;
        [SerializeField] private List<Edge> edges = new List<Edge>();

        #endregion

        #region Properties

        /// <summary>
        /// 시작 분기 id
        /// </summary>
        public string StartID => startID;

        /// <summary>
        /// 분기 연결 목록
        /// </summary>
        public IReadOnlyList<Edge> Edges => edges;

        #endregion
    }
}
