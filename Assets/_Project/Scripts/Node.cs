using System.Collections.Generic;

namespace FMV
{
    /// <summary>
    /// 분기도의 분기 하나. 결과별로 다음 분기 id를 가진다
    /// </summary>
    /// <remarks>
    /// 다음 분기를 객체가 아닌 id로 들고 있어서, 연결 시점에 다음 분기가 아직 없어도 된다
    /// </remarks>
    public class Node
    {
        #region Private Fields

        private readonly string id;
        private readonly Dictionary<NodeResult, string> nextIds = new Dictionary<NodeResult, string>();

        #endregion

        #region Properties

        /// <summary>
        /// 분기 id
        /// </summary>
        public string ID => id;

        /// <summary>
        /// 이어지는 분기가 없으면 엔딩
        /// </summary>
        public bool IsEnd => nextIds.Count == 0;

        #endregion

        #region Connection

        /// <param name="id"> 분기 id </param>
        public Node(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// 결과에 따라 이어질 분기를 등록
        /// </summary>
        /// <remarks>
        /// 같은 결과가 이미 있으면 덮어쓴다
        /// </remarks>
        /// <param name="result"> 이 분기가 끝난 결과 </param>
        /// <param name="nextId"> 이어질 분기 id </param>
        public void Connect(NodeResult result, string nextId)
        {
            nextIds[result] = nextId;
        }

        /// <summary>
        /// 결과에 해당하는 다음 분기 id를 반환
        /// </summary>
        /// <returns> 연결이 없으면 null </returns>
        public string GetNext(NodeResult result)
        {
            return nextIds.TryGetValue(result, out string nextId) ? nextId : null;
        }

        #endregion
    }
}
