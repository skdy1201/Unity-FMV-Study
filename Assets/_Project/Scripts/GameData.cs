using UnityEngine;
using System.Collections.Generic;

namespace FMV
{
    /// <summary>
    /// videos.json과 nodes.json을 읽어 영상 id별 VideoClip 이름과 Action 정보를 매핑하고, Cladogram으로 분기 Graph를 만든다
    /// </summary>
    [System.Serializable]
    public class GameData : ISetting
    {
        #region Serialized Fields

        [SerializeField] private TextAsset videoJson;
        [SerializeField] private TextAsset nodeJson;
        [SerializeField] private Cladogram cladogram;

        #endregion

        #region Private Fields

        private Dictionary<string, string> videoMapping = new Dictionary<string, string>();
        private Dictionary<string, JsonNodeInfo> nodeMapping = new Dictionary<string, JsonNodeInfo>();
        private Graph cladoGraph;

        #endregion

        #region ISetting

        public void Init()
        {
            JsonMapping();

            cladoGraph = new Graph(cladogram);
        }

        public void End()
        {
        }

        #endregion

        #region Mapping

        /// <summary>
        /// JSON 데이터를 읽어 매핑 테이블을 만든다
        /// </summary>
        private void JsonMapping()
        {
            ClipIdMapping();
            NodeInfoMapping();
        }

        /// <summary>
        /// 영상 id를 키로, VideoClip 이름을 값으로 등록한다
        /// </summary>
        /// <remarks>
        /// 같은 id가 여러 번 있으면 뒤에 나온 값으로 덮어쓴다
        /// </remarks>
        private void ClipIdMapping()
        {
            JsonVideoTable table = JsonUtility.FromJson<JsonVideoTable>(videoJson.text);

            foreach (JsonVideoInfo info in table.Videos)
            {
                videoMapping[info.ID] = info.Clip;
            }

            // DEBUG: 매핑 결과 확인용
            foreach (KeyValuePair<string, string> pair in videoMapping)
            {
                Debug.Log($"{pair.Key} → {pair.Value}");
            }
        }

        /// <summary>
        /// 분기 id를 키로, Action 정보를 값으로 등록한다
        /// </summary>
        /// <remarks>
        /// nodes.json에는 Action이 있는 분기만 적으므로, 등록되지 않은 분기는 Action이 없는 분기다.
        /// 같은 id가 여러 번 있으면 뒤에 나온 값으로 덮어쓴다
        /// </remarks>
        private void NodeInfoMapping()
        {
            JsonNodeTable table = JsonUtility.FromJson<JsonNodeTable>(nodeJson.text);

            foreach (JsonNodeInfo node in table.Nodes)
            {
                nodeMapping[node.ID] = node;
            }
        }

        #endregion

        #region Lookup

        /// <summary>
        /// JSON 클립이름 매칭 테이블에서 ID를 통해 클립이름을 찾는다
        /// </summary>
        /// <param name="id"> 클립 ID </param>
        /// <returns> 클립 이름 </returns>
        public string FindClip(string id)
        {
            if(!videoMapping.TryGetValue(id, out string  clip))
            {
                Debug.LogError($"{id}'s Clip search fail");     
            }
            
            return clip;
        }

        /// <summary>
        /// 분기 id로 Action 정보를 찾는다
        /// </summary>
        /// <remarks>
        /// nodes.json에 없는 분기는 Action이 없는 정상 분기이므로 에러를 남기지 않는다
        /// </remarks>
        /// <param name="id"> 분기 id </param>
        /// <param name="info"> 찾은 Action 정보. 없으면 null </param>
        /// <returns> Action이 있는 분기면 true </returns>
        public bool TryGetNodeInfo(string id, out JsonNodeInfo info)
        {
            return nodeMapping.TryGetValue(id, out info);
        }

        #endregion
    }
}
