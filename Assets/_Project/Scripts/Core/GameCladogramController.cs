using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 분기도 데이터를 준비하고, GameController와 분기도 화면 사이를 잇는다
    /// </summary>
    /// <remarks>
    /// 화면을 켜고 끄는 일은 CladogramUIController가 맡고, 여기서는 참조로 들고 호출만 한다.
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

        [Header("UI")]
        [SerializeField] private CladogramUIController cladogramUI;

        [Header("Layout")]
        [Tooltip("x: 단계(깊이) 사이 가로 간격, y: 같은 단계 안 카드 사이 세로 간격")]
        [SerializeField] private Vector2 cardSpacing = new Vector2(450f, 350f);

        #endregion

        #region Private Fields

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
        }

        private void Start()
        {
            BuildCladogram();
        }

        private void OnDisable()
        {
            foreach (ISetting set in interfaceList)
            {
                set.End();
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
        /// 분기 id로 카드 정보와 썸네일을 찾아, 썸네일과 제목을 띄운 카드 한 장을 만든다
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

            return card;
        }

        #endregion
    }
}
