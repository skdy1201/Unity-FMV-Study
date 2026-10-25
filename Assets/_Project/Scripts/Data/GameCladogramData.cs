using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기도 화면에만 쓰는 데이터를 들고 있는 파츠. cards.json을 읽어 분기 id별 카드 정보를 매핑한다
    /// </summary>
    /// <remarks>
    /// 게임 진행에 필요한 데이터는 GameData가, 분기도를 보여 주는 데만 쓰는 데이터는 여기서 맡는다
    /// </remarks>
    [System.Serializable]
    public class GameCladogramData : ISetting
    {
        #region Serialized Fields

        [SerializeField] private TextAsset cardJson;

        #endregion

        #region Private Fields

        private Dictionary<string, JsonCardInfo> cardMapping = new Dictionary<string, JsonCardInfo>();

        #endregion

        #region Properties

        /// <summary>
        /// 분기 id를 키로 한 카드 정보. Init 이후에 사용할 수 있다
        /// </summary>
        /// <remarks>
        /// 읽기 전용으로 내보내므로, 밖에서는 TryGetValue로 조회만 한다
        /// </remarks>
        public IReadOnlyDictionary<string, JsonCardInfo> CardMapping => cardMapping;

        #endregion

        #region ISetting

        /// <summary>
        /// JSON으로 카드 매핑 테이블을 만든다
        /// </summary>
        public void Init()
        {
            CardInfoMapping();
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
        /// 분기 id를 키로, 카드 정보를 값으로 등록한다
        /// </summary>
        /// <remarks>
        /// 같은 id가 여러 번 있으면 뒤에 나온 값으로 덮어쓴다
        /// </remarks>
        private void CardInfoMapping()
        {
            JsonCardTable table = JsonUtility.FromJson<JsonCardTable>(cardJson.text);

            foreach (JsonCardInfo card in table.Cards)
            {
                cardMapping[card.ID] = card;
            }
        }

        #endregion
    }
}
