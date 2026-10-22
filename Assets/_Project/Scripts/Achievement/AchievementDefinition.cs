using System;
using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 업적 하나의 정의. AchievementTable 인스펙터에서 입력한다
    /// </summary>
    /// <remarks>
    /// 바뀌지 않는 정의 데이터만 둔다. 열림 여부는 save.json의 열린 업적 목록만 관리한다.
    /// 필드 이름을 바꾸면 에셋에 저장된 값이 끊기니 FormerlySerializedAs를 같이 쓴다
    /// </remarks>
    [Serializable]
    public class AchievementDefinition
    {
        #region Serialized Fields

        [SerializeField] private string id;
        [SerializeField] private string title;
        [SerializeField] private string description;
        [SerializeField] private Sprite icon;
        [SerializeField] private AchievementCondition conditions;
        [SerializeField] private int requiredFallCount;
        [SerializeField] private List<string> requiredEndings = new List<string>();

        #endregion

        #region Properties

        /// <summary>
        /// 업적 id. save.json의 열린 업적 목록에 이 값이 남는다
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
        /// 업적 아이콘. 아이콘과 배경을 합친 이미지 한 장이다
        /// </summary>
        public Sprite Icon => icon;

        /// <summary>
        /// 이 업적이 검사할 조건의 종류
        /// </summary>
        /// <remarks>
        /// Flags enum이라 인스펙터에서 여러 개를 함께 고를 수 있다
        /// </remarks>
        public AchievementCondition Conditions => conditions;

        /// <summary>
        /// 업적이 열리려면 필요한 누적 실패 횟수. 0이면 실패 조건 없음
        /// </summary>
        public int RequiredFallCount => requiredFallCount;

        /// <summary>
        /// 업적이 열리려면 봐야 하는 엔딩 id 목록. endings.json의 id와 같은 값을 쓴다
        /// </summary>
        public IReadOnlyList<string> RequiredEndings => requiredEndings;

        #endregion
    }
}
