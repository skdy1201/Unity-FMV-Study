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

        // TODO: QTE(id 3)의 actionStart·timeLimit은 임의 값이다. 영상을 보면서 실제 구간으로 맞춘다
        // TODO: QTE는 슬로모션으로 처리한다. 배속을 담을 speed 필드를 추가하고,
        //       (클립 길이 - actionStart) / speed가 timeLimit보다 길도록 값을 맞춘다
        [SerializeField] private float actionStart;
        [SerializeField] private float loopStart;
        [SerializeField] private float loopEnd;
        [SerializeField] private float timeLimit;

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

        #endregion
    }
}
