using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 업적 조회 목록 UI를 보이고 숨기고, 슬롯마다 업적 아이콘이나 잠금 이미지를 띄운다
    /// </summary>
    /// <remarks>
    /// 같은 오브젝트의 CanvasGroup으로 켜고 끈다.
    /// 슬롯은 AchievementTable에 입력한 업적 순서와 같은 순서로 Inspector에서 넣는다
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class AchievementListUIController : MonoBehaviour, IUIVisible
    {
        #region Serialized Fields

        [SerializeField] private List<Image> slotImages = new List<Image>();
        [SerializeField] private Sprite lockedSprite;

        #endregion

        #region Private Fields

        private CanvasGroup group;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();

            // 시작할 때는 숨겨 둔다
            Hide();
        }

        #endregion

        #region IUIVisible

        /// <summary>
        /// 조회 목록을 보이고 입력을 받게 한다
        /// </summary>
        public void Show()
        {
            SetGroupVisible(true);
        }

        /// <summary>
        /// 조회 목록을 숨기고 입력을 막는다
        /// </summary>
        public void Hide()
        {
            SetGroupVisible(false);
        }

        #endregion

        #region Achievement

        /// <summary>
        /// 업적 정의를 순서대로 돌며, 열린 업적은 아이콘을, 잠긴 업적은 잠금 이미지를 슬롯에 띄운다
        /// </summary>
        /// <param name="achievements"> AchievementTable의 업적 정의 목록 </param>
        /// <param name="unlockedIds"> save.json의 열린 업적 id 목록 </param>
        public void SetAchievements(IReadOnlyList<AchievementDefinition> achievements, List<string> unlockedIds)
        {
            for (int i = 0; i < achievements.Count; ++i)
            {
                // 슬롯보다 업적이 많으면 남은 업적은 띄울 자리가 없다
                if (i >= slotImages.Count)
                {
                    Debug.LogError($"Not enough achievement slots: {slotImages.Count} < {achievements.Count}");
                    return;
                }

                AchievementDefinition achievement = achievements[i];
                bool isUnlocked = unlockedIds.Contains(achievement.ID);

                slotImages[i].sprite = isUnlocked ? achievement.Icon : lockedSprite;
            }
        }

        #endregion

        #region Helpers

        /// <remarks>
        /// CanvasGroup으로 켜고 끄므로, 보임·입력·레이캐스트를 한 번에 맞춘다
        /// </remarks>
        private void SetGroupVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        #endregion
    }
}
