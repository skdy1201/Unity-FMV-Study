using UnityEngine;
using System.Collections.Generic;

namespace FMV
{
    /// <summary>
    /// videos.json을 읽어 영상 id와 VideoClip 이름을 매핑한다
    /// </summary>
    public class GameData : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private TextAsset videoJson;

        #endregion

        #region Private Fields

        private Dictionary<string, string> videoMapping = new Dictionary<string, string>();

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            JsonMapping();
        }

        #endregion

        #region Mapping

        /// <summary>
        /// JSON 데이터를 읽어 매핑 테이블을 만든다
        /// </summary>
        private void JsonMapping()
        {
            ClipIdMapping();
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

        #endregion
    }
}
