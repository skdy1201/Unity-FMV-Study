using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneChange
{
    static public void ChangeScene(string s, int mode = 0)
    {
        SceneManager.LoadScene(s, (LoadSceneMode)mode);
    }

    static public void GameExit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
