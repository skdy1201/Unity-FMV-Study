using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 분기도에 놓이는 카드 한 장. 분기 id를 가지고 썸네일과 제목을 띄운다
    /// </summary>
    /// <remarks>
    /// 분기도가 코드로 생성한 뒤 setter로 값을 넣는다. 썸네일과 제목은 따로 들고 있지 않고 카드의 Image와 텍스트에 바로 띄운다
    /// </remarks>
    public class CladogramCard : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private string id;

        [Header("UI")]
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private TMP_Text titleText;

        #endregion

        #region Setter

        /// <summary>
        /// 카드가 가리킬 분기 id를 정한다
        /// </summary>
        public void SetId(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// 카드에 썸네일과 제목을 띄운다
        /// </summary>
        /// <param name="thumbnail"> 썸네일 Image에 띄울 Sprite </param>
        /// <param name="title"> 제목 텍스트 </param>
        public void SetThumbnail(Sprite thumbnail, string title)
        {
            thumbnailImage.sprite = thumbnail;
            titleText.text = title;
        }

        #endregion
    }
}
