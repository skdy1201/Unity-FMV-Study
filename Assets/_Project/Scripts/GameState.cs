using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 현재 단계, 최대 단계, 실패 횟수를 들고 있고 PlayerPrefs로 저장·불러오기를 한다
    /// </summary>
    /// <remarks>
    /// GameController가 소유하는 일반 클래스라 씬 오브젝트 없이 쓴다.
    /// 단계는 분기 id("2a" 같은 값)로 들고 있어 Graph·GameData와 같은 값을 쓴다
    /// </remarks>
    public class GameState
    {
        #region Private Fields

        private const string MaxGradeKey = "maxGrade";
        private const string FailCountKey = "failCount";
        private const string NewestGradeKey = "newestGrade";

        private string maxGrade = string.Empty;
        private string newestGrade = string.Empty;
        private int failCount = 0;

        #endregion

        #region Properties

        /// <summary>
        /// 지금까지 가장 멀리 간 분기 id. 기록이 없으면 빈 문자열
        /// </summary>
        public string MaxGrade => maxGrade;

        /// <summary>
        /// 이번 판에서 마지막으로 도달한 분기 id. 기록이 없으면 빈 문자열
        /// </summary>
        public string NewestGrade => newestGrade;

        /// <summary>
        /// 누적 실패 횟수
        /// </summary>
        public int FailCount => failCount;

        #endregion

        #region Save

        /// <summary>
        /// 저장된 기록을 불러온다
        /// </summary>
        /// <remarks>
        /// 키가 없으면 숫자는 0, 문자열은 빈 문자열이 나오므로 세이브가 없는 첫 실행도 그 값으로 시작한다
        /// </remarks>
        public void Init()
        {
            maxGrade = PlayerPrefs.GetString(MaxGradeKey);
            newestGrade = PlayerPrefs.GetString(NewestGradeKey);
            failCount = PlayerPrefs.GetInt(FailCountKey);
        }

        /// <summary>
        /// 현재 기록을 PlayerPrefs에 쓴다
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetString(MaxGradeKey, maxGrade);
            PlayerPrefs.SetString(NewestGradeKey, newestGrade);
            PlayerPrefs.SetInt(FailCountKey, failCount);
            PlayerPrefs.Save();
        }

        #endregion

        #region Progress

        /// <summary>
        /// 게임이 끝날 때 호출해 기록을 저장한다
        /// </summary>
        public void End()
        {
            Save();
        }

        /// <summary>
        /// 현재 단계를 도달한 분기 id로 바꾸고, 최대 단계보다 앞선 단계면 최대 단계도 갱신한다
        /// </summary>
        /// <remarks>
        /// 같은 단계 안의 갈래("2a", "2b")는 앞서고 뒤서는 관계가 없으므로, 먼저 기록된 쪽을 유지한다
        /// </remarks>
        /// <param name="id"> 도달한 분기 id </param>
        public void UpdateGrade(string id)
        {
            newestGrade = id;

            if (ParseStage(id) > ParseStage(maxGrade))
            {
                maxGrade = id;
            }
        }

        /// <summary>
        /// 실패 횟수를 1 올린다
        /// </summary>
        public void AddFail()
        {
            ++failCount;
        }

        #endregion

        #region Helpers

        /// <remarks>
        /// "2a"는 2, "10"은 10처럼 앞쪽 숫자만 읽는다
        /// </remarks>
        /// <returns> 숫자로 시작하지 않으면 0 </returns>
        private static int ParseStage(string id)
        {
            int stage = 0;

            foreach (char c in id)
            {
                if (!char.IsDigit(c))
                {
                    break;
                }

                stage = stage * 10 + (c - '0');
            }

            return stage;
        }

        #endregion
    }
}
