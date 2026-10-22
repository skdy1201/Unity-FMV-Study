using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 업적 조회 목록 UI를 보이고 숨긴다
    /// </summary>
    /// <remarks>
    /// 같은 오브젝트의 CanvasGroup으로 켜고 끈다. 목록 내용 채우기는 다음 단계에서 붙인다
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class AchievementListUIController : MonoBehaviour, IUIVisible
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

        #region IUIVisible

        /// <summary>
        /// 조회 목록을 보이고 입력을 받게 한다
        /// </summary>
        public void Show()
        {
            SetGroupVisible(true);
        }

        /// <summary>
        /// 조회 목록을 숨기고 입력을 막는다
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
