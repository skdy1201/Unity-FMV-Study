using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// save.json 전체. 진행 기록과 열린 업적 id 목록을 들고 있다
    /// </summary>
    /// <remarks>
    /// 업적 정의(제목·설명·조건)는 achievements.json에 있으므로 여기에는 id만 남긴다.
    /// 필드 초기값은 save.json이 없는 첫 실행의 기본값이 된다
    /// </remarks>
    [System.Serializable]
    public class JsonSaveData
    {
        #region Serialized Fields

        [SerializeField] private JsonProgressInfo progress = new JsonProgressInfo();
        [SerializeField] private List<string> unlockedAchievements = new List<string>();

        #endregion

        #region Properties

        /// <summary>
        /// 진행 기록
        /// </summary>
        public JsonProgressInfo Progress => progress;

        /// <summary>
        /// 지금까지 열린 업적 id 목록. achievements.json의 id와 같은 값을 쓴다
        /// </summary>
        /// <remarks>
        /// 이 목록에 없던 업적이 판정에서 열리면 새로 달성한 업적이다
        /// </remarks>
        public List<string> UnlockedAchievements => unlockedAchievements;

        #endregion
    }
}
