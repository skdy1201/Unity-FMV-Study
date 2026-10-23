using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 새로 열린 업적을 알리는 인게임 팝업 UI를 보이고 숨긴다
    /// </summary>
    /// <remarks>
    /// 같은 오브젝트의 CanvasGroup으로 켜고 끈다. 팝업 이미지는 화면 아래에 숨겨 두고, 이미지 높이만큼 올렸다가 내린다
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class AchievementPopupUIController : MonoBehaviour, IUIVisible
    {
        #region Serialized Fields

        [SerializeField] private Image popupImage;

        // 올라갔다 내려오기까지 걸리는 전체 시간(초). 절반은 올라가고 절반은 내려온다
        [SerializeField] private float popupDuration = 2f;

        #endregion

        #region Private Fields

        private CanvasGroup group;

        // 숨어 있는 위치. 팝업이 끝나면 여기로 돌아온다
        private Vector2 hiddenPosition;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            hiddenPosition = popupImage.rectTransform.anchoredPosition;

            // 시작할 때는 숨겨 둔다
            Hide();
        }

        #endregion

        #region IUIVisible

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

        #region Popup

        /// <summary>
        /// 팝업 이미지를 바꾸고 올렸다가 내린다
        /// </summary>
        /// <param name="sprite"> 팝업에 띄울 업적 이미지 </param>
        /// <returns> 팝업이 끝날 때까지 기다릴 수 있도록 실행 중인 코루틴을 돌려준다 (yield return으로 대기) </returns>
        public Coroutine Popup(Sprite sprite)
        {
            popupImage.sprite = sprite;

            return StartCoroutine(PlayPopup());
        }

        /// <remarks>
        /// 매 프레임 이동량을 더하지 않고 지난 시간 비율로 위치를 정해, 프레임이 튀어도 끝에서 정확히 제자리로 돌아온다
        /// </remarks>
        private IEnumerator PlayPopup()
        {
            RectTransform rect = popupImage.rectTransform;
            float height = rect.rect.height;
            float half = popupDuration / 2f;
            float time = 0f;

            Show();

            while (time < popupDuration)
            {
                yield return null;

                time += Time.deltaTime;

                // 앞 절반은 0 → 1로 올라가고, 뒤 절반은 1 → 0으로 내려온다
                float ratio = (time < half) ? (time / half) : ((popupDuration - time) / half);
                rect.anchoredPosition = hiddenPosition + Vector2.up * (height * Mathf.Clamp01(ratio));
            }

            rect.anchoredPosition = hiddenPosition;
            Hide();
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
