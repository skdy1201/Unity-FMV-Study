using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 메인 메뉴에서 지금 보이는 화면
    /// </summary>
    public enum MainMenuState
    {
        Title = 0,
        Achievement = 1,
    }

    /// <summary>
    /// 메인 메뉴 씬의 SaveController와 화면 UI를 생성해 들고, 메뉴 버튼 클릭을 현재 상태에 맞게 처리한다
    /// </summary>
    /// <remarks>
    /// 상태는 아직 둘뿐이라 enum 하나로 두고, 늘어나면 GameState·GamePhase처럼 분리할지 그때 정한다
    /// </remarks>
    public class MainMenuController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Prefabs")]
        [SerializeField] private SaveController saveControllerPrefab;

        [Header("UI Prefabs")]
        [SerializeField] private TitleUIController titleUIPrefab;
        [SerializeField] private AchievementListUIController achievementListUIPrefab;

        #endregion

        #region Private Fields

        private MainMenuState menuState = MainMenuState.Title;

        private SaveController saveController;
        private TitleUIController titleUI;
        private AchievementListUIController achievementListUI;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            SpawnSaveController();
            SpawnMenuUI();
        }

        #endregion

        #region Save

        /// <summary>
        /// SaveController 프리팹을 생성해 참조를 보관한다
        /// </summary>
        /// <remarks>
        /// Instantiate가 반환하기 전에 SaveController.Awake가 save.json을 불러오므로, 업적 목록이 기록을 읽기 전에 가장 먼저 부른다
        /// </remarks>
        private void SpawnSaveController()
        {
            saveController = Instantiate(saveControllerPrefab);
        }

        #endregion

        #region Menu

        /// <summary>
        /// MainMenuInteract 버튼을 눌렀을 때, 현재 상태에 따라 다음 화면으로 바꾼다
        /// </summary>
        /// <remarks>
        /// 타이틀의 업적 버튼과 업적 목록의 닫기 버튼이 같은 함수를 쓰고, 무엇을 할지는 상태로 정한다
        /// </remarks>
        private void MenuInteract()
        {
            switch (menuState)
            {
                case MainMenuState.Title:
                    titleUI.Hide();
                    achievementListUI.Show();
                    menuState = MainMenuState.Achievement;
                    break;
                case MainMenuState.Achievement:
                    achievementListUI.Hide();
                    titleUI.Show();
                    menuState = MainMenuState.Title;
                    break;
                default:
                    Debug.LogError($"Unknown main menu state: {menuState}");
                    break;
            }
        }

        #endregion

        #region UI

        /// <summary>
        /// 타이틀·업적 목록 UI 프리팹을 생성하고, 각 UI의 MainMenuInteract 버튼을 MenuInteract에 연결한다
        /// </summary>
        /// <remarks>
        /// 업적 목록은 자기 Awake에서 숨겨지므로, 시작 화면은 MainMenuState.Title과 같다.
        /// 메인 메뉴에서는 업적이 새로 열리지 않으므로 목록은 생성할 때 한 번만 채운다
        /// </remarks>
        private void SpawnMenuUI()
        {
            titleUI = Instantiate(titleUIPrefab);
            BindMenuButtons(titleUI.gameObject);

            achievementListUI = Instantiate(achievementListUIPrefab);
            achievementListUI.SetAchievements(saveController.Achievements, saveController.SaveData.UnlockedAchievements);
            BindMenuButtons(achievementListUI.gameObject);
        }

        /// <remarks>
        /// 씬 이동·종료 버튼은 ButtonSetting이 스스로 처리하므로, MainMenuInteract 버튼만 골라 연결한다
        /// </remarks>
        private void BindMenuButtons(GameObject ui)
        {
            foreach (ButtonSetting button in ui.GetComponentsInChildren<ButtonSetting>())
            {
                if (button.ButtonType == ButtonType.MainMenuInteract)
                {
                    button.Bind(MenuInteract);
                }
            }
        }

        #endregion
    }
}
