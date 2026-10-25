using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// save.json 기록을 들고, SaveFile로 불러오고 저장한다.
    /// 업적 정의를 들고, 저장 기록과 비교해 새로 열린 업적을 판정한다
    /// </summary>
    /// <remarks>
    /// 씬마다 하나씩 둔다. 메인 메뉴는 MainMenuController가 프리팹으로 생성하고, 플레이 씬은 여러 컨트롤러가 함께 쓰므로 씬에 배치해 참조로 받는다.
    /// 기록은 Awake에서 불러오므로, 다른 컨트롤러는 Start 이후에 SaveData를 받는다.
    /// 기록은 씬마다 새로 읽어 디스크와 맞추고, 다른 파츠는 이 객체가 가진 같은 JsonSaveData를 받아 쓴다.
    /// 업적은 플레이 씬 밖(타이틀)에서도 쓰므로, 씬마다 있는 이 객체가 정의와 판정을 함께 맡는다
    /// </remarks>
    public class SaveController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private AchievementTable achievementTable;

        #endregion

        #region Private Fields

        private JsonSaveData saveData;

        #endregion

        #region Properties

        /// <summary>
        /// save.json에서 불러온 저장 기록
        /// </summary>
        /// <remarks>
        /// 다른 파츠가 같은 객체를 바꿔야 저장에 반영되므로, 객체 자체를 바꾸는 set은 두지 않는다
        /// </remarks>
        public JsonSaveData SaveData => saveData;

        /// <summary>
        /// AchievementTable에 입력한 업적 정의 목록
        /// </summary>
        /// <remarks>
        /// 에셋의 읽기 전용 목록을 그대로 내보낸다
        /// </remarks>
        public IReadOnlyList<AchievementDefinition> Achievements => achievementTable.Achievements;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Load();
        }

        #endregion

        #region Save

        /// <summary>
        /// save.json을 읽어 저장 기록을 새로 만든다
        /// </summary>
        /// <remarks>
        /// 이전에 받아 둔 SaveData 참조는 새 기록과 달라지므로, 다시 불렀다면 SaveData를 다시 받아야 한다
        /// </remarks>
        public void Load()
        {
            saveData = SaveFile.Load();
        }

        /// <summary>
        /// 지금 들고 있는 저장 기록을 save.json에 덮어쓴다
        /// </summary>
        public void Save()
        {
            SaveFile.Save(saveData);
        }

        #endregion

        #region Achievement

        /// <summary>
        /// 아직 열리지 않은 업적의 조건을 저장 기록과 비교해, 새로 열린 업적을 찾는다.
        /// 새로 열린 업적은 열린 업적 목록에 넣고, 호출한 쪽에 돌려준다
        /// </summary>
        /// <remarks>
        /// 저장은 하지 않으므로, 호출한 쪽이 Save로 남긴다.
        /// 이벤트로 알리지 않고 반환하는 이유는, 호출한 쪽이 팝업을 다 띄울 때까지 기다린 뒤 씬을 바꿔야 하기 때문이다
        /// </remarks>
        /// <returns> 이번에 새로 열린 업적 목록. 없으면 빈 목록 </returns>
        public List<AchievementDefinition> CheckAchievement()
        {
            List<string> unlocked = saveData.UnlockedAchievements;
            List<AchievementDefinition> newlyUnlocked = new List<AchievementDefinition>();

            foreach (AchievementDefinition achievement in achievementTable.Achievements)
            {
                // 이미 열린 업적은 새로 알릴 것이 없다
                if (unlocked.Contains(achievement.ID))
                {
                    continue;
                }

                if (!IsConditionMet(achievement))
                {
                    continue;
                }

                unlocked.Add(achievement.ID);

                LogAchievement(achievement);

                newlyUnlocked.Add(achievement);
            }

            return newlyUnlocked;
        }

        /// <remarks>
        /// 업적이 가진 조건을 모두 만족해야 true. 조건이 하나도 없는 업적은 잘못 적힌 것으로 보고 열지 않는다
        /// </remarks>
        private bool IsConditionMet(AchievementDefinition achievement)
        {
            AchievementCondition conditions = achievement.Conditions;

            if (conditions == AchievementCondition.None)
            {
                return false;
            }

            if (conditions.HasFlag(AchievementCondition.FallCount) && !CheckFallCount(achievement))
            {
                return false;
            }

            if (conditions.HasFlag(AchievementCondition.Ending) && !CheckEndings(achievement))
            {
                return false;
            }

            return true;
        }

        private bool CheckFallCount(AchievementDefinition achievement)
        {
            return saveData.Progress.FallCount >= achievement.RequiredFallCount;
        }

        /// <remarks>
        /// 엔딩 id는 분기 id와 같아서, 방문한 분기 목록으로 본 엔딩을 확인한다
        /// </remarks>
        private bool CheckEndings(AchievementDefinition achievement)
        {
            foreach (string endingId in achievement.RequiredEndings)
            {
                if (!saveData.Progress.Visited.Contains(endingId))
                {
                    return false;
                }
            }

            return true;
        }

        /// <remarks>
        /// 달성 팝업 UI를 붙이기 전에 판정 결과를 콘솔로 확인하기 위한 임시 출력
        /// </remarks>
        private void LogAchievement(AchievementDefinition achievement)
        {
            Debug.Log($"[Achievement] {achievement.Title}\n{achievement.Description}");
        }

        #endregion
    }
}
