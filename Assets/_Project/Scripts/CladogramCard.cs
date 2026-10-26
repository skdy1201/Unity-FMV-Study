using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 분기도에 놓이는 카드 한 장. 분기 id를 가지고 썸네일과 제목을 띄우며, 진행 상태로 색과 잠김을 바꾸고 호버 중에는 크기가 커졌다 작아진다
    /// </summary>
    /// <remarks>
    /// 분기도가 코드로 생성한 뒤 setter로 값을 넣는다. 썸네일과 제목은 따로 들고 있지 않고 카드의 Image와 텍스트에 바로 띄운다.
    /// 호버는 마우스가 들어오고 나가는 순간에만 이벤트를 받아, 호버 중인 카드에서만 코루틴이 돈다.
    /// 클릭되면 Clicked로 분기 id만 알리고, 그 id로 무엇을 할지는 분기도 컨트롤러가 정한다
    /// </remarks>
    public class CladogramCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        #region Serialized Fields

        [SerializeField] private string id;

        [Header("UI")]
        [SerializeField] private Image frameImage;
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Image lockImage;

        [Header("State")]
        [FormerlySerializedAs("visitedColor")]
        [SerializeField] private Color watchedColor = Color.white;

        [Header("Hover")]
        [Tooltip("호버 중 가장 커졌을 때의 배율")]
        [SerializeField] private float hoverScale = 1.05f;
        [Tooltip("커졌다 작아지는 속도")]
        [SerializeField] private float hoverSpeed = 6f;

        #endregion

        #region Private Fields

        private Coroutine hoverRoutine;
        private CardState state = CardState.Locked;

        // 끝까지 보지 않은 카드는 프리팹에 정해 둔 원래 색을 그대로 쓴다
        private Color frameOriginalColor;
        private Color thumbnailOriginalColor;

        #endregion

        #region Events

        /// <summary>
        /// 열렸거나 본 카드를 좌클릭하면 카드의 분기 id를 넘겨 호출된다
        /// </summary>
        public event Action<string> Clicked;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            frameOriginalColor = frameImage.color;
            thumbnailOriginalColor = thumbnailImage.color;
        }

        private void OnDisable()
        {
            // 꺼지면 코루틴도 멈추므로, 커진 채로 남지 않게 원래 크기로 돌린다
            StopHover();
        }

        #endregion

        #region Setter

        /// <summary>
        /// 카드가 가리킬 분기 id를 정한다
        /// </summary>
        public void SetId(string id)
        {
            this.id = id;
        }

        /// <summary>
        /// 카드에 썸네일과 제목을 띄운다
        /// </summary>
        /// <param name="thumbnail"> 썸네일 Image에 띄울 Sprite </param>
        /// <param name="title"> 제목 텍스트 </param>
        public void SetThumbnail(Sprite thumbnail, string title)
        {
            thumbnailImage.sprite = thumbnail;
            titleText.text = title;
        }

        /// <summary>
        /// 진행 상태에 맞게 카드를 띄운다. 본 카드는 본 색, 열린 카드는 원래 색, 잠긴 카드는 프레임·썸네일·제목을 가리고 자물쇠만 띄운다
        /// </summary>
        /// <remarks>
        /// Image 색은 원본 Sprite에 곱해지므로, 원래 색보다 밝게 보이려면 프리팹의 원래 색을 본 색보다 어둡게 둔다.
        /// 잠긴 카드도 자리는 그대로 두어 분기도의 모양은 보이게 한다
        /// </remarks>
        /// <param name="state"> save.json 기록으로 정한 카드 상태 </param>
        public void SetState(CardState state)
        {
            this.state = state;

            bool isLocked = state == CardState.Locked;
            bool isWatched = state == CardState.Watched;

            // 잠긴 카드는 자물쇠만 남긴다. 프레임이 꺼지면 레이캐스트도 받지 않아 호버·클릭이 걸리지 않는다
            frameImage.enabled = !isLocked;
            thumbnailImage.enabled = !isLocked;
            titleText.enabled = !isLocked;
            lockImage.enabled = isLocked;

            frameImage.color = isWatched ? watchedColor : frameOriginalColor;
            thumbnailImage.color = isWatched ? watchedColor : thumbnailOriginalColor;
        }

        #endregion

        #region Click

        /// <summary>
        /// 열렸거나 본 카드를 좌클릭하면 Clicked로 분기 id를 알린다
        /// </summary>
        /// <remarks>
        /// 잠긴 카드는 고를 수 없으므로 무시한다.
        /// 분기도를 끌다가 손을 뗀 경우도 클릭으로 잡히지 않도록 드래그 중이었으면 무시한다
        /// </remarks>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || eventData.dragging)
            {
                return;
            }

            if (state == CardState.Locked)
            {
                return;
            }

            Clicked?.Invoke(id);
        }

        #endregion

        #region Hover

        /// <summary>
        /// 마우스가 올라오면 크기가 커졌다 작아지는 연출을 시작한다
        /// </summary>
        /// <remarks>
        /// 그리는 순서는 바꾸지 않으므로, 커져도 옆 카드와 겹치지 않게 분기도의 카드 간격을 넉넉히 둔다
        /// </remarks>
        public void OnPointerEnter(PointerEventData eventData)
        {
            StopHover();
            hoverRoutine = StartCoroutine(HoverPulse());
        }

        /// <summary>
        /// 마우스가 나가면 연출을 멈추고 원래 크기로 돌린다
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            StopHover();
        }

        /// <remarks>
        /// 1배에서 시작해 hoverScale까지 커졌다 다시 1배로 돌아오기를 반복한다. 멈추는 건 StopHover가 맡는다
        /// </remarks>
        private IEnumerator HoverPulse()
        {
            float time = 0f;

            while (true)
            {
                time += Time.deltaTime;

                // (1 - cos) / 2는 0에서 시작해 0~1을 오가므로, 시작 순간 1배에서 튀지 않고 부드럽게 커진다
                float wave = (1f - Mathf.Cos(time * hoverSpeed)) * 0.5f;
                transform.localScale = Vector3.one * Mathf.Lerp(1f, hoverScale, wave);

                yield return null;
            }
        }

        /// <summary>
        /// 호버 연출을 멈추고 원래 크기로 돌린다
        /// </summary>
        private void StopHover()
        {
            if (hoverRoutine != null)
            {
                StopCoroutine(hoverRoutine);
                hoverRoutine = null;
            }

            transform.localScale = Vector3.one;
        }

        #endregion
    }
}
