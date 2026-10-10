using System.Collections.Generic;
using FMV;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] private GameData gameData = new GameData();
    private GameState gameState = new GameState();
    [SerializeField] private ClipConnect clipConnect = new ClipConnect();

    private List<ISetting> interfaceList;

    #region Properties

    /// <summary>
    /// videos.json에서 읽은 영상 id와 클립 이름 매핑
    /// </summary>
    public GameData GameData => gameData;

    /// <summary>
    /// 현재·최고 단계와 실패 횟수를 저장하는 상태
    /// </summary>
    public GameState GameState => gameState;

    /// <summary>
    /// 클립 이름으로 VideoClip을 찾아 주는 조회 테이블
    /// </summary>
    public ClipConnect ClipConnect => clipConnect;

    #endregion

    private void Awake()
    {
        interfaceList = new List<ISetting>{gameData, gameState, clipConnect};

        foreach(ISetting set in interfaceList)
        {
            set.Init();
        }
    }

    private void OnDisable()
    {
        foreach(ISetting set in interfaceList)
        {
            set.End();
        }
    }


}
