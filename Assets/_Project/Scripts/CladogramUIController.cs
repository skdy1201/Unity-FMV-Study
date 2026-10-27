using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 분기도 화면 UI를 보이고 숨기고, 배경을 띄우고, 카드를 생성하며, 드래그로 이동하고 휠로 확대·축소한다
    /// </summary>
    /// <remarks>
    /// 분기도 Canvas에 붙이고, 같은 오브젝트의 CanvasGroup으로 켜고 끈다.
    /// 배경 Sprite는 Inspector에서 넣고 Awake에서 배경 Image에 적용한다.
    /// 씬에 들어오면 분기도가 먼저 나오므로 보이는 상태로 시작한다. 언제 끌지는 바깥 컨트롤러가 정하고, 여기서는 Show/Hide 호출만 받는다.
    /// 드래그·휠은 EventSystem 이벤트로 받으므로, Hide로 레이캐스트가 막히면 입력도 자동으로 끊긴다.
    /// 카드는 드래그를 받지 않아 카드 위에서 끌어도 이벤트가 이 오브젝트까지 올라온다
    /// </remarks>
    [RequireComponent(typeof(CanvasGroup))]
    public class CladogramUIController : MonoBehaviour, IUIVisible, IDragHandler, IScrollHandler
    {
        #region Serialized Fields

        [Header("Background")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Sprite backgroundSprite;

        [Header("Card")]
        [SerializeField] private CladogramCard cardPrefab;
        [SerializeField] private RectTransform cardContent;

        [Header("Line")]
        [SerializeField] private Sprite openedLineSprite;
        [SerializeField] private Sprite lockedLineSprite;
        [SerializeField] private Sprite jointSprite;
        [SerializeField] private float lineThickness = 25f;
        [Tooltip("꺾이는 점에 놓는 joint의 가로·세로 크기")]
        [SerializeField] private float jointSize = 40f;

        [Header("Zoom")]
        [Tooltip("휠 한 칸에 바뀌는 배율")]
        [SerializeField] private float zoomStep = 0.1f;
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 1.5f;

        [Header("Button")]
        [Tooltip("카드 영역 밖(Canvas 바로 아래)에 두어 드래그·확대에 따라 움직이지 않게 한다")]
        [SerializeField] private ButtonSetting backButton;

        #endregion

        #region Private Fields

        private CanvasGroup group;
        private Canvas canvas;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            canvas = GetComponent<Canvas>();
            backgroundImage.sprite = backgroundSprite;
        }

        #endregion

        #region IUIVisible

        /// <summary>
        /// 분기도를 보이고 입력을 받게 한다
        /// </summary>
        public void Show()
        {
            SetGroupVisible(true);
        }

        /// <summary>
        /// 분기도를 숨기고 입력을 막는다
        /// </summary>
        public void Hide()
        {
            SetGroupVisible(false);
        }

        #endregion

        #region Button

        /// <summary>
        /// 뒤로 가기 버튼에 클릭 기능을 연결한다
        /// </summary>
        /// <remarks>
        /// 버튼은 프리팹 안에 있어 바깥 컨트롤러가 직접 참조할 수 없으므로, 어디로 갈지는 부른 쪽이 넘긴다
        /// </remarks>
        /// <param name="action"> 클릭 시 호출할 기능 </param>
        public void BindBack(UnityAction action)
        {
            if (backButton.ButtonType != ButtonType.Back)
            {
                Debug.LogError($"Not Match {ButtonType.Back} Button Type: {backButton.name}");
                return;
            }

            backButton.Bind(action);
        }

        #endregion

        #region Card

        /// <summary>
        /// 카드 프리팹을 카드 영역(Content) 아래에 생성한다
        /// </summary>
        /// <remarks>
        /// 값(id, 썸네일)과 위치는 채우지 않은 채로 돌려주므로, 부른 쪽에서 채운다
        /// </remarks>
        /// <returns> 생성한 카드 </returns>
        public CladogramCard CreateCard()
        {
            return Instantiate(cardPrefab, cardContent);
        }

        #endregion

        #region Line

        /// <summary>
        /// 두 점 사이에 선 한 조각을 카드와 같은 cardContent 아래에 만든다
        /// </summary>
        /// <remarks>
        /// 카드와 같은 부모라 카드 좌표를 그대로 쓰고, 드래그·확대도 함께 따라간다.
        /// UI는 형제 순서대로 그려지므로, 카드보다 먼저 만들어야 카드 뒤에 깔린다.
        /// 선 스프라이트를 Tiled로 길이 방향으로 반복해 늘려도 점선 간격이 유지된다.
        /// 가로로 만든 뒤 두 점의 방향만큼 돌리므로, 세로 조각은 90도 돌린 가로 선이 된다.
        /// 분기도는 꺾인 선으로만 이으므로 가로·세로 조각만 넘긴다
        /// </remarks>
        /// <param name="start"> 시작점 (cardContent 기준 좌표) </param>
        /// <param name="end"> 끝점 (cardContent 기준 좌표) </param>
        /// <param name="isOpened"> 열린 분기로 이어지면 true, 잠긴 분기면 false </param>
        /// <returns> 만든 선 Image </returns>
        public Image CreateLine(Vector2 start, Vector2 end, bool isOpened)
        {
            GameObject line = new GameObject("Line", typeof(RectTransform), typeof(Image));
            line.transform.SetParent(cardContent, false);

            Image image = line.GetComponent<Image>();
            image.sprite = isOpened ? openedLineSprite : lockedLineSprite;
            image.type = Image.Type.Tiled;

            // 선은 카드 뒤 장식이라 드래그·호버를 가로채지 않게 한다
            image.raycastTarget = false;

            Vector2 direction = end - start;

            // 피벗이 가운데이므로 두 점의 중간에 놓고, 길이만큼 늘린 뒤 방향만큼 돌린다
            RectTransform rect = (RectTransform)line.transform;
            rect.anchoredPosition = (start + end) * 0.5f;
            rect.sizeDelta = new Vector2(direction.magnitude, lineThickness);
            rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

            return image;
        }

        /// <summary>
        /// 선이 꺾이는 점에 joint 하나를 cardContent 아래에 만든다
        /// </summary>
        /// <remarks>
        /// 가로·세로 조각이 만나는 모서리를 덮으므로, 선보다 나중에·카드보다 먼저 만든다
        /// </remarks>
        /// <param name="position"> 꺾이는 점 (cardContent 기준 좌표) </param>
        /// <returns> 만든 joint Image </returns>
        public Image CreateJoint(Vector2 position)
        {
            GameObject joint = new GameObject("Joint", typeof(RectTransform), typeof(Image));
            joint.transform.SetParent(cardContent, false);

            Image image = joint.GetComponent<Image>();
            image.sprite = jointSprite;
            image.raycastTarget = false;

            RectTransform rect = (RectTransform)joint.transform;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(jointSize, jointSize);

            return image;
        }

        #endregion

        #region Navigation

        /// <summary>
        /// 드래그한 만큼 카드 영역을 마우스와 같은 방향으로 옮긴다
        /// </summary>
        /// <remarks>
        /// delta는 화면 픽셀 단위이고 anchoredPosition은 Canvas 단위이므로, Canvas Scaler의 배율로 나눠 맞춘다
        /// </remarks>
        public void OnDrag(PointerEventData eventData)
        {
            cardContent.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }

        /// <summary>
        /// 휠을 굴린 만큼 카드 영역을 확대·축소하되, 마우스 아래에 있던 지점이 그 자리에 남도록 위치를 보정한다
        /// </summary>
        public void OnScroll(PointerEventData eventData)
        {
            float oldZoom = cardContent.localScale.x;
            float newZoom = Mathf.Clamp(oldZoom + eventData.scrollDelta.y * zoomStep, minZoom, maxZoom);

            if (Mathf.Approximately(oldZoom, newZoom))
            {
                return;
            }

            // Overlay Canvas는 카메라가 없으므로 null을 넘긴다
            RectTransformUtility.ScreenPointToLocalPointInRectangle(cardContent, eventData.position, null, out Vector2 pointer);

            cardContent.localScale = Vector3.one * newZoom;

            // 마우스 아래 지점의 부모 기준 위치는 anchoredPosition + pointer * zoom이다.
            // 배율이 바뀌어도 이 값이 그대로이도록 anchoredPosition을 pointer * (oldZoom - newZoom)만큼 옮긴다
            cardContent.anchoredPosition += pointer * (oldZoom - newZoom);
        }

        #endregion

        #region Helpers

        /// <remarks>
        /// CanvasGroup으로 켜고 끄므로, 보임·입력·레이캐스트를 한 번에 맞춘다
        /// </remarks>
        private void SetGroupVisible(bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        #endregion
    }
}
