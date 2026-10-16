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

        // DEBUG: TestScene에서 Start로 바로 돌려 보려고 쓰는 제한 시간
        [SerializeField] private float testDuration = 5f;

        #endregion

        #region Private Fields

        // DEBUG: GameController와 같은 입력 경로로 클릭을 받아, 원 위 클릭이 UI로 걸러지지 않는지도 함께 확인한다
        private GameInput testInput = new GameInput();

        #endregion

        #region Unity Lifecycle

        // DEBUG: 테스트용 자동 시작. ActionUIController에서 ActiveQTE를 부르게 되면 지운다
        private void Start()
        {
            ActiveQTE(testDuration);
        }

        // DEBUG: 테스트용 클릭 판정. GameController.HandleInput에서 CheckQTE를 부르게 되면 지운다
        private void Update()
        {
            if (testInput.IsScreenClicked())
            {
                Debug.Log(CheckQTE() ? "QTE Success" : "QTE Fail");
            }
        }

        #endregion

        #region QTE

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

        #endregion
    }
}
