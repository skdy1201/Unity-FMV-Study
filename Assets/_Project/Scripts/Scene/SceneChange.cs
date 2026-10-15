using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환과 게임 종료를 모은 정적 유틸
/// </summary>
public static class SceneChange
{
    /// <summary>
    /// 이름으로 씬을 불러옴
    /// </summary>
    /// <remarks>
    /// 대상 씬이 Build Settings에 등록되어 있어야 한다
    /// </remarks>
    /// <param name="sceneName"> 불러올 씬 이름 </param>
    /// <param name="mode"> Single이면 현재 씬을 교체, Additive면 현재 씬 위에 추가 </param>
    public static void ChangeScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(sceneName, mode);
    }

    /// <summary>
    /// 게임 종료. 에디터에서는 플레이 모드를 끝냄
    /// </summary>
    public static void GameExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
