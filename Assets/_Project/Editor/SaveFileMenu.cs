using System.IO;
using UnityEditor;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 상단 FMV 메뉴에서 persistentDataPath의 save.json을 지운다
    /// </summary>
    /// <remarks>
    /// 세이브가 없는 첫 실행 흐름이나 업적이 처음 열리는 흐름을 다시 확인할 때 쓴다.
    /// Play 중에 지우면 종료할 때 GameController가 SaveController로 다시 저장하므로, Play를 멈춘 뒤 실행한다
    /// </remarks>
    public static class SaveFileMenu
    {
        #region Menu

        [MenuItem("FMV/Reset Save File")]
        private static void ResetSaveFile()
        {
            if (!EditorUtility.DisplayDialog("Reset Save File", $"저장 파일을 지웁니다.\n{SaveFile.FilePath}", "지우기", "취소"))
            {
                return;
            }

            if (!File.Exists(SaveFile.FilePath))
            {
                Debug.Log("Save file not found");
                return;
            }

            File.Delete(SaveFile.FilePath);
            Debug.Log($"Save file deleted: {SaveFile.FilePath}");
        }

        #endregion
    }
}
