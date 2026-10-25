using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기도 화면에만 쓰는 데이터를 들고 있는 파츠
    /// </summary>
    /// <remarks>
    /// 게임 진행에 필요한 데이터는 GameData가, 분기도를 보여 주는 데만 쓰는 데이터는 여기서 맡는다
    /// </remarks>
    [System.Serializable]
    public class GameCladogramData : ISetting
    {
        #region ISetting

        public void Init()
        {
        }

        public void End()
        {
        }

        #endregion
    }
}
