using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 인스펙터에 등록한 썸네일 Sprite를 이름으로 찾아 주는 조회 테이블
    /// </summary>
    /// <remarks>
    /// 키는 Sprite 에셋 이름(sprite.name)이다. 이름이 겹치면 목록에서 뒤에 있는 Sprite가 남는다.
    /// JSON에는 Sprite를 넣을 수 없어 cards.json에는 이름만 적고, 이름을 Sprite로 바꾸는 일은 여기서 한다
    /// </remarks>
    [System.Serializable]
    public class ThumbnailConnect : ISetting
    {
        #region Serialized Fields

        [SerializeField] private List<Sprite> thumbnails = new List<Sprite>();

        #endregion

        #region Private Fields

        private Dictionary<string, Sprite> thumbnailMap = new Dictionary<string, Sprite>();

        #endregion

        #region ISetting

        public void Init()
        {
            foreach (Sprite thumbnail in thumbnails)
            {
                thumbnailMap[thumbnail.name] = thumbnail;
            }
        }

        public void End()
        {
        }

        #endregion

        #region Lookup

        /// <summary>
        /// 썸네일 이름에 해당하는 Sprite를 반환
        /// </summary>
        /// <remarks>
        /// 분기 id("2a")가 아니라 cards.json의 썸네일 이름("2a_Bargainburg_FailToStart")을 받는다.
        /// null을 넣으면 예외가 난다
        /// </remarks>
        /// <param name="thumbnailName"> 찾을 Sprite 에셋 이름 </param>
        /// <returns> 찾은 Sprite, 없으면 null </returns>
        public Sprite GetThumbnail(string thumbnailName)
        {
            return thumbnailMap.TryGetValue(thumbnailName, out Sprite thumbnail) ? thumbnail : null;
        }

        #endregion
    }
}
