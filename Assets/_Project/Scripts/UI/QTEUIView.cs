using System;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// QTE UI 프리팹의 타이머와 타이밍 원을 함께 시작하고, 클릭 판정을 넘긴다
    /// </summary>
    /// <remarks>
    /// 타이머와 타이밍 원은 같은 제한 시간으로 동시에 시작하는 형제라, 서로 모르게 두고 이 컴포넌트가 둘을 직접 부른다
    /// </remarks>
    public class QTEUIView : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private QTETimer qteTimer;
        [SerializeField] private QTEClick qteClick;

        #endregion

        #region Private Fields

        private Vector2 qtePosition;

        #endregion

        #region Events

        /// <summary>
        /// 제한 시간이 다 되어 타이머가 끝나면 호출된다
        /// </summary>
        public event Action OnTimeOut;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            qteTimer.OnTimerEnd += TimerEnd;
        }

        #endregion

        #region QTE

        /// <summary>
        /// 노드마다 다른 QTE 위치를 받아 둔다
        /// </summary>
        /// <param name="position"> 노드 정보에 적힌 QTE 위치 </param>
        public void SetQTEPosition(Vector2 position)
        {
            qtePosition = position;
        }

        /// <summary>
        /// 타이머와 타이밍 원을 같은 제한 시간으로 동시에 시작한다
        /// </summary>
        /// <param name="duration"> 제한 시간(초) </param>
        public void ActiveQTE(float duration)
        {
            qteTimer.ActiveTimer(duration);
            qteClick.ActiveQTE(duration);
        }

        /// <summary>
        /// 지금 타이밍 원의 크기가 성공 범위 안에 있는지 판정한다
        /// </summary>
        /// <returns> 성공 범위 안이면 true </returns>
        public bool CheckQTE()
        {
            return qteClick.QTECheck();
        }

        /// <remarks>
        /// 타이머는 프리팹 안쪽 파츠라 ActionUIController가 직접 참조하지 않으므로, 이 컴포넌트가 받아 그대로 넘긴다
        /// </remarks>
        private void TimerEnd()
        {
            OnTimeOut?.Invoke();
        }

        #endregion
    }
}
