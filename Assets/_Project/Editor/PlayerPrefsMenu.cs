using UnityEditor;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 상단 FMV 메뉴에서 저장된 PlayerPrefs를 지운다
    /// </summary>
    /// <remarks>
    /// 세이브가 없는 첫 실행 흐름을 다시 확인할 때 쓴다.
    /// Play 중에 지우면 종료할 때 GameState.End가 다시 저장하므로, Play를 멈춘 뒤 실행한다
    /// </remarks>
    public static class PlayerPrefsMenu
    {
        #region Menu

        [MenuItem("FMV/Reset PlayerPrefs")]
        private static void ResetPlayerPrefs()
        {
            if (!EditorUtility.DisplayDialog("Reset PlayerPrefs", "저장된 PlayerPrefs를 모두 지웁니다.", "지우기", "취소"))
            {
                return;
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs reset");
        }

        #endregion
    }
}
