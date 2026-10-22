using UnityEngine;

namespace FMV
{
    /// <summary>
    /// save.json 기록을 들고, SaveFile로 불러오고 저장한다
    /// </summary>
    /// <remarks>
    /// 씬 루트 컨트롤러가 프리팹으로 생성해 씬마다 소유한다.
    /// 기록은 씬마다 새로 읽어 디스크와 맞추고, 다른 파츠는 이 객체가 가진 같은 JsonSaveData를 받아 쓴다
    /// </remarks>
    public class SaveController : MonoBehaviour
    {
        #region Private Fields

        private JsonSaveData saveData;

        #endregion

        #region Properties

        /// <summary>
        /// save.json에서 불러온 저장 기록
        /// </summary>
        /// <remarks>
        /// 다른 파츠가 같은 객체를 바꿔야 저장에 반영되므로, 객체 자체를 바꾸는 set은 두지 않는다
        /// </remarks>
        public JsonSaveData SaveData => saveData;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Load();
        }

        #endregion

        #region Save

        /// <summary>
        /// save.json을 읽어 저장 기록을 새로 만든다
        /// </summary>
        /// <remarks>
        /// 이전에 받아 둔 SaveData 참조는 새 기록과 달라지므로, 다시 불렀다면 SaveData를 다시 받아야 한다
        /// </remarks>
        public void Load()
        {
            saveData = SaveFile.Load();
        }

        /// <summary>
        /// 지금 들고 있는 저장 기록을 save.json에 덮어쓴다
        /// </summary>
        public void Save()
        {
            SaveFile.Save(saveData);
        }

        #endregion
    }
}
