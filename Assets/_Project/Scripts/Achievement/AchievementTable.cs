using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 업적 정의 전체. 인스펙터에서 입력한다
    /// </summary>
    /// <remarks>
    /// 정의는 바뀌지 않으므로 여러 씬이 같은 에셋을 참조해도 된다.
    /// 에디터에서는 플레이 중에 바꾼 ScriptableObject 값이 그대로 남으므로, 외부에는 읽기 전용으로만 노출한다
    /// </remarks>
    [CreateAssetMenu(fileName = "AchievementTable", menuName = "FMV/Achievement Table")]
    public class AchievementTable : ScriptableObject
    {
        #region Serialized Fields

        [SerializeField] private List<AchievementDefinition> achievements = new List<AchievementDefinition>();

        #endregion

        #region Properties

        /// <summary>
        /// 인스펙터에 입력한 순서대로의 업적 목록
        /// </summary>
        public IReadOnlyList<AchievementDefinition> Achievements => achievements;

        #endregion
    }
}
