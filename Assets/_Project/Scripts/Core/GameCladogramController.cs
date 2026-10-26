using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기도 데이터를 준비하고, GameController와 분기도 화면 사이를 잇는다
    /// </summary>
    /// <remarks>
    /// 화면을 켜고 끄는 일은 CladogramUIController가 맡고, 여기서는 참조로 들고 호출만 한다.
    /// 분기도에만 쓰는 파츠는 ISetting으로 묶어 Awake에서 Init, OnDisable에서 End를 같은 순서로 호출한다
    /// </remarks>
    public class GameCladogramController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Parts")]
        [SerializeField] private GameCladogramData cladogramData = new GameCladogramData();
        [SerializeField] private ThumbnailConnect thumbnailConnect = new ThumbnailConnect();

        [Header("UI")]
        [SerializeField] private CladogramUIController cladogramUI;

        #endregion

        #region Private Fields

        private List<ISetting> interfaceList;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
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
    }
}
