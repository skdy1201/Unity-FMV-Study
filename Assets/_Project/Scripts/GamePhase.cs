namespace FMV
{
    /// <summary>
    /// 분기 하나 안에서의 진행 단계
    /// </summary>
    public enum PhaseState
    {
        Entry = 0,
        Play = 1,
        Action = 2,
        Out = 3,
        End = 4,
    }

    /// <summary>
    /// 현재 진행 단계와 마지막 분기 결과를 기록한다
    /// </summary>
    /// <remarks>
    /// 단계에 따른 동작은 GameController가 직접 맡고, 여기서는 값만 저장하고 돌려준다
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

        #region Transition

        /// <summary>
        /// 현재 단계와 분기 결과를 기록한다
        /// </summary>
        /// <param name="next"> 바꿀 단계 </param>
        /// <param name="result"> 분기가 끝난 결과. Out으로 바꿀 때 넘긴다 </param>
        public void SetPhase(PhaseState next, NodeResult result = NodeResult.Finished)
        {
            current = next;
            lastResult = result;
        }

        #endregion
    }
}
