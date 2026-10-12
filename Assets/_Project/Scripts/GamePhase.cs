using System;

namespace FMV
{
    /// <summary>
    /// 분기 하나 안에서의 진행 단계
    /// </summary>
    public enum PhaseState
    {
        Entry = 0,
        Action = 1,
        Out = 2,
        End = 3,
    }
    /// <summary>
    /// 현재 진행 단계와 마지막 분기 결과를 들고, 단계가 바뀌면 이벤트로 알린다
    /// </summary>
    /// <remarks>
    /// 단계에 따른 동작은 PhaseChange를 구독한 GameController가 맡고, 여기서는 단계만 기록한다
    /// </remarks>
    public class GamePhase
    {
        #region Private Fields

        private PhaseState current = PhaseState.Entry;
        private NodeResult lastResult = NodeResult.Finished;

        #endregion

        #region Properties

        /// <summary>
        /// 현재 진행 단계
        /// </summary>
        public PhaseState Current => current;

        /// <summary>
        /// 마지막으로 단계를 바꿀 때 넘긴 분기 결과
        /// </summary>
        /// <remarks>
        /// Out에서 다음 분기를 찾을 때 쓴다
        /// </remarks>
        public NodeResult LastResult => lastResult;

        #endregion

        #region Events

        /// <summary>
        /// 단계가 바뀐 뒤 바뀐 단계를 넘겨 호출된다
        /// </summary>
        public event Action<PhaseState> PhaseChange;

        #endregion

        #region Transition

        /// <summary>
        /// 단계를 바꾸고 PhaseChange를 호출한다
        /// </summary>
        /// <param name="next"> 바꿀 단계 </param>
        /// <param name="result"> 분기가 끝난 결과. Out으로 바꿀 때 넘긴다 </param>
        public void ChangePhase(PhaseState next, NodeResult result = NodeResult.Finished)
        {
            current = next;
            lastResult = result;

            PhaseChange?.Invoke(current);
        }

        #endregion
    }
}
