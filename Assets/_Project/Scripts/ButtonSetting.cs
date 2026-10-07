using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum ButtonType
{
    Change = 0,
    Exit = 1,
}

public class ButtonSetting : MonoBehaviour
{
    #region Member
    [SerializeField] private ButtonType buttonType;
    [SerializeField] private Button unityButton;
    [SerializeField] private string ChangeSceneString;

    private UnityAction onClickAction;

    #endregion

    #region UnityLifeCycle

    private void Awake()
    {
        unityButton = GetComponent<Button>();

        switch(buttonType)
        {
            case ButtonType.Change:
                onClickAction = ()=>SceneChange.ChangeScene(ChangeSceneString);
                unityButton.onClick.AddListener(onClickAction);
                break;
            case ButtonType.Exit:
                onClickAction = ()=>SceneChange.GameExit();
                break;
            default:
            break;
        }
    }

    private void OnDestroy()
    {
        unityButton.onClick.RemoveListener(onClickAction);
    }
    #endregion

}
