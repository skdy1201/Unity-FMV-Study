using UnityEngine;
using System.Collections.Generic;

namespace FMV
{
    /// <summary>
    /// videos.json, nodes.json, endings.json, achievements.json을 읽어 영상 id별 VideoClip 이름, Action 정보, 엔딩 정보, 업적 정보를 매핑하고, Cladogram으로 분기 Graph를 만든다.
    /// save.json의 저장 기록도 불러온다
    /// </summary>
    [System.Serializable]
    public class GameData : ISetting
    {
        #region Serialized Fields

        [SerializeField] private TextAsset videoJson;
        [SerializeField] private TextAsset nodeJson;
        [SerializeField] private TextAsset endingJson;
        [SerializeField] private TextAsset achievementJson;
        [SerializeField] private Cladogram cladogram;

        #endregion

        #region Private Fields

        private Dictionary<string, string> videoMapping = new Dictionary<string, string>();
        private Dictionary<string, JsonNodeInfo> nodeMapping = new Dictionary<string, JsonNodeInfo>();
        private Dictionary<string, JsonEndingInfo> endingMapping = new Dictionary<string, JsonEndingInfo>();
        private Dictionary<string, JsonAchievementInfo> achievementMapping = new Dictionary<string, JsonAchievementInfo>();
        private Graph cladoGraph;
        private JsonSaveData gameSave;

        #endregion

        #region Properties

        /// <summary>
        /// Cladogram으로 만든 분기도. Init 이후에 사용할 수 있다
        /// </summary>
        public Graph Graph => cladoGraph;

        /// <summary>
        /// save.json에서 불러온 저장 기록. Init 이후에 사용할 수 있다
        /// </summary>
        /// <remarks>
        /// GameController가 같은 객체를 GameState에 넘기고, 기록을 바꾸고 저장하는 일은 GameState가 맡는다
        /// </remarks>
        public JsonSaveData Save => gameSave;

        #endregion

        #region ISetting

        /// <summary>
        /// JSON 매핑 테이블과 분기 Graph를 만든다
        /// </summary>
        public void Init()
        {
            JsonMapping();

            cladoGraph = new Graph(cladogram);
        }

        /// <summary>
        /// 정리할 것이 없어 비워 둔다
        /// </summary>
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
            EndingInfoMapping();

            // 업적 매핑이 열린 업적 목록을 보므로 기록을 먼저 불러온다
            gameSave = SaveFile.Load();
            AchievementInfoMapping();
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

        /// <summary>
        /// 엔딩 분기 id를 키로, 엔딩 정보를 값으로 등록한다
        /// </summary>
        /// <remarks>
        /// 같은 id가 여러 번 있으면 뒤에 나온 값으로 덮어쓴다
        /// </remarks>
        private void EndingInfoMapping()
        {
            JsonEndingTable table = JsonUtility.FromJson<JsonEndingTable>(endingJson.text);

            foreach (JsonEndingInfo ending in table.Endings)
            {
                endingMapping[ending.ID] = ending;
            }
        }

        /// <summary>
        /// 업적 id를 키로, 업적 정보를 값으로 등록한다.
        /// 조건 이름을 파싱하고, save.json에 열린 업적으로 남아 있으면 IsUnlocked를 켠다
        /// </summary>
        /// <remarks>
        /// 같은 id가 여러 번 있으면 뒤에 나온 값으로 덮어쓴다.
        /// gameSave를 보므로 SaveFile.Load 뒤에 불러야 한다
        /// </remarks>
        private void AchievementInfoMapping()
        {
            JsonAchievementTable table = JsonUtility.FromJson<JsonAchievementTable>(achievementJson.text);

            foreach (JsonAchievementInfo achievement in table.Achievements)
            {
                achievement.ParseConditions();
                achievement.IsUnlocked = gameSave.UnlockedAchievements.Contains(achievement.ID);
                achievementMapping[achievement.ID] = achievement;
            }
        }

        #endregion

        #region Lookup

        /// <summary>
        /// 영상 id로 VideoClip 이름을 찾는다
        /// </summary>
        /// <param name="id"> 영상 id </param>
        /// <returns> VideoClip 이름. 없으면 null </returns>
        public string FindClip(string id)
        {
            if (!videoMapping.TryGetValue(id, out string clip))
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

        /// <summary>
        /// 분기 id로 엔딩 정보를 찾는다
        /// </summary>
        /// <remarks>
        /// endings.json에 없는 분기는 엔딩이 아니므로 에러를 남기지 않는다
        /// </remarks>
        /// <param name="id"> 분기 id </param>
        /// <param name="info"> 찾은 엔딩 정보. 없으면 null </param>
        /// <returns> 엔딩 분기면 true </returns>
        public bool TryGetEndingInfo(string id, out JsonEndingInfo info)
        {
            return endingMapping.TryGetValue(id, out info);
        }

        /// <summary>
        /// 업적 id로 업적 정보를 찾는다
        /// </summary>
        /// <param name="id"> 업적 id </param>
        /// <param name="info"> 찾은 업적 정보. 없으면 null </param>
        /// <returns> achievements.json에 있는 업적이면 true </returns>
        public bool TryGetAchievementInfo(string id, out JsonAchievementInfo info)
        {
            return achievementMapping.TryGetValue(id, out info);
        }

        #endregion
    }
}
