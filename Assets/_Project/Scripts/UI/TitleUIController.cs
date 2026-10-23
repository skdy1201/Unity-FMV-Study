using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 메인 메뉴의 타이틀 화면(타이틀 이미지·메뉴 버튼) Canvas를 보이고 숨긴다
    /// </summary>
    /// <remarks>
    /// 같은 오브젝트의 CanvasGroup으로 켜고 끈다. 버튼 연결은 MainMenuController가 생성한 뒤 맡는다
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class TitleUIController : MonoBehaviour, IUIVisible
    {
        #region Private Fields

        private CanvasGroup group;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        #endregion

        #region IUIVisible

        /// <summary>
        /// 타이틀 화면을 보이고 입력을 받게 한다
        /// </summary>
        public void Show()
        {
            SetGroupVisible(true);
        }

        /// <summary>
        /// 타이틀 화면을 숨기고 입력을 막는다
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
