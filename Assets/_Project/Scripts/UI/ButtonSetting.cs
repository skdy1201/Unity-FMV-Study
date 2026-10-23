using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 버튼 클릭 시 수행할 기능 종류
    /// </summary>
    public enum ButtonType
    {
        ChangeScene = 0,
        Exit = 1,
        MediaPlay = 2,
        Select = 3,
        MainMenuInteract = 4,
    }

    /// <summary>
    /// ButtonType에 따라 같은 오브젝트의 Button 클릭 기능을 연결
    /// </summary>
    /// <remarks>
    /// GameButton 프리팹 하나로 여러 메뉴 버튼을 만들기 위해, 기능을 Inspector의 ButtonType으로 고른다
    /// </remarks>
    [RequireComponent(typeof(Button))]
    public class ButtonSetting : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private ButtonType buttonType;
        [SerializeField] private string targetSceneName;
        [SerializeField] private ButtonVisual buttonVisual;

        #endregion

        #region Private Fields

        private Button unityButton;
        private UnityAction onClickAction;
        private bool isListenerAdded = false;

        #endregion

        #region Properties

        /// <summary>
        /// Inspector에서 고른 버튼 기능 종류
        /// </summary>
        public ButtonType ButtonType => buttonType;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            unityButton = GetComponent<Button>();
            onClickAction = CreateClickAction();

            // Bind가 교체하는 onClickAction과 섞이지 않도록 따로 한 번만 등록한다.
            // 버튼 사용 여부는 구독 해제가 아니라 Button 쪽 상태로 조정한다
            if (buttonVisual != null)
            {
                unityButton.onClick.AddListener(buttonVisual.ChangeImage);
            }
        }

        private void OnEnable()
        {
            if (onClickAction != null && !isListenerAdded)
            {
                unityButton.onClick.AddListener(onClickAction);
                isListenerAdded = true;
            }
        }

        private void OnDisable()
        {
            if (isListenerAdded)
            {
                unityButton.onClick.RemoveListener(onClickAction);
                isListenerAdded = false;
            }
        }

        #endregion

        #region Click Binding

        /// <summary>
        /// 외부 인스턴스가 필요한 클릭 기능을 연결한다. 이미 연결된 기능이 있으면 교체한다
        /// </summary>
        /// <remarks>
        /// 비활성 상태면 등록하지 않고, 활성화될 때 OnEnable이 등록한다
        /// </remarks>
        /// <param name="action"> 클릭 시 호출할 기능 </param>
        public void Bind(UnityAction action)
        {
            // 이전 기능을 빼지 않고 덮어쓰면, 버튼에는 남았는데 참조를 잃어 다시 뺄 수 없다
            if (isListenerAdded)
            {
                unityButton.onClick.RemoveListener(onClickAction);
                isListenerAdded = false;
            }

            onClickAction = action;

            if (onClickAction != null && isActiveAndEnabled)
            {
                unityButton.onClick.AddListener(onClickAction);
                isListenerAdded = true;
            }
        }

        /// <summary>
        /// 선택지 버튼일 때만 클릭 기능을 연결한다
        /// </summary>
        /// <remarks>
        /// 타입 검사를 Bind 밖에 두어, 선택지 버튼에만 필요한 처리를 Bind를 건드리지 않고 여기에 더할 수 있게 한다
        /// </remarks>
        /// <param name="action"> 클릭 시 호출할 기능 </param>
        public void SelectBind(UnityAction action)
        {
            if (buttonType != ButtonType.Select)
            {
                Debug.LogError($"Not Match {ButtonType.Select} Button Type: {name}");
                return;
            }

            Bind(action);
        }

        private UnityAction CreateClickAction()
        {
            switch (buttonType)
            {
                case ButtonType.ChangeScene:
                    return () => SceneChange.ChangeScene(targetSceneName);
                case ButtonType.Exit:
                    return SceneChange.GameExit;
                default:
                    return null;
            }
        }

        #endregion
    }
}
