using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 타이밍 원을 제한 시간 동안 줄이고, 클릭한 순간 타겟 원과 크기가 맞는지 판정한다
    /// </summary>
    /// <remarks>
    /// 크기는 localScale만 바꾸고 rect는 그대로 두므로, 두 원의 rect 비율은 시작할 때 한 번만 구한다
    /// </remarks>
    public class QTEClick : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Image targetImage;
        [SerializeField] private Image timingImage;

        // 성공 범위. 타이밍 원이 타겟 원의 몇 배 크기인지로 적는다 (예: 1.2 / 0.8)
        [SerializeField] private float outbound;
        [SerializeField] private float inbound;

        [SerializeField] private Vector3 timingScale = Vector3.one;

        #endregion

        #region Private Fields

        // 배율 1일 때 타이밍 원이 타겟보다 몇 배 큰지. localScale만 바뀌므로 QTE 동안 변하지 않는다
        private float sizeRatio;

        private Coroutine shrinkRoutine;

        #endregion

        #region Unity Lifecycle

        // 다른 오브젝트의 Start에서 ActiveQTE를 불러도 처음 크기와 비율이 준비돼 있도록 Awake에서 구한다
        private void Awake()
        {
            timingScale = timingImage.rectTransform.localScale;

            float targetWidth = targetImage.rectTransform.rect.width;
            if (targetWidth <= 0f)
            {
                Debug.LogError($"Target image width is zero: {name}");
            }
            else
            {
                sizeRatio = timingImage.rectTransform.rect.width / targetWidth;
            }

            // 원 위를 눌러도 GameInput이 UI 클릭으로 걸러 내지 않도록 레이캐스트를 끈다
            targetImage.raycastTarget = false;
            timingImage.raycastTarget = false;
        }

        #endregion

        #region QTE

        /// <summary>
        /// 지금 타이밍 원의 크기가 성공 범위 안에 있는지 판정한다
        /// </summary>
        /// <remarks>
        /// 화면에 보이는 크기(rect × localScale) 기준으로 비교하므로, 두 원의 이미지 크기가 달라도 된다
        /// </remarks>
        /// <returns> 타이밍 원이 타겟 원의 inbound ~ outbound배 사이면 true </returns>
        public bool QTECheck()
        {
            float current = sizeRatio * timingImage.rectTransform.localScale.x / targetImage.rectTransform.localScale.x;

            return (current <= outbound) && (current >= inbound);
        }

        /// <summary>
        /// 타이밍 원을 처음 크기로 되돌리고 제한 시간 동안 줄이기 시작한다
        /// </summary>
        /// <param name="duration"> 제한 시간(초) </param>
        /// <remarks>
        /// 이미 줄어들고 있으면 멈추고 새로 시작해, 코루틴 두 개가 같은 원을 동시에 줄이지 않게 한다
        /// </remarks>
        public void ActiveQTE(float duration)
        {
            if (shrinkRoutine != null)
            {
                StopCoroutine(shrinkRoutine);
            }

            shrinkRoutine = StartCoroutine(ShrinkTiming(duration));
        }

        /// <remarks>
        /// 마지막 프레임에 time이 음수가 될 수 있어, 배율이 뒤집히지 않도록 0~1로 자른다
        /// </remarks>
        private IEnumerator ShrinkTiming(float duration)
        {
            float time = duration;
            Vector3 maxScale = timingScale;

            while (time > 0)
            {
                yield return null;

                time -= Time.deltaTime;
                Vector3 nowScale = maxScale * Mathf.Clamp01(time / duration);
                timingImage.rectTransform.localScale = nowScale;
            }

            shrinkRoutine = null;
        }

        #endregion
    }
}
