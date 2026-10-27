using System;
using System.Collections.Generic;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기도 데이터를 준비하고, GameController와 분기도 화면 사이를 잇는다
    /// </summary>
    /// <remarks>
    /// 화면을 켜고 끄는 일은 CladogramUIController가 맡고, 여기서는 프리팹으로 생성해 참조로 들고 호출만 한다.
    /// 분기도에만 쓰는 파츠는 ISetting으로 묶어 Awake에서 Init, OnDisable에서 End를 같은 순서로 호출한다.
    /// 분기 구조(Graph)는 GameController에게서 받아, 영상 진행과 같은 데이터로 분기도를 그린다
    /// </remarks>
    public class GameCladogramController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Parts")]
        [SerializeField] private GameCladogramData cladogramData = new GameCladogramData();
        [SerializeField] private ThumbnailConnect thumbnailConnect = new ThumbnailConnect();

        [Header("Controllers")]
        [SerializeField] private GameController gameController;
        [SerializeField] private SaveController saveController;

        [Header("UI Prefabs")]
        [SerializeField] private CladogramUIController cladogramUIPrefab;

        [Header("Layout")]
        [Tooltip("x: 단계(깊이) 사이 가로 간격, y: 같은 단계 안 카드 사이 세로 간격")]
        [SerializeField] private Vector2 cardSpacing = new Vector2(900f, 350f);

        #endregion

        #region Private Fields

        private CladogramUIController cladogramUI;
        private List<ISetting> interfaceList;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            interfaceList = new List<ISetting> { cladogramData, thumbnailConnect };

            foreach (ISetting set in interfaceList)
            {
                set.Init();
            }

            SpawnCladogramUI();
        }

        private void Start()
        {
            DrawCladogram();

            gameController.OnBackToCladogram += ShowCladogram;
            cladogramUI.BindBack(() => SceneChange.ChangeScene("MainMenuScene"));

            // 바로 재생으로 들어와도 뒤로 가기에 쓰도록 분기도는 만들어 두고 화면만 숨긴다
            if (!SceneChange.IsCladogramEntry)
            {
                cladogramUI.Hide();
            }
        }

        private void OnDisable()
        {
            foreach (ISetting set in interfaceList)
            {
                set.End();
            }
        }

        #endregion

        #region UI

        /// <summary>
        /// 분기도 화면 프리팹을 생성해 참조를 보관한다
        /// </summary>
        /// <remarks>
        /// 카드는 이 화면 아래에 만들어지므로, Start의 BuildCladogram보다 먼저 Awake에서 생성한다
        /// </remarks>
        private void SpawnCladogramUI()
        {
            cladogramUI = Instantiate(cladogramUIPrefab);
        }

        /// <summary>
        /// 카드 영역을 비우고, 지금 기록으로 선과 카드를 다시 그린다
        /// </summary>
        /// <remarks>
        /// 카드 상태와 선의 열림은 그릴 때 한 번 정해지므로, 기록이 바뀐 뒤 분기도를 다시 보일 때마다 부른다
        /// </remarks>
        private void DrawCladogram()
        {
            Graph graph = gameController.Graph;

            cladogramUI.ClearContent();

            // 선이 카드 뒤에 깔리도록 선을 먼저 만든다
            Dictionary<string, Vector2> positions = MakeLayout(graph);
            BuildLines(graph, positions);
            BuildCladogram();
        }

        /// <summary>
        /// 영상에서 뒤로 돌아오면, 그동안 바뀐 기록으로 분기도를 다시 그려 보인다
        /// </summary>
        private void ShowCladogram()
        {
            DrawCladogram();
            cladogramUI.Show();
        }

        #endregion

        #region Layout

        /// <summary>
        /// 시작 분기부터 BFS로 한 단계씩 돌며, 분기마다 단계(깊이)와 단계 안 순서로 카드가 놓일 위치를 정한다
        /// </summary>
        /// <remarks>
        /// 위치만 먼저 모아 두고, 선과 카드는 이 결과를 받아 그린다.
        /// 선은 앞 분기에서 다음 분기로 이어지므로 id로 바로 위치를 찾을 수 있게 Dictionary로 둔다.
        /// 시작 카드가 (0, 0)이고, 단계가 깊어질수록 오른쪽으로 가며 같은 단계의 카드는 y = 0을 가운데로 위아래에 펼친다.
        /// 여러 분기가 같은 분기로 이어지면 처음 만난 단계에 한 번만 넣는다
        /// </remarks>
        /// <param name="graph"> 분기 구조 </param>
        /// <returns> 분기 id별 카드 위치 (cardContent 기준 anchoredPosition) </returns>
        private Dictionary<string, Vector2> MakeLayout(Graph graph)
        {
            Dictionary<string, Vector2> positions = new Dictionary<string, Vector2>();
            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(graph.Start.ID);
            visited.Add(graph.Start.ID);

            int depth = 0;

            while (queue.Count > 0)
            {
                // 한 단계를 꺼내기 전에는 큐에 그 단계의 분기만 들어 있으므로, 지금 개수가 이 단계의 카드 수다
                int levelCount = queue.Count;

                for (int row = 0; row < levelCount; ++row)
                {
                    string id = queue.Dequeue();

                    // 가운데 순서를 0으로 맞춘다. 짝수 개면 가운데가 두 카드 사이라 반 칸씩 어긋난다 (2개: +0.5, -0.5)
                    float offset = (levelCount - 1) * 0.5f - row;
                    positions.Add(id, new Vector2(depth * cardSpacing.x, offset * cardSpacing.y));

                    // NodeResult 순서로 돌아, 같은 단계 안의 세로 순서가 매번 같게 한다
                    Node node = graph.FindNode(id);
                    foreach (NodeResult result in Enum.GetValues(typeof(NodeResult)))
                    {
                        string nextId = node.GetNext(result);
                        if (nextId != null && visited.Add(nextId))
                        {
                            queue.Enqueue(nextId);
                        }
                    }
                }

                ++depth;
            }

            return positions;
        }

        #endregion

        #region Line

        /// <summary>
        /// 분기마다 다음 분기로 이어지는 꺾인 선을 만들고, 꺾이는 점에 joint를 놓는다
        /// </summary>
        /// <remarks>
        /// 관계는 Graph에서, 위치는 MakeLayout 결과에서 가져온다.
        /// 한 연결은 a(부모 → midX 가로), b(midX에서 자식 높이까지 세로), c(자식 높이에서 자식까지 가로) 세 조각이다.
        /// midX는 부모에서 단계 간격의 절반이라, 한 부모의 갈래는 모두 같은 세로줄에서 갈라진다.
        /// b·c는 부모와 자식이 둘 다 열려 있으면, a는 그런 갈래가 하나라도 있으면 열린 선이다.
        /// 시작 분기는 기록이 없어도 열림이므로, a를 부모 기준으로 하면 처음부터 갈라지는 점까지 반만 열려 보인다.
        /// 조각이 겹치면 열린 선이 위에 보이도록 잠긴 조각을 먼저 그리고, joint는 그 위에 놓는다.
        /// joint는 b의 양 끝이라 갈라지는 점·합쳐지는 점이 모두 포함되고, 같은 좌표는 한 번만 놓는다.
        /// 선은 카드 뒤에 깔려야 하므로 카드보다 먼저 부른다
        /// </remarks>
        /// <param name="graph"> 분기 구조 </param>
        /// <param name="positions"> 분기 id별 카드 위치 </param>
        private void BuildLines(Graph graph, Dictionary<string, Vector2> positions)
        {
            List<(Vector2 start, Vector2 end)> lockedSegments = new List<(Vector2 start, Vector2 end)>();
            List<(Vector2 start, Vector2 end)> openedSegments = new List<(Vector2 start, Vector2 end)>();
            HashSet<Vector2> joints = new HashSet<Vector2>();

            foreach ((string id, Vector2 pos) in positions)
            {
                bool isOpened = GetCardState(id) != CardState.Locked;
                Vector2 branch = new Vector2(pos.x + cardSpacing.x * 0.5f, pos.y);
                bool hasNext = false;
                bool hasOpenedPath = false;

                foreach (NodeResult result in Enum.GetValues(typeof(NodeResult)))
                {
                    Node next = graph.GetNext(id, result);

                    if (next != null)
                    {
                        hasNext = true;

                        Vector2 nextPos = positions[next.ID];
                        Vector2 turn = new Vector2(branch.x, nextPos.y);

                        bool isPathOpened = isOpened && GetCardState(next.ID) != CardState.Locked;
                        hasOpenedPath |= isPathOpened;

                        List<(Vector2 start, Vector2 end)> pathSegments = isPathOpened ? openedSegments : lockedSegments;

                        // 자식이 부모와 같은 높이면 b가 없이 가로로 곧게 이어진다
                        if (turn != branch)
                        {
                            pathSegments.Add((branch, turn));
                            joints.Add(branch);
                            joints.Add(turn);
                        }

                        pathSegments.Add((turn, nextPos));
                    }
                }

                // a는 갈래 수와 상관없이 부모마다 한 번만 그리고, 갈래를 하나라도 지나가 봤어야 열린 선이다
                if (hasNext)
                {
                    List<(Vector2 start, Vector2 end)> branchSegments = hasOpenedPath ? openedSegments : lockedSegments;
                    branchSegments.Add((pos, branch));
                }
            }

            foreach ((Vector2 start, Vector2 end) in lockedSegments)
            {
                cladogramUI.CreateLine(start, end, false);
            }

            foreach ((Vector2 start, Vector2 end) in openedSegments)
            {
                cladogramUI.CreateLine(start, end, true);
            }

            foreach (Vector2 joint in joints)
            {
                cladogramUI.CreateJoint(joint);
            }
        }

        #endregion

        #region Card

        /// <summary>
        /// 시작 분기부터 BFS로 한 단계씩 돌며 분기마다 카드를 만들고, 단계(깊이)와 단계 안 순서로 위치를 정한다
        /// </summary>
        /// <remarks>
        /// GameController의 Graph는 GameData가 Awake에서 만들므로 Start에서 부른다.
        /// 시작 카드가 (0, 0)이고, 단계가 깊어질수록 오른쪽으로 가며 같은 단계의 카드는 y = 0을 가운데로 위아래에 펼친다.
        /// 여러 분기가 같은 분기로 이어지면 처음 만난 단계에 한 번만 만든다
        /// </remarks>
        private void BuildCladogram()
        {
            Graph graph = gameController.Graph;

            Queue<string> queue = new Queue<string>();
            HashSet<string> visited = new HashSet<string>();

            queue.Enqueue(graph.Start.ID);
            visited.Add(graph.Start.ID);

            int depth = 0;

            while (queue.Count > 0)
            {
                // 한 단계를 꺼내기 전에는 큐에 그 단계의 분기만 들어 있으므로, 지금 개수가 이 단계의 카드 수다
                int levelCount = queue.Count;

                for (int row = 0; row < levelCount; ++row)
                {
                    string id = queue.Dequeue();

                    CladogramCard card = MakeCard(id);
                    if (card != null)
                    {
                        // 가운데 순서를 0으로 맞춘다. 짝수 개면 가운데가 두 카드 사이라 반 칸씩 어긋난다 (2개: +0.5, -0.5)
                        float offset = (levelCount - 1) * 0.5f - row;

                        RectTransform cardRect = (RectTransform)card.transform;
                        cardRect.anchoredPosition = new Vector2(depth * cardSpacing.x, offset * cardSpacing.y);
                    }

                    // NodeResult 순서로 돌아, 같은 단계 안의 세로 순서가 매번 같게 한다
                    Node node = graph.FindNode(id);
                    foreach (NodeResult result in Enum.GetValues(typeof(NodeResult)))
                    {
                        string nextId = node.GetNext(result);
                        if (nextId != null && visited.Add(nextId))
                        {
                            queue.Enqueue(nextId);
                        }
                    }
                }

                ++depth;
            }
        }

        /// <summary>
        /// 분기 id로 카드 정보와 썸네일을 찾아, 썸네일과 제목을 띄우고 진행 상태를 정한 카드 한 장을 만든다
        /// </summary>
        /// <remarks>
        /// 생성은 CladogramUIController에 맡기고, 여기서는 데이터를 찾아 채운다. 위치는 BuildCladogram이 정한다
        /// </remarks>
        /// <param name="id"> 카드로 만들 분기 id </param>
        /// <returns> 만든 카드, cards.json에 없는 id면 null </returns>
        private CladogramCard MakeCard(string id)
        {
            if (!cladogramData.CardMapping.TryGetValue(id, out JsonCardInfo info))
            {
                Debug.LogError($"Card info not found: {id}");
                return null;
            }

            CladogramCard card = cladogramUI.CreateCard();
            card.SetId(id);
            card.SetThumbnail(thumbnailConnect.GetThumbnail(info.Thumbnail), info.Title);

            card.SetState(GetCardState(id));
            card.Clicked += SelectCard;

            return card;
        }

        /// <summary>
        /// 고른 카드의 분기부터 영상을 시작하고 분기도를 숨긴다
        /// </summary>
        /// <remarks>
        /// 잠긴 카드는 카드 쪽에서 걸러지므로 여기에는 열렸거나 본 분기 id만 온다
        /// </remarks>
        /// <param name="id"> 고른 카드의 분기 id </param>
        private void SelectCard(string id)
        {
            cladogramUI.Hide();
            gameController.GameEntry(id);
        }

        /// <summary>
        /// save.json 기록으로 카드 상태를 정한다
        /// </summary>
        /// <remarks>
        /// SaveController는 Awake에서 save.json을 불러오므로, Start에서 부르는 BuildCladogram 안에서는 기록이 준비되어 있다.
        /// 시작 분기는 기록이 없어도 고를 수 있어야 하므로 최소 열림으로 둔다
        /// </remarks>
        /// <param name="id"> 분기 id </param>
        private CardState GetCardState(string id)
        {
            JsonProgressInfo progress = saveController.SaveData.Progress;

            if (progress.Watched.Contains(id))
            {
                return CardState.Watched;
            }

            if (progress.Visited.Contains(id) || id == gameController.Graph.Start.ID)
            {
                return CardState.Opened;
            }

            return CardState.Locked;
        }

        #endregion
    }
}
