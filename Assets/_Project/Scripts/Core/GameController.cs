using System;
using System.Collections;
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

        [Header("Parts")]
        [SerializeField] private GameData gameData = new GameData();
        [SerializeField] private ClipConnect clipConnect = new ClipConnect();

        [Header("Scene References")]
        [FormerlySerializedAs("videoPlayer")]
        [SerializeField] private VideoPlayerController videoPlayerController;
        [SerializeField] private ActionUIController actionUIController;

        [Header("Prefabs")]
        [SerializeField] private SaveController saveControllerPrefab;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject mediaInteractUI;
        [SerializeField] private GameObject selectUIPrefab;
        [SerializeField] private GameObject qteUIPrefab;
        [SerializeField] private AchievementPopupUIController achievementPopupUIPrefab;

        #endregion

        #region Private Fields

        private const float DefaultActionSpeed = 0.5f;
        private const float PopupInterval = 0.2f;

        private GameState gameState = new GameState();
        private GameInput gameInput = new GameInput();

        private SaveController saveController;
        private AchievementPopupUIController achievementPopupUI;

        // 이번 씬에서 들어간 엔딩. 엔딩에 들어가면 NewestGrade가 엔딩 뒤 시작 위치로 바뀌므로, 엔딩 영상이 끝났는지는 이 값으로 안다.
        // 엔딩 뒤에는 씬이 바뀌므로 한 번 들어가면 비우지 않는다
        private JsonEndingInfo currentEnding;
        private ButtonVisual mediaVisual;
        private List<ISetting> interfaceList;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            SpawnSaveController();

            interfaceList = new List<ISetting> { gameData, clipConnect };

            foreach (ISetting set in interfaceList)
            {
                set.Init();
            }

            videoPlayerController.ClipEnd += ClipEnd;
            videoPlayerController.OnActionReached += EnterAction;

            ConnectMediaButton();
            ConnectActionUIController();
            SpawnAchievementPopupUI();
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

            saveController.Save();
        }

        #endregion

        #region Save

        /// <summary>
        /// SaveController 프리팹을 생성해 참조를 보관하고, 불러온 기록을 GameState에 넘긴다
        /// </summary>
        /// <remarks>
        /// Instantiate가 반환하기 전에 SaveController.Awake가 실행되어 save.json을 불러오므로,
        /// 다른 파츠가 기록을 쓰기 전에 가장 먼저 부른다
        /// </remarks>
        private void SpawnSaveController()
        {
            saveController = Instantiate(saveControllerPrefab);
            gameState.GameSave = saveController.SaveData;
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

            videoPlayerController.PrepareNextClips(clips);
            videoPlayerController.PlayClip(currentId);

            gameState.GamePhase.SetPhase(PhaseState.Play);

            // nodes.json에 없는 분기는 Action 없이 클립 끝까지 재생하므로 기다리지 않는다
            if (gameData.TryGetNodeInfo(currentId, out JsonNodeInfo info))
            {
                gameState.CheckpointGrade = currentId;
                videoPlayerController.WaitActionTime(info.Timing.ActionStart);
            }

            if (gameData.TryGetEndingInfo(currentId, out JsonEndingInfo ending))
            {
                EnterEnding(ending);
            }

            // 분기에 들어갈 때마다 저장해, 중간에 꺼져도 이 분기부터 다시 시작한다
            saveController.Save();
        }

        /// <summary>
        /// 엔딩 분기에 들어가면 엔딩을 본 것으로 보고, 실패 횟수·다음 시작 위치를 바꾼 뒤 새로 열린 업적 팝업을 띄운다
        /// </summary>
        /// <remarks>
        /// 엔딩 영상이 끝날 때까지 미루면 팝업이 영상 뒤에 떠서 기다려야 하므로, 들어가는 순간 처리한다.
        /// 다음 시작 위치도 여기서 엔딩 뒤 위치로 바꿔, 엔딩 영상 도중에 꺼도 엔딩을 다시 보거나 실패 횟수가 두 번 오르지 않는다
        /// </remarks>
        /// <param name="ending"> 들어간 엔딩 정보 </param>
        private void EnterEnding(JsonEndingInfo ending)
        {
            currentEnding = ending;

            if (ending.IsFinal)
            {
                // 빈 문자열이면 다음 실행 때 GameEntry가 시작 분기부터 진행한다
                gameState.UpdateGrade(string.Empty);
            }
            else
            {
                gameState.AddFall();
                gameState.UpdateGrade(gameState.CheckpointGrade);
            }

            // 업적 조건인 실패 횟수와 본 엔딩은 엔딩에 들어갈 때 바뀌므로, 바뀐 뒤에 새로 열린 업적을 찾는다
            StartCoroutine(ShowNewAchievements());
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
                videoPlayerController.SetLoopTime(new Vector2(timing.LoopStart, timing.LoopEnd), speed);
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
                    videoPlayerController.SetActionSpeed(speed);
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
            // 엔딩 기록은 들어갈 때 이미 바꿨으므로, 영상이 끝나면 메인 메뉴로 넘어가기만 한다
            if (currentEnding != null)
            {
                StartCoroutine(PlayEnding());

                return;
            }

            // 엔딩은 위에서 빠져나가므로, 여기까지 오면 NewestGrade는 지금 재생이 끝난 일반 분기다
            string currentId = gameState.NewestGrade;
            string nextId = gameData.Graph.GetNext(currentId, result)?.ID;

            if (nextId == null)
            {
                // 일반 분기는 다음 분기가 있어야 한다. 없으면 막다른 분기가 endings.json에 빠진 데이터 오류다
                Debug.LogError($"Ending info not found: {currentId}");

                StartCoroutine(PlayEnding());

                return;
            }

            gameState.UpdateGrade(nextId);

            GameEntry();
        }

        /// <summary>
        /// 엔딩 영상이 끝나면, 떠 있는 업적 팝업이 모두 끝날 때까지 기다린 뒤 메인 메뉴로 넘어간다
        /// </summary>
        /// <remarks>
        /// 씬을 바꾸면 팝업도 함께 사라지므로, 엔딩 영상이 팝업보다 짧을 때를 위해 기다린다
        /// </remarks>
        private IEnumerator PlayEnding()
        {
            yield return new WaitUntil(() => !achievementPopupUI.IsPlaying);

            SceneChange.ChangeScene("MainMenuScene");
        }

        #endregion

        #region Achievement

        /// <summary>
        /// 새로 열린 업적을 찾아 저장하고, 간격을 두고 팝업을 겹쳐 띄운다
        /// </summary>
        /// <remarks>
        /// 팝업을 띄우기만 하고 끝날 때까지 기다리지 않는다. 같은 씬 안에서는 분기가 바뀌어도 팝업이 남으므로,
        /// 게임 도중에는 StartCoroutine으로 시작만 하고, 씬을 바꿀 때만 IsPlaying으로 기다린다.
        /// 하나씩 끝날 때마다 다음을 띄우면 업적 수만큼 기다림이 길어지므로, 앞 팝업이 끝나기 전에 다음을 띄운다
        /// </remarks>
        private IEnumerator ShowNewAchievements()
        {
            List<AchievementDefinition> unlocked = saveController.CheckAchievement();

            // 열린 업적 목록이 바뀌었으므로, 팝업 도중 꺼져도 남도록 팝업보다 먼저 저장한다
            saveController.Save();

            foreach (AchievementDefinition achievement in unlocked)
            {
                achievementPopupUI.Popup(achievement.Icon);

                yield return new WaitForSeconds(PopupInterval);
            }
        }

        #endregion

        #region UI

        /// <summary>
        /// MediaInteractUI 프리팹을 생성하고, 버튼 클릭을 영상 재생/일시정지에 연결한다
        /// </summary>
        /// <remarks>
        /// 프리팹은 씬의 VideoPlayerController를 Inspector에서 참조할 수 없으므로, 생성한 뒤 코드로 연결한다
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

            playButton.Bind(videoPlayerController.RenewPlay);
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

        /// <summary>
        /// 업적 팝업 UI 프리팹을 생성해 참조를 보관한다
        /// </summary>
        /// <remarks>
        /// 생성될 때 팝업이 스스로 숨겨지므로, 엔딩에서 Popup을 부르기 전까지는 보이지 않는다
        /// </remarks>
        private void SpawnAchievementPopupUI()
        {
            achievementPopupUI = Instantiate(achievementPopupUIPrefab);
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
