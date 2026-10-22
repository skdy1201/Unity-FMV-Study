using System;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 업적 하나의 정보. achievements.json의 항목 하나에 해당한다
    /// </summary>
    /// <remarks>
    /// JsonUtility는 필드 이름으로 JSON 키를 찾으므로, 필드 이름을 바꾸면 JSON 키도 같이 바꿔야 한다.
    /// 열린 업적은 save.json에 id로만 남기고, 실행 중 열림 여부는 IsUnlocked에 둔다
    /// </remarks>
    [System.Serializable]
    public class JsonAchievementInfo
    {
        #region Serialized Fields

        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private string icon;
        [SerializeField] private string[] conditions;
        [SerializeField] private int requiredFallCount;
        [SerializeField] private string[] requiredEndings;

        #endregion

        #region Private Fields

        private bool isUnlocked;
        private AchievementCondition conditionFlags = AchievementCondition.None;

        #endregion

        #region Properties

        /// <summary>
        /// 업적 id
        /// </summary>
        public string ID => id;

        /// <summary>
        /// 업적 이름
        /// </summary>
        public string Title => title;

        /// <summary>
        /// 업적 설명. 잠긴 업적은 UI에서 가린다
        /// </summary>
        public string Description => description;

        /// <summary>
        /// 업적 아이콘 스프라이트를 찾는 키. 아이콘과 배경을 합친 이미지 한 장을 가리킨다
        /// </summary>
        public string Icon => icon;

        /// <summary>
        /// 업적이 열리려면 필요한 누적 실패 횟수. 0이면 실패 조건 없음
        /// </summary>
        public int RequiredFallCount => requiredFallCount;

        /// <summary>
        /// 업적이 열리려면 봐야 하는 엔딩 id 목록. endings.json의 id와 같은 값을 쓴다
        /// </summary>
        public string[] RequiredEndings => requiredEndings;

        /// <summary>
        /// 이 업적이 검사할 조건의 종류
        /// </summary>
        /// <remarks>
        /// ParseConditions를 부르기 전에는 None이다
        /// </remarks>
        public AchievementCondition Conditions => conditionFlags;

        /// <summary>
        /// 업적이 열렸으면 true
        /// </summary>
        /// <remarks>
        /// 실행 중 판정 결과라 achievements.json에서 읽지 않는다 (SerializeField 없음).
        /// 영구 기록은 save.json의 열린 업적 목록에 남긴다
        /// </remarks>
        public bool IsUnlocked
        {
            get => isUnlocked;
            set => isUnlocked = value;
        }

        #endregion

        #region Conditions

        /// <summary>
        /// achievements.json의 조건 이름 목록을 AchievementCondition으로 바꿔 Conditions에 담는다
        /// </summary>
        /// <remarks>
        /// JsonUtility는 enum을 숫자로만 읽어서, JSON에는 enum 이름을 문자열로 적고 여기서 바꾼다.
        /// GameData가 업적을 매핑할 때 한 번 부른다. 모르는 이름은 경고만 남기고 건너뛴다
        /// </remarks>
        public void ParseConditions()
        {
            conditionFlags = AchievementCondition.None;

            if (conditions == null)
            {
                return;
            }

            foreach (string name in conditions)
            {
                if (Enum.TryParse(name, out AchievementCondition condition))
                {
                    conditionFlags |= condition;
                }
                else
                {
                    Debug.LogWarning($"Unknown achievement condition: {name} ({id})");
                }
            }
        }

        #endregion
    }
}
