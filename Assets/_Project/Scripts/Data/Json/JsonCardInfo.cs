using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기도 카드 하나의 분기 id, 썸네일 이름, 제목. cards.json의 항목 하나에 해당한다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 필드 이름으로 JSON 키를 찾으므로, 필드 이름을 바꾸면 JSON 키도 같이 바꿔야 한다
    /// </remarks>
    [System.Serializable]
    public class JsonCardInfo
    {
        #region Serialized Fields

        [SerializeField] private string id;
        [SerializeField] private string thumbnail;
        [SerializeField] private string title;

        #endregion

        #region Properties

        /// <summary>
        /// 카드가 나타내는 분기 id
        /// </summary>
        public string ID => id;

        /// <summary>
        /// 확장자를 뺀 썸네일 Sprite 이름
        /// </summary>
        public string Thumbnail => thumbnail;

        /// <summary>
        /// 카드에 띄울 제목
        /// </summary>
        public string Title => title;

        #endregion
    }
}
