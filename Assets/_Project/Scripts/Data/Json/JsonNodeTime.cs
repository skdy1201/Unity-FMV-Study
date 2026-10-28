using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기 하나의 Action 시간 정보(초)
    /// </summary>
    /// <remarks>
    /// JsonUtility는 필드 이름으로 JSON 키를 찾으므로, 필드 이름을 바꾸면 JSON 키도 같이 바꿔야 한다
    /// </remarks>
    [System.Serializable]
    public class JsonNodeTime
    {
        #region Serialized Fields

        [SerializeField] private float actionStart;
        [SerializeField] private float loopStart;
        [SerializeField] private float loopEnd;
        [SerializeField] private float timeLimit;
        [SerializeField] private float speed;

        #endregion

        #region Properties

        /// <summary>
        /// Entry에서 Action으로 넘어가는 클립 시간
        /// </summary>
        public float ActionStart => actionStart;

        /// <summary>
        /// 선택을 기다리며 반복할 구간의 시작
        /// </summary>
        public float LoopStart => loopStart;

        /// <summary>
        /// 선택을 기다리며 반복할 구간의 끝
        /// </summary>
        /// <remarks>
        /// LoopStart 이하이면 반복하지 않는다
        /// </remarks>
        public float LoopEnd => loopEnd;

        /// <summary>
        /// 선택 제한 시간
        /// </summary>
        /// <remarks>
        /// 0이면 제한 없이 기다린다
        /// </remarks>
        public float TimeLimit => timeLimit;

        /// <summary>
        /// Action 구간의 재생 배속
        /// </summary>
        /// <remarks>
        /// JsonUtility는 JSON에 없는 숫자 필드를 0으로 채우므로, 0 이하이면 GameController가 기본 슬로 배속을 쓴다
        /// </remarks>
        public float Speed => speed;

        #endregion
    }
}
