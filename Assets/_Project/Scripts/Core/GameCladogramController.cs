using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기도 화면을 보이고 숨기고, GameController와 분기도 사이를 잇는다
    /// </summary>
    /// <remarks>
    /// 분기도 Canvas에 붙이고, 같은 오브젝트의 CanvasGroup으로 켜고 끈다.
    /// 분기도에만 쓰는 파츠는 ISetting으로 묶어 Awake에서 Init, OnDisable에서 End를 같은 순서로 호출한다
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class GameCladogramController : MonoBehaviour, IUIVisible
    {
        #region Serialized Fields

        [Header("Parts")]
        [SerializeField] private GameCladogramData cladogramData = new GameCladogramData();
        [SerializeField] private ThumbnailConnect thumbnailConnect = new ThumbnailConnect();

        #endregion

        #region Private Fields

        private CanvasGroup group;
        private List<ISetting> interfaceList;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();

            interfaceList = new List<ISetting> { cladogramData, thumbnailConnect };

            foreach (ISetting set in interfaceList)
            {
                set.Init();
            }
        }

        private void OnDisable()
        {
            foreach (ISetting set in interfaceList)
            {
                set.End();
            }
        }

        #endregion

        #region IUIVisible

        /// <summary>
        /// 분기도를 보이고 입력을 받게 한다
        /// </summary>
        public void Show()
        {
            SetGroupVisible(true);
        }

        /// <summary>
        /// 분기도를 숨기고 입력을 막는다
        /// </summary>
        public void Hide()
        {
            SetGroupVisible(false);
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
