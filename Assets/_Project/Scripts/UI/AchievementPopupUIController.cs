using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 새로 열린 업적을 알리는 인게임 팝업 UI를 보이고 숨긴다
    /// </summary>
    /// <remarks>
    /// 같은 오브젝트의 CanvasGroup으로 켜고 끈다. 팝업 이미지는 화면 아래에 숨겨 두고, 이미지 높이만큼 올렸다가 내린다.
    /// 업적이 여러 개 열리면 겹쳐 띄울 수 있도록, popupImage는 원본으로만 쓰고 Popup마다 복제한 슬롯을 움직인다
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class AchievementPopupUIController : MonoBehaviour, IUIVisible
    {
        #region Serialized Fields

        // 슬롯을 복제할 원본. 직접 움직이지 않고 시작할 때 꺼 둔다
        [SerializeField] private Image popupImage;

        // 올라갔다 내려오기까지 걸리는 전체 시간(초). 절반은 올라가고 절반은 내려온다
        [SerializeField] private float popupDuration = 2f;

        [SerializeField] private UISound popupSound;

        #endregion

        #region Private Fields

        private CanvasGroup group;

        // 숨어 있는 위치. 모든 슬롯이 여기서 올라갔다가 여기로 돌아온다
        private Vector2 hiddenPosition;

        // 지금 떠 있는 슬롯 수. 새 슬롯은 이 수만큼 위로 쌓고, 0이 되면 팝업을 숨긴다
        private int activeCount;

        #endregion

        #region Properties

        /// <summary>
        /// 떠 있는 팝업 슬롯이 하나라도 있으면 true
        /// </summary>
        /// <remarks>
        /// 씬을 바꾸면 팝업도 함께 사라지므로, 씬을 바꾸기 전에 이 값이 false가 될 때까지 기다린다
        /// </remarks>
        public bool IsPlaying => activeCount > 0;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            hiddenPosition = popupImage.rectTransform.anchoredPosition;
            popupImage.gameObject.SetActive(false);

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
        /// 원본 이미지를 복제한 슬롯에 업적 이미지를 넣고 올렸다가 내린다
        /// </summary>
        /// <param name="sprite"> 팝업에 띄울 업적 이미지 </param>
        /// <returns> 팝업이 끝날 때까지 기다릴 수 있도록 실행 중인 코루틴을 돌려준다 (yield return으로 대기) </returns>
        /// <remarks>
        /// 앞 팝업이 떠 있는 동안 다시 부르면, 새 슬롯은 떠 있는 슬롯 수만큼 위에 멈춰 겹치지 않는다.
        /// 소리는 떠 있는 팝업이 없을 때 올라오는 첫 슬롯만 낸다
        /// </remarks>
        public Coroutine Popup(Sprite sprite)
        {
            Image slot = Instantiate(popupImage, popupImage.transform.parent);
            slot.sprite = sprite;
            slot.gameObject.SetActive(true);

            // 여러 업적이 짧은 간격으로 열리면 같은 소리가 겹쳐 지저분해지므로, 한 묶음에 한 번만 낸다
            if (activeCount == 0)
            {
                popupSound.Play();
            }

            int stackIndex = activeCount;
            activeCount++;

            Show();

            return StartCoroutine(PlayPopup(slot, stackIndex));
        }

        /// <remarks>
        /// 앞 절반·뒤 절반 모두 위쪽에서 느려지는 이징을 걸어, 빠르게 올라가 위에 오래 머물다 빠르게 내려온다.
        /// 매 프레임 이동량을 더하지 않고 지난 시간 비율로 위치를 정해, 프레임이 튀어도 끝에서 정확히 제자리로 돌아온다
        /// </remarks>
        private IEnumerator PlayPopup(Image slot, int stackIndex)
        {
            RectTransform rect = slot.rectTransform;

            // 아래 슬롯들 위에 쌓이도록, 순서만큼 이미지 높이를 더 올린다
            float riseHeight = rect.rect.height * (stackIndex + 1);
            float half = popupDuration / 2f;
            float time = 0f;

            while (time < popupDuration)
            {
                yield return null;

                time += Time.deltaTime;

                // 앞 절반은 0 → 1로 올라가고, 뒤 절반은 1 → 0으로 내려온다
                float ratio = (time < half) ? (time / half) : ((popupDuration - time) / half);
                rect.anchoredPosition = hiddenPosition + Vector2.up * (riseHeight * EaseOutQuint(ratio));
            }

            Destroy(slot.gameObject);
            activeCount--;

            // 다른 슬롯이 아직 떠 있으면 숨기지 않는다
            if (activeCount == 0)
            {
                Hide();
            }
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

        /// <remarks>
        /// 0~1 비율을 처음에 빠르고 1에 가까울수록 느리게 바꾼다 (easeOutQuint: 1 - (1 - t)^5).
        /// Mathf에는 없어 직접 둔다. 범위를 벗어난 값은 0~1로 자른다
        /// </remarks>
        private static float EaseOutQuint(float t)
        {
            float remain = 1f - Mathf.Clamp01(t);

            return 1f - remain * remain * remain * remain * remain;
        }

        #endregion
    }
}
