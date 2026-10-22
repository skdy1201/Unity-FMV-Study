using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
        [SerializeField] private GameObject mediaInteractUI;
        [FormerlySerializedAs("actionUIPrefab")]
        [SerializeField] private GameObject selectUIPrefab;
        [SerializeField] private GameObject qteUIPrefab;
        [SerializeField] private ActionUIController actionUIController;

        #endregion

        #region Private Fields

        private const float DefaultActionSpeed = 0.5f;

        private GameState gameState = new GameState();
        private GameInput gameInput = new GameInput();

        private ButtonVisual mediaVisual;
        private List<ISetting> interfaceList;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            LoadSave();

            interfaceList = new List<ISetting> { gameData, gameState, clipConnect };

            foreach (ISetting set in interfaceList)
            {
                set.Init();
            }

            videoPlayer.ClipEnd += ClipEnd;
            videoPlayer.OnActionReached += EnterAction;

            ConnectMediaButton();
            ConnectActionUIController();
        }

        private void Start()
        {
            GameEntry();
        }

        private void Update()
        {
            HandleInput();
        }

        private void OnDisable()
        {
            foreach (ISetting set in interfaceList)
            {
                set.End();
            }
        }

        #endregion

        #region Save

        /// <summary>
        /// GameData가 save.json을 불러오게 하고, 같은 기록을 GameState에 넘긴다
        /// </summary>
        /// <remarks>
        /// ISetting.Init은 인자를 받지 않고 GameData의 업적 매핑이 기록을 보므로, Init보다 먼저 부른다
        /// </remarks>
        private void LoadSave()
        {
            gameData.LoadSave();
            gameState.GameSave = gameData.Save;
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

            gameState.UpdateGrade(currentId);

            videoPlayer.PrepareNextClips(clips);
            videoPlayer.PlayClip(currentId);

            gameState.GamePhase.SetPhase(PhaseState.Play);

            // nodes.json에 없는 분기는 Action 없이 클립 끝까지 재생하므로 기다리지 않는다
            if (gameData.TryGetNodeInfo(currentId, out JsonNodeInfo info))
            {
                gameState.CheckpointGrade = currentId;
                videoPlayer.WaitActionTime(info.Timing.ActionStart);
            }

            // 분기에 들어갈 때마다 저장해, 중간에 꺼져도 이 분기부터 다시 시작한다
            gameState.Save();
        }

        /// <summary>
        /// 클립이 ActionStart에 도달하면 Action 단계로 바꾸고, 노드 타입에 맞게 반복 구간과 UI를 켠다
        /// </summary>
        /// <remarks>
        /// 비디오는 시간만 알리고, 어떤 Action인지는 여기서 노드 정보로 판단한다
        /// </remarks>
        private void EnterAction()
        {
            if (!gameData.TryGetNodeInfo(gameState.NewestGrade, out JsonNodeInfo info))
            {
                return;
            }

            gameState.GamePhase.SetPhase(PhaseState.Action);

            JsonNodeTime timing = info.Timing;

            float speed = timing.Speed > 0f ? timing.Speed : DefaultActionSpeed;

            // LoopEnd가 LoopStart 이하이면 반복하지 않는 분기다
            if (timing.LoopEnd > timing.LoopStart)
            {
                videoPlayer.SetLoopTime(new Vector2(timing.LoopStart, timing.LoopEnd), speed);
            }

            // Play 단계에서 켜 둔 미디어 버튼이 Action 중에도 눌려 영상을 멈추지 않도록 숨긴다
            mediaVisual.SetVisible(false);

            switch (info.Type)
            {
                case "Choice":
                    actionUIController.ActiveSelectUI(info.Options);
                    break;
                case "QTE":
                    // QTE는 반복 구간이 없어 SetLoopTime을 타지 않으므로 슬로 배속을 따로 건다
                    videoPlayer.SetActionSpeed(speed);
                    actionUIController.ActiveQTEUI(timing.TimeLimit);
                    break;
                default:
                    Debug.LogError($"Unknown action type: {info.Type}");
                    break;
            }
        }

        /// <summary>
        /// 선택지를 고르면 Out 단계로 바꾸고, 고른 결과의 다음 분기로 넘어간다
        /// </summary>
        /// <remarks>
        /// 클립 끝까지 기다리지 않고 바로 넘어가며, 다음 분기 진입은 클립이 끝났을 때와 같은 ClipEnd 흐름을 탄다.
        /// 반복 구간은 다음 클립의 PlayClip이 해제한다
        /// </remarks>
        /// <param name="result"> 고른 선택지의 결과 </param>
        private void ExitAction(NodeResult result)
        {
            gameState.GamePhase.SetPhase(PhaseState.Out, result);
            ClipEnd(result);
        }

        /// <summary>
        /// QTE 판정 결과를 성공·실패 분기 결과로 바꿔 Action을 끝낸다
        /// </summary>
        /// <param name="isSuccess"> 성공 범위 안에서 클릭했으면 true </param>
        private void QTEChecked(bool isSuccess)
        {
            ExitAction(isSuccess ? NodeResult.Success : NodeResult.Fail);
        }

        /// <summary>
        /// QTE 제한 시간 안에 판정하지 못하면, 노드에 적힌 시간 초과 결과로 Action을 끝낸다
        /// </summary>
        private void QTETimeOut()
        {
            if (gameData.TryGetNodeInfo(gameState.NewestGrade, out JsonNodeInfo info))
            {
                ExitAction(info.TimeoutResult);
            }
        }

        private void ClipEnd(NodeResult result)
        {
            string currentId = gameState.NewestGrade;
            string nextId = gameData.Graph.GetNext(currentId, result)?.ID;

            if (nextId == null)
            {
                // TODO: QTE(3) UI가 없어 4·5 쪽에 가지 못하므로, 4a·5 엔딩 흐름은 QTE를 만든 뒤 확인한다
                if (!gameData.TryGetEndingInfo(currentId, out JsonEndingInfo ending))
                {
                    // 막다른 분기는 모두 endings.json에 있어야 한다
                    Debug.LogError($"Ending info not found: {currentId}");
                }
                else if (ending.IsFinal)
                {
                    // 빈 문자열이면 다음 실행 때 GameEntry가 시작 분기부터 진행한다
                    gameState.UpdateGrade(string.Empty);
                }
                else
                {
                    gameState.AddFall();
                    gameState.UpdateGrade(gameState.CheckpointGrade);
                }

                // 엔딩은 GameEntry를 다시 타지 않으므로, 씬을 바꾸기 전에 엔딩 뒤 기록을 저장한다
                gameState.Save();

                SceneChange.ChangeScene("TitleScene");

                return;
            }

            gameState.UpdateGrade(nextId);

            GameEntry();
        }

        #endregion

        #region UI

        /// <summary>
        /// MediaInteractUI 프리팹을 생성하고, 버튼 클릭을 영상 재생/일시정지에 연결한다
        /// </summary>
        /// <remarks>
        /// 프리팹은 씬의 VideoPlayerScript를 Inspector에서 참조할 수 없으므로, 생성한 뒤 코드로 연결한다
        /// </remarks>
        private void ConnectMediaButton()
        {
            GameObject ui = Instantiate(mediaInteractUI);
            ButtonSetting playButton = ui.GetComponentInChildren<ButtonSetting>();

            if (playButton.ButtonType != ButtonType.MediaPlay)
            {
                Debug.LogError($"Not Match {ButtonType.MediaPlay} Button Type");
                return;
            }

            playButton.Bind(videoPlayer.RenewPlay);
            mediaVisual = ui.GetComponentInChildren<ButtonVisual>();
        }

        /// <summary>
        /// SelectUI·QTEUI 프리팹을 생성해 ActionUIController에 등록하고, 각 결과를 받도록 구독한다
        /// </summary>
        /// <remarks>
        /// 프리팹 원본이 숨김(alpha 0) 상태라 생성 직후에는 보이지 않고, Action 단계에서 ActionUIController가 켠다
        /// </remarks>
        private void ConnectActionUIController()
        {
            GameObject selectUI = Instantiate(selectUIPrefab);
            actionUIController.SetSelectUI(selectUI);
            actionUIController.OptionSelected += ExitAction;

            GameObject qteUI = Instantiate(qteUIPrefab);
            actionUIController.SetQTEUI(qteUI);
            actionUIController.OnQTEChecked += QTEChecked;
            actionUIController.OnQTETimeOut += QTETimeOut;
        }

        #endregion

        #region Input

        /// <remarks>
        /// 영상만 재생하는 Play 단계에서만 화면 클릭으로 미디어 버튼을 보이거나 숨긴다.
        /// Action의 QTE·선택지 구간은 일시정지를 막으므로 토글하지 않는다
        /// </remarks>
        private void HandleInput()
        {
            if (mediaVisual == null)
            {
                return;
            }

            if (gameInput.IsScreenClicked())
            {
                if (gameState.GamePhase.Current == PhaseState.Play)
                {
                    mediaVisual.ToggleVisible();
                }
                else if (gameState.GamePhase.Current == PhaseState.Action)
                {
                    if (gameData.TryGetNodeInfo(gameState.NewestGrade, out JsonNodeInfo info))
                    {
                        if (info.Type == "QTE")
                        {
                            actionUIController.CheckQTE();
                        }
                    }
                }
            }
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
