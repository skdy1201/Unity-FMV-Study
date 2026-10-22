using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 새로 열린 업적을 알리는 인게임 팝업 UI를 보이고 숨긴다
    /// </summary>
    /// <remarks>
    /// 같은 오브젝트의 CanvasGroup으로 켜고 끈다. 표시할 업적 내용·연출은 다음 단계에서 붙인다
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class AchievementPopupUIController : MonoBehaviour, IShowHide
    {
        #region Private Fields

        private CanvasGroup group;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();

            // 시작할 때는 숨겨 둔다
            Hide();
        }

        #endregion

        #region IShowHide

        /// <summary>
        /// 팝업을 보인다
        /// </summary>
        public void Show()
        {
            SetGroupVisible(true);
        }

        /// <summary>
        /// 팝업을 숨긴다
        /// </summary>
        public void Hide()
        {
            SetGroupVisible(false);
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
