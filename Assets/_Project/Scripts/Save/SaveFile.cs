using System;
using System.IO;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// save.json을 persistentDataPath에서 읽고 쓴다
    /// </summary>
    /// <remarks>
    /// Assets 안의 파일(TextAsset)은 빌드에서 읽기 전용이라, 바뀌는 기록은 persistentDataPath에 둔다.
    /// 씬 오브젝트가 없는 static 클래스라 타이틀 씬과 플레이 씬 어디서나 같은 방식으로 쓴다
    /// </remarks>
    public static class SaveFile
    {
        #region Private Fields

        private const string FileName = "save.json";

        #endregion

        #region Properties

        /// <summary>
        /// save.json의 전체 경로
        /// </summary>
        /// <remarks>
        /// Windows는 C:\Users\사용자\AppData\LocalLow\회사명\제품명\save.json
        /// </remarks>
        public static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

        #endregion

        #region Save

        /// <summary>
        /// save.json을 읽어 저장 기록을 만든다
        /// </summary>
        /// <returns> 파일이 없거나 읽을 수 없으면 기본값으로 만든 새 기록 </returns>
        public static JsonSaveData Load()
        {
            if (!File.Exists(FilePath))
            {
                return new JsonSaveData();
            }

            try
            {
                string json = File.ReadAllText(FilePath);
                JsonSaveData data = JsonUtility.FromJson<JsonSaveData>(json);

                // 빈 파일이면 FromJson이 null을 돌려준다
                return data ?? new JsonSaveData();
            }
            catch (Exception e)
            {
                // 파일이 깨져 있어도 게임은 새 기록으로 시작한다
                Debug.LogWarning($"Save load failed, start with new data: {e.Message}");
                return new JsonSaveData();
            }
        }

        /// <summary>
        /// 저장 기록을 save.json에 덮어쓴다
        /// </summary>
        /// <param name="data"> 저장할 기록 </param>
        public static void Save(JsonSaveData data)
        {
            // 두 번째 인자 true는 줄바꿈·들여쓰기를 넣어 파일을 눈으로 확인하기 쉽게 한다
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(FilePath, json);
        }

        /// <summary>
        /// save.json을 지운다
        /// </summary>
        /// <remarks>
        /// 파일만 지우므로, 이미 불러 둔 SaveController의 기록은 그대로 남는다. 지운 뒤에는 씬을 다시 불러 새 기록으로 시작한다.
        /// 씬을 떠날 때 저장하는 GameController가 있는 플레이 씬에서 지우면 다시 저장되므로, 저장하지 않는 타이틀 씬에서 부른다
        /// </remarks>
        /// <returns> 파일이 있어서 지웠으면 true, 원래 없었으면 false </returns>
        public static bool Delete()
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            File.Delete(FilePath);
            return true;
        }

        #endregion
    }
}
