using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// 버튼 클릭 시 수행할 기능 종류
/// </summary>
public enum ButtonType
{
    ChangeScene = 0,
    Exit = 1,
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
    [FormerlySerializedAs("ChangeSceneString")]
    [SerializeField] private string targetSceneName;

    #endregion

    #region Private Fields

    private Button unityButton;
    private UnityAction onClickAction;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        unityButton = GetComponent<Button>();
        onClickAction = CreateClickAction();
    }

    private void OnEnable()
    {
        if (onClickAction != null)
        {
            unityButton.onClick.AddListener(onClickAction);
        }
    }

    private void OnDisable()
    {
        if (onClickAction != null)
        {
            unityButton.onClick.RemoveListener(onClickAction);
        }
    }

    #endregion

    #region Click Binding

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
