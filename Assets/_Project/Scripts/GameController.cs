using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 게임 데이터·상태·클립 조회 같은 파츠를 들고, 씬 시작과 끝에 한꺼번에 초기화·정리한다
    /// </summary>
    /// <remarks>
    /// 파츠는 ISetting으로 묶어 Awake에서 Init, OnDisable에서 End를 같은 순서로 호출한다
    /// </remarks>
    public class GameController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private GameData gameData = new GameData();
        [SerializeField] private ClipConnect clipConnect = new ClipConnect();

        [SerializeField] private VideoPlayerScript videoPlayer;

        #endregion

        #region Private Fields

        private GameState gameState = new GameState();

        private List<ISetting> interfaceList;

        #endregion

        #region Properties

        /// <summary>
        /// videos.json에서 읽은 영상 id와 클립 이름 매핑
        /// </summary>
        public GameData GameData => gameData;

        /// <summary>
        /// 현재·최고 단계와 실패 횟수를 저장하는 상태
        /// </summary>
        public GameState GameState => gameState;

        /// <summary>
        /// 클립 이름으로 VideoClip을 찾아 주는 조회 테이블
        /// </summary>
        public ClipConnect ClipConnect => clipConnect;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            interfaceList = new List<ISetting> { gameData, gameState, clipConnect };

            foreach (ISetting set in interfaceList)
            {
                set.Init();
            }
        }

        private void Update()
        {
            
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
