using UnityEditor;

/// <summary>
/// ButtonSetting의 Inspector에서 ButtonType에 필요한 필드만 표시
/// </summary>
/// <remarks>
/// 필드는 FindProperty에 문자열로 찾으므로, ButtonSetting의 필드 이름을 바꾸면 여기도 같이 바꿔야 한다
/// </remarks>
[CustomEditor(typeof(ButtonSetting))]
[CanEditMultipleObjects]
public class ButtonSettingEditor : Editor
{
    #region Private Fields

    private SerializedProperty scriptProperty;
    private SerializedProperty buttonType;
    private SerializedProperty targetSceneName;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        scriptProperty = serializedObject.FindProperty("m_Script");
        buttonType = serializedObject.FindProperty("buttonType");
        targetSceneName = serializedObject.FindProperty("targetSceneName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawScriptField();
        EditorGUILayout.PropertyField(buttonType);

        if (ShouldShowSceneName())
        {
            EditorGUILayout.PropertyField(targetSceneName);
        }

        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region Inspector Drawing

    private void DrawScriptField()
    {
        using (new EditorGUI.DisabledScope(true))
        {
            EditorGUILayout.PropertyField(scriptProperty);
        }
    }

    private bool ShouldShowSceneName()
    {
        // 여러 버튼을 함께 선택했을 때 타입이 섞여 있으면, 씬 이름을 편집할 수 있게 표시한다
        if (buttonType.hasMultipleDifferentValues)
        {
            return true;
        }

        return (ButtonType)buttonType.enumValueIndex == ButtonType.ChangeScene;
    }

    #endregion
}
