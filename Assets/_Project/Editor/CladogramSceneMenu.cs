using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// 상단 FMV 메뉴에서 Cladogram 에셋을 읽어 분기도를 카드와 선 스프라이트로 배치한 씬을 만든다
    /// </summary>
    /// <remarks>
    /// 분기도 에셋이 어떻게 보이는지 확인하기 위한 임시 배치다. 위치는 시작 분기에서 몇 단계 떨어졌는지로만 정하므로, 필요하면 씬에서 직접 옮긴다.
    /// save.json에서 방문한 분기는 썸네일 카드, 방문하지 않은 분기는 자물쇠로 그린다.
    /// 다시 실행하면 씬을 새로 만들어 덮어쓰므로 손으로 옮긴 위치는 사라진다
    /// </remarks>
    public static class CladogramSceneMenu
    {
        #region Private Fields

        private const string ScenePath = "Assets/_Project/Scenes/Dev/CladogramScene.unity";
        private const string VideoJsonPath = "Assets/_Project/Data/Json/videos.json";
        private const string BranchImageFolder = "Assets/_Project/Data/Image/Branch";
        private const string ThumbnailFolder = "Assets/_Project/Data/Image/Thumbnail";
        private const string FontPath = "Assets/_Project/Fonts/Galmuri/Galmuri11 SDF.asset";

        private const float DepthSpacing = 4f;
        private const float RowSpacing = 2.6f;
        private const float CameraMargin = 0.5f;

        // 썸네일 칸 중심과 제목 띠 중심의 카드 중심 기준 높이. 위 테두리 20px, 아래 띠 55px라 썸네일 칸이 위로 치우친다
        private const float WindowOffsetY = 0.175f;
        private const float TitleOffsetY = -0.65f;
        private const float TitleFontSize = 2f;
        private const float LineThickness = 0.25f;

        // 뒤에서부터 그리는 순서. 지나온 길이 안 간 길에 덮이지 않도록 열린 선을 위에 둔다
        private const int BackgroundOrder = -100;
        private const int LockedLineOrder = 0;
        private const int UnlockedLineOrder = 1;
        private const int JointOrder = 2;
        private const int ThumbnailOrder = 10;
        private const int FrameOrder = 11;
        private const int TitleOrder = 12;

        // branch_frame을 9-slice로 세로만 늘려 썸네일 칸을 원본 160x90(16:9)에서 160x120(4:3)으로 맞춘 크기
        private static readonly Vector2 CardSize = new Vector2(2f, 1.95f);
        private static readonly Vector2 WindowSize = new Vector2(1.6f, 1.2f);

        #endregion

        #region Menu

        [MenuItem("FMV/Build Cladogram Scene")]
        private static void BuildCladogramScene()
        {
            Cladogram cladogram = FindCladogram();
            if (cladogram == null)
            {
                Debug.LogError("Cladogram asset not found");
                return;
            }

            Sprite frameSprite = LoadSprite(BranchImageFolder, "branch_frame");
            Sprite lockSprite = LoadSprite(BranchImageFolder, "branch_locked");
            Sprite unlockedLineSprite = LoadSprite(BranchImageFolder, "line_unlocked");
            Sprite lockedLineSprite = LoadSprite(BranchImageFolder, "line_locked");
            Sprite jointSprite = LoadSprite(BranchImageFolder, "line_joint");
            Sprite backgroundSprite = LoadSprite(BranchImageFolder, "map_bg");
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);

            if (frameSprite == null || lockSprite == null || unlockedLineSprite == null || lockedLineSprite == null
                || jointSprite == null || backgroundSprite == null || font == null)
            {
                Debug.LogError("Cladogram scene assets not found");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null
                && !EditorUtility.DisplayDialog("Build Cladogram Scene", $"{ScenePath}를 새로 만들어 덮어씁니다.", "덮어쓰기", "취소"))
            {
                return;
            }

            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Dictionary<string, Vector3> positions = CalculatePositions(cladogram);
            Dictionary<string, string> clipNames = LoadClipNames();
            HashSet<string> visited = new HashSet<string>(SaveFile.Load().Progress.Visited);

            Transform nodeRoot = new GameObject("Nodes").transform;
            foreach (KeyValuePair<string, Vector3> pair in positions)
            {
                if (visited.Contains(pair.Key))
                {
                    Sprite thumbnail = clipNames.TryGetValue(pair.Key, out string clipName) ? LoadSprite(ThumbnailFolder, clipName) : null;
                    CreateCard(pair.Key, pair.Value, frameSprite, thumbnail, font, nodeRoot);
                }
                else
                {
                    CreateLock(pair.Key, pair.Value, lockSprite, nodeRoot);
                }
            }

            Transform edgeRoot = new GameObject("Edges").transform;
            foreach (Edge edge in cladogram.Edges)
            {
                if (!positions.TryGetValue(edge.StartID, out Vector3 start) || !positions.TryGetValue(edge.NextID, out Vector3 end))
                {
                    continue;
                }

                // 양쪽 분기를 모두 지나왔어야 지나온 길이다
                if (visited.Contains(edge.StartID) && visited.Contains(edge.NextID))
                {
                    CreateEdge(edge, start, end, unlockedLineSprite, jointSprite, UnlockedLineOrder, edgeRoot);
                }
                else
                {
                    // 이음새 이미지는 금색뿐이라, 점선인 안 간 길에는 붙이지 않는다
                    CreateEdge(edge, start, end, lockedLineSprite, null, LockedLineOrder, edgeRoot);
                }
            }

            Camera camera = Camera.main;
            if (camera != null)
            {
                FrameCamera(camera, positions.Values);
                CreateBackground(camera, backgroundSprite);
            }

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            Debug.Log($"Cladogram scene saved: {ScenePath}");
        }

        /// <summary>
        /// 프로젝트에서 처음 찾은 Cladogram 에셋을 불러옴
        /// </summary>
        /// <returns> 없으면 null </returns>
        private static Cladogram FindCladogram()
        {
            string[] guids = AssetDatabase.FindAssets("t:Cladogram");
            if (guids.Length == 0)
            {
                return null;
            }

            if (guids.Length > 1)
            {
                Debug.LogWarning($"Cladogram asset {guids.Length}개 중 첫 번째를 사용합니다");
            }

            return AssetDatabase.LoadAssetAtPath<Cladogram>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }

        #endregion

        #region Load

        /// <returns> 없으면 경고를 남기고 null </returns>
        private static Sprite LoadSprite(string folder, string fileName)
        {
            string path = $"{folder}/{fileName}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"Sprite not found: {path}");
            }

            return sprite;
        }

        /// <summary>
        /// videos.json을 읽어 분기 id → 클립 이름 표를 만든다
        /// </summary>
        /// <remarks>
        /// 썸네일 파일 이름은 클립 이름과 같으므로, 분기 id로 썸네일을 찾을 때 쓴다
        /// </remarks>
        private static Dictionary<string, string> LoadClipNames()
        {
            Dictionary<string, string> clipNames = new Dictionary<string, string>();

            TextAsset json = AssetDatabase.LoadAssetAtPath<TextAsset>(VideoJsonPath);
            if (json == null)
            {
                Debug.LogWarning($"Video json not found: {VideoJsonPath}");
                return clipNames;
            }

            foreach (JsonVideoInfo info in JsonUtility.FromJson<JsonVideoTable>(json.text).Videos)
            {
                clipNames[info.ID] = info.Clip;
            }

            return clipNames;
        }

        #endregion

        #region Layout

        /// <summary>
        /// 시작 분기에서 BFS로 단계를 매겨, 단계는 x축, 같은 단계 안의 순서는 y축으로 배치
        /// </summary>
        /// <remarks>
        /// 시작 분기에서 닿지 않는 분기는 배치하지 않는다
        /// </remarks>
        private static Dictionary<string, Vector3> CalculatePositions(Cladogram cladogram)
        {
            Dictionary<string, List<string>> nextIds = new Dictionary<string, List<string>>();
            foreach (Edge edge in cladogram.Edges)
            {
                if (!nextIds.TryGetValue(edge.StartID, out List<string> list))
                {
                    list = new List<string>();
                    nextIds.Add(edge.StartID, list);
                }

                list.Add(edge.NextID);
            }

            Dictionary<string, int> depths = new Dictionary<string, int> { { cladogram.StartID, 0 } };
            List<List<string>> layers = new List<List<string>> { new List<string> { cladogram.StartID } };
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(cladogram.StartID);

            while (queue.Count > 0)
            {
                string id = queue.Dequeue();
                if (!nextIds.TryGetValue(id, out List<string> list))
                {
                    continue;
                }

                foreach (string nextId in list)
                {
                    if (string.IsNullOrEmpty(nextId) || depths.ContainsKey(nextId))
                    {
                        continue;
                    }

                    int depth = depths[id] + 1;
                    depths.Add(nextId, depth);
                    if (layers.Count <= depth)
                    {
                        layers.Add(new List<string>());
                    }

                    layers[depth].Add(nextId);
                    queue.Enqueue(nextId);
                }
            }

            Dictionary<string, Vector3> positions = new Dictionary<string, Vector3>();
            for (int depth = 0; depth < layers.Count; ++depth)
            {
                List<string> layer = layers[depth];
                for (int row = 0; row < layer.Count; ++row)
                {
                    // 같은 단계의 분기들을 y = 0 기준 위아래로 고르게 펼친다
                    float y = ((layer.Count - 1) * 0.5f - row) * RowSpacing;
                    positions.Add(layer[row], new Vector3(depth * DepthSpacing, y, 0f));
                }
            }

            return positions;
        }

        #endregion

        #region Create

        /// <summary>
        /// 방문한 분기를 카드 틀, 썸네일, 분기 id 제목으로 만든다
        /// </summary>
        /// <param name="thumbnail"> 없으면 빈 칸으로 둔다 </param>
        private static void CreateCard(string id, Vector3 position, Sprite frame, Sprite thumbnail, TMP_FontAsset font, Transform parent)
        {
            Transform card = new GameObject(id).transform;
            card.SetParent(parent);
            card.position = position;

            SpriteRenderer frameRenderer = CreateSprite("Frame", frame, FrameOrder, card);
            frameRenderer.drawMode = SpriteDrawMode.Sliced;
            frameRenderer.size = CardSize;

            if (thumbnail != null)
            {
                SpriteRenderer thumbnailRenderer = CreateSprite("Thumbnail", thumbnail, ThumbnailOrder, card);
                thumbnailRenderer.transform.localPosition = new Vector3(0f, WindowOffsetY, 0f);

                // 썸네일 원본 크기와 상관없이 썸네일 칸에 꽉 맞춘다
                Vector2 spriteSize = thumbnail.bounds.size;
                thumbnailRenderer.transform.localScale = new Vector3(WindowSize.x / spriteSize.x, WindowSize.y / spriteSize.y, 1f);
            }

            TextMeshPro title = new GameObject("Title").AddComponent<TextMeshPro>();
            title.transform.SetParent(card, false);
            title.transform.localPosition = new Vector3(0f, TitleOffsetY, 0f);
            title.rectTransform.sizeDelta = new Vector2(WindowSize.x, LineThickness);
            title.font = font;
            title.fontSize = TitleFontSize;
            title.alignment = TextAlignmentOptions.Center;
            title.textWrappingMode = TextWrappingModes.NoWrap;
            title.sortingOrder = TitleOrder;
            title.text = id;
        }

        private static void CreateLock(string id, Vector3 position, Sprite lockSprite, Transform parent)
        {
            SpriteRenderer renderer = CreateSprite(id, lockSprite, ThumbnailOrder, parent);
            renderer.transform.position = position;
        }

        /// <summary>
        /// 두 분기를 가로 → 세로 → 가로로 직각으로 꺾어 잇는다
        /// </summary>
        /// <remarks>
        /// 높이가 같으면 꺾지 않고 바로 잇는다. 선은 분기 중심끼리 이어 카드 뒤로 지나가게 한다
        /// </remarks>
        /// <param name="jointSprite"> 꺾이는 곳에 붙일 이음새. null이면 붙이지 않는다 </param>
        private static void CreateEdge(Edge edge, Vector3 start, Vector3 end, Sprite lineSprite, Sprite jointSprite, int order, Transform parent)
        {
            Transform root = new GameObject($"{edge.StartID} -> {edge.NextID} ({edge.Result})").transform;
            root.SetParent(parent);

            if (Mathf.Approximately(start.y, end.y))
            {
                CreateLineSegment(start, end, lineSprite, order, root);
                return;
            }

            // 두 분기 사이 가운데에서 세로로 꺾는다
            float middleX = (start.x + end.x) * 0.5f;
            Vector3 startCorner = new Vector3(middleX, start.y, 0f);
            Vector3 endCorner = new Vector3(middleX, end.y, 0f);

            CreateLineSegment(start, startCorner, lineSprite, order, root);
            CreateLineSegment(startCorner, endCorner, lineSprite, order, root);
            CreateLineSegment(endCorner, end, lineSprite, order, root);

            if (jointSprite != null)
            {
                CreateSprite("Joint", jointSprite, JointOrder, root).transform.position = startCorner;
                CreateSprite("Joint", jointSprite, JointOrder, root).transform.position = endCorner;
            }
        }

        /// <summary>
        /// 선 이미지를 Tiled로 이어 붙여 두 점 사이 가로 또는 세로 구간을 그린다
        /// </summary>
        /// <remarks>
        /// 선 이미지는 가로 방향이라 세로 구간은 90도 돌려 그린다
        /// </remarks>
        private static void CreateLineSegment(Vector3 from, Vector3 to, Sprite sprite, int order, Transform parent)
        {
            SpriteRenderer renderer = CreateSprite("Line", sprite, order, parent);
            renderer.transform.position = (from + to) * 0.5f;

            if (Mathf.Approximately(from.x, to.x))
            {
                renderer.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }

            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = new Vector2(Vector3.Distance(from, to), LineThickness);
        }

        private static SpriteRenderer CreateSprite(string name, Sprite sprite, int order, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;

            return renderer;
        }

        /// <summary>
        /// 기본 카메라를 정면 직교 카메라로 바꿔 분기도 전체가 보이게 맞춤
        /// </summary>
        private static void FrameCamera(Camera camera, IEnumerable<Vector3> positions)
        {
            bool hasBounds = false;
            Bounds bounds = new Bounds();
            foreach (Vector3 position in positions)
            {
                // 분기 중심만 담으면 가장자리 카드가 잘리므로 카드 크기만큼 담는다
                Bounds card = new Bounds(position, CardSize);
                if (hasBounds)
                {
                    bounds.Encapsulate(card);
                }
                else
                {
                    bounds = card;
                    hasBounds = true;
                }
            }

            camera.orthographic = true;
            camera.transform.SetPositionAndRotation(new Vector3(bounds.center.x, bounds.center.y, -10f), Quaternion.identity);
            camera.orthographicSize = Mathf.Max(bounds.extents.y, bounds.extents.x / camera.aspect) + CameraMargin;
        }

        /// <summary>
        /// 배경을 카메라 자식으로 두고, 비율을 유지한 채 카메라 화면을 덮도록 키운다
        /// </summary>
        /// <remarks>
        /// 카메라를 옮겨도 배경은 따라오므로, 분기도가 길어져 카메라를 움직여도 배경이 끊기지 않는다
        /// </remarks>
        private static void CreateBackground(Camera camera, Sprite sprite)
        {
            SpriteRenderer renderer = CreateSprite("Background", sprite, BackgroundOrder, camera.transform);
            renderer.transform.localPosition = new Vector3(0f, 0f, 10f);

            float viewHeight = camera.orthographicSize * 2f;
            float viewWidth = viewHeight * camera.aspect;
            Vector2 spriteSize = sprite.bounds.size;
            renderer.transform.localScale = Vector3.one * Mathf.Max(viewWidth / spriteSize.x, viewHeight / spriteSize.y);
        }

        #endregion
    }
}
