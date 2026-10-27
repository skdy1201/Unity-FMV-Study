using UnityEngine;
using UnityEngine.SceneManagement;

namespace FMV
{
    /// <summary>
    /// 씬 전환과 게임 종료를 모은 정적 유틸
    /// </summary>
    public static class SceneChange
    {
        /// <summary>
        /// PlayScene에 분기도부터 들어가는지 여부. false면 영상을 바로 재생한다
        /// </summary>
        /// <remarks>
        /// 씬이 바뀌면 오브젝트는 사라지므로 static에 둔다. ChangeScene이 부를 때마다 다시 정한다
        /// </remarks>
        public static bool IsCladogramEntry { get; private set; }

        /// <summary>
        /// 플레이를 시작할 때 진입 정보를 기본값으로 되돌린다
        /// </summary>
        /// <remarks>
        /// 프로젝트가 Domain Reload를 끄고 있어 static 값이 이전 플레이에서 남으므로, Unity가 플레이 시작마다 부르게 한다
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetEntry()
        {
            IsCladogramEntry = false;
        }

        /// <summary>
        /// 이름으로 씬을 불러옴
        /// </summary>
        /// <remarks>
        /// 대상 씬이 Build Settings에 등록되어 있어야 한다.
        /// 진입 정보를 넘기지 않으면 false로 되돌리므로, 씬을 떠날 때 따로 비우지 않아도 된다
        /// </remarks>
        /// <param name="sceneName"> 불러올 씬 이름 </param>
        /// <param name="isCladogramEntry"> PlayScene에 분기도부터 들어가는지 여부 </param>
        /// <param name="mode"> Single이면 현재 씬을 교체, Additive면 현재 씬 위에 추가 </param>
        public static void ChangeScene(string sceneName, bool isCladogramEntry = false, LoadSceneMode mode = LoadSceneMode.Single)
        {
            IsCladogramEntry = isCladogramEntry;
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
}
