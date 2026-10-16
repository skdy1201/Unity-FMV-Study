using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 제한 시간 동안 타이머 바를 줄이고, 시간이 다 되면 알린다
    /// </summary>
    public class QTETimer : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Image timerBar;

        [SerializeField] private Image timerAnimationImage;
        [SerializeField] private Sprite[] frames;
        // 제한 시간 동안 넘길 총 프레임 수. 10장 기준 20이면 두 바퀴 돈다
        [SerializeField] private int flipCount = 20;

        #endregion

        #region Private Fields

        private Coroutine timerRoutine;
        private Coroutine animationRoutine;

        #endregion

        #region Events

        /// <summary>
        /// 제한 시간이 다 되어 타이머 바가 비면 호출된다
        /// </summary>
        public event Action OnTimerEnd;

        #endregion

        #region Timer

        /// <summary>
        /// 타이머 바를 가득 채우고 제한 시간 동안 줄이기 시작한다
        /// </summary>
        /// <param name="duration"> 제한 시간(초) </param>
        /// <remarks>
        /// 이미 돌고 있는 타이머가 있으면 멈추고 새로 시작해, 코루틴 두 개가 같은 바를 동시에 줄이지 않게 한다
        /// </remarks>
        public void ActiveTimer(float duration)
        {
            if (timerRoutine != null)
            {
                StopCoroutine(timerRoutine);
                StopCoroutine(animationRoutine);
            }

            timerRoutine = StartCoroutine(OnTimer(duration));
            animationRoutine = StartCoroutine(PlayFlipbook(duration));
        }

        /// <remarks>
        /// fillAmount는 0~1 비율이라 남은 시간 / 제한 시간을 그대로 넣는다.
        /// 마지막 프레임에 time이 음수가 될 수 있어, 루프가 끝나면 바를 0으로 맞춘 뒤 OnTimerEnd를 호출한다
        /// </remarks>
        private IEnumerator OnTimer(float duration)
        {
            float time = duration;
            timerBar.fillAmount = 1f;

            while (time > 0f)
            {
                yield return null;

                time -= Time.deltaTime;
                timerBar.fillAmount = time / duration;
            }

            timerBar.fillAmount = 0f;
            timerRoutine = null;

            StopCoroutine(animationRoutine);
            timerAnimationImage.sprite = frames[0];

            OnTimerEnd?.Invoke();
        }

        /// <remarks>
        /// 제한 시간을 flipCount로 나눠 프레임 간격을 정해, 제한 시간이 달라도 같은 횟수만큼 넘어가게 한다
        /// </remarks>
        private IEnumerator PlayFlipbook(float duration)
        {
            int index = 0;
            var wait = new WaitForSeconds(duration / flipCount);

            while (true)
            {
                timerAnimationImage.sprite = frames[index];
                index = (index + 1) % frames.Length;
                yield return wait;
            }
        }

        #endregion
    }
}
