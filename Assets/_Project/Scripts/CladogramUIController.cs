using UnityEngine;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 분기도 화면 UI를 보이고 숨기고, 배경을 띄우고, 카드를 생성한다
    /// </summary>
    /// <remarks>
    /// 분기도 Canvas에 붙이고, 같은 오브젝트의 CanvasGroup으로 켜고 끈다.
    /// 배경 Sprite는 Inspector에서 넣고 Awake에서 배경 Image에 적용한다.
    /// 씬에 들어오면 분기도가 먼저 나오므로 보이는 상태로 시작한다. 언제 끌지는 바깥 컨트롤러가 정하고, 여기서는 Show/Hide 호출만 받는다
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class CladogramUIController : MonoBehaviour, IUIVisible
    {
        #region Serialized Fields

        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite backgroundSprite;

        [Header("Card")]
        [SerializeField] private CladogramCard cardPrefab;
        [SerializeField] private RectTransform cardContent;

        #endregion

        #region Private Fields

        private CanvasGroup group;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            backgroundImage.sprite = backgroundSprite;
        }

        #endregion

        #region IUIVisible

        /// <summary>
        /// 분기도를 보이고 입력을 받게 한다
        /// </summary>
        public void Show()
        {
            SetGroupVisible(true);
        }

        /// <summary>
        /// 분기도를 숨기고 입력을 막는다
        /// </summary>
        public void Hide()
        {
            SetGroupVisible(false);
        }

        #endregion

        #region Card

        /// <summary>
        /// 카드 프리팹을 카드 영역(Content) 아래에 생성한다
        /// </summary>
        /// <remarks>
        /// 값(id, 썸네일)과 위치는 채우지 않은 채로 돌려주므로, 부른 쪽에서 채운다
        /// </remarks>
        /// <returns> 생성한 카드 </returns>
        public CladogramCard CreateCard()
        {
            return Instantiate(cardPrefab, cardContent);
        }

        #endregion

        #region Helpers

        /// <remarks>
        /// CanvasGroup으로 켜고 끄므로, 보임·입력·레이캐스트를 한 번에 맞춘다
        /// </remarks>
        private void SetGroupVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        #endregion
    }
}
