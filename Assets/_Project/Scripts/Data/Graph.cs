using System.Collections.Generic;

namespace FMV
{
    /// <summary>
    /// Cladogram 데이터로 만든 분기도. id로 분기를 찾고 다음 분기를 알려준다
    /// </summary>
    /// <remarks>
    /// Edge 순서와 상관없이 같은 결과가 나온다.
    /// Dictionary는 Unity가 저장하지 못하므로 에셋에 두지 않고, 실행 시 매번 새로 만든다
    /// </remarks>
    public class Graph
    {
        #region Private Fields

        private readonly Dictionary<string, Node> nodes = new Dictionary<string, Node>();

        #endregion

        #region Properties

        /// <summary>
        /// 시작 분기
        /// </summary>
        public Node Start { get; }

        #endregion

        #region Build

        /// <param name="data"> 분기 연결 정보가 담긴 에셋 </param>
        public Graph(Cladogram data)
        {
            foreach (Edge edge in data.Edges)
            {
                GetOrAdd(edge.StartID).Connect(edge.Result, edge.NextID);

                // 엔딩처럼 출발하는 Edge가 없는 분기도 등록되도록 도착 쪽도 확보한다
                GetOrAdd(edge.NextID);
            }

            Start = GetOrAdd(data.StartID);
        }

        private Node GetOrAdd(string id)
        {
            if (!nodes.TryGetValue(id, out Node node))
            {
                node = new Node(id);
                nodes.Add(id, node);
            }

            return node;
        }

        #endregion

        #region Query

        /// <summary>
        /// id로 분기를 찾음
        /// </summary>
        /// <returns> 없으면 null </returns>
        public Node FindNode(string id)
        {
            return nodes.TryGetValue(id, out Node node) ? node : null;
        }

        /// <summary>
        /// 현재 분기와 결과로 다음 분기를 찾음
        /// </summary>
        /// <param name="currentId"> 현재 분기 id </param>
        /// <param name="result"> 현재 분기가 끝난 결과 </param>
        /// <returns> 이어지는 분기가 없으면 null (엔딩) </returns>
        public Node GetNext(string currentId, NodeResult result)
        {
            Node current = FindNode(currentId);
            if (current == null)
            {
                return null;
            }

            string nextId = current.GetNext(result);
            return nextId != null ? FindNode(nextId) : null;
        }

        #endregion
    }
}
