using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

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

        #region Unity Lifecycle

        private void Awake()
        {
            interfaceList = new List<ISetting> { gameData, gameState, clipConnect };

            foreach (ISetting set in interfaceList)
            {
                set.Init();
            }
        }

        private void Start()
        {
            GameEntry();           
        }

        private void OnDisable()
        {
            foreach (ISetting set in interfaceList)
            {
                set.End();
            }
        }

        #endregion

        #region Phase

        private void GameEntry()
        {
            Graph graph = gameData.Graph;

            // 세이브가 없는 첫 실행이면 NewestGrade가 빈 문자열이므로 시작 분기부터 진행한다
            string currentId = string.IsNullOrEmpty(gameState.NewestGrade) ? graph.Start.ID : gameState.NewestGrade;

            Node current = graph.FindNode(currentId);
            if (current == null)
            {
                Debug.LogError($"Node not found: {currentId}");
                return;
            }

            List<(string id, VideoClip clip)> clips = new List<(string id, VideoClip clip)>
            {
                (currentId, GetClipById(currentId))
            };

            foreach (NodeResult result in Enum.GetValues(typeof(NodeResult)))
            {
                string nextId = current.GetNext(result);
                if (nextId != null)
                {
                    clips.Add((nextId, GetClipById(nextId)));
                }
            }

            videoPlayer.PrepareNextClips(clips);
            videoPlayer.PlayClip(currentId);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// 분기 id로 VideoClip을 찾는다
        /// </summary>
        /// <remarks>
        /// 분기 id("2a")와 VideoClip 이름("2a_Bargainburg_FailToStart")은 다른 값이다.
        /// GameData가 id → 클립 이름, ClipConnect가 클립 이름 → VideoClip을 따로 맡아 서로를 모르므로,
        /// 두 파츠를 함께 들고 있는 컨트롤러가 이어 준다.
        /// 클립이 필요하면 ClipConnect.GetClip에 id를 넣지 말고 이 메서드를 쓴다
        /// </remarks>
        /// <param name="id"> 분기 id </param>
        /// <returns> 찾은 클립. id나 클립 이름이 없으면 null </returns>
        private VideoClip GetClipById(string id)
        {
            string clipName = gameData.FindClip(id);

            // Dictionary는 null 키를 받으면 예외를 던지므로, 이름을 못 찾았으면 ClipConnect에 넘기지 않는다
            return clipName != null ? clipConnect.GetClip(clipName) : null;
        }

        #endregion
    }
}
