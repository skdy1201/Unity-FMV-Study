using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 최대 단계와 실패 횟수를 들고 있고 PlayerPrefs로 저장·불러오기를 한다
    /// </summary>
    /// <remarks>
    /// GameController가 소유하는 일반 클래스라 씬 오브젝트 없이 쓴다
    /// </remarks>
    public class GameState
    {
        #region Private Fields

        private const string MaxGradeKey = "maxGrade";
        private const string FailCountKey = "failCount";

        private int maxGrade = 0;
        private int failCount = 0;

        #endregion

        #region Properties

        /// <summary>
        /// 지금까지 도달한 최대 단계
        /// </summary>
        public int MaxGrade => maxGrade;

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
        /// 키가 없으면 0이 나오므로 세이브가 없는 첫 실행도 0으로 시작한다
        /// </remarks>
        public void Init()
        {
            maxGrade = PlayerPrefs.GetInt(MaxGradeKey);
            failCount = PlayerPrefs.GetInt(FailCountKey);
        }

        /// <summary>
        /// 현재 기록을 PlayerPrefs에 쓴다
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetInt(MaxGradeKey, maxGrade);
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
        /// 단계를 1 올린다
        /// </summary>
        /// <remarks>
        /// 지금은 누적값이다. 최대 단계(영상 기준 5) 제한과 이번 판 단계와의 비교는 아직 없다
        /// </remarks>
        public void GradeUp()
        {
            // TODO: 이번 판 단계를 따로 두고 maxGrade = Mathf.Max(maxGrade, currentGrade)로 갱신
            ++maxGrade;
        }

        /// <summary>
        /// 실패 횟수를 1 올린다
        /// </summary>
        public void AddFail()
        {
            ++failCount;
        }

        #endregion
    }
}
