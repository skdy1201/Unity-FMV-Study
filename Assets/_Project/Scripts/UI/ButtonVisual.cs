using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FMV
{
    /// <summary>
    /// 버튼 Image의 표시 여부와 스프라이트 교체를 담당한다
    /// </summary>
    public class ButtonVisual : MonoBehaviour
    {
        /// <summary>
        /// 현재 스프라이트(from)일 때 바꿀 스프라이트(to)의 쌍
        /// </summary>
        /// <remarks>
        /// Unity는 ValueTuple을 직렬화하지 못하므로 인스펙터 노출을 위해 struct로 정의한다
        /// </remarks>
        [System.Serializable]
        public struct SpritePair
        {
            public Sprite from;
            public Sprite to;
        }

        #region Serialized Fields

        [SerializeField] private Image buttonImage;
        [SerializeField] private List<SpritePair> spriteChain = new List<SpritePair>();

        #endregion

        #region Private Fields

        private Button unityButton;
        private bool isVisible = false;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (buttonImage == null)
            {
                buttonImage = GetComponent<Image>();
            }

            unityButton = GetComponent<Button>();

            ApplyVisible();
        }

        #endregion

        #region Visual

        /// <summary>
        /// 버튼을 보이거나 숨기고, 숨겼을 때는 클릭도 받지 않게 한다
        /// </summary>
        public void ToggleVisible()
        {
            isVisible = !isVisible;

            ApplyVisible();
        }

        /// <summary>
        /// 버튼을 지정한 상태로 보이거나 숨기고, 숨겼을 때는 클릭도 받지 않게 한다
        /// </summary>
        /// <remarks>
        /// 현재 상태를 모르는 곳에서 ToggleVisible을 부르면 숨겨야 할 버튼이 오히려 보일 수 있어, 상태를 직접 정할 때 쓴다
        /// </remarks>
        /// <param name="visible"> 보이게 하려면 true </param>
        public void SetVisible(bool visible)
        {
            isVisible = visible;

            ApplyVisible();
        }

        /// <summary>
        /// 현재 스프라이트와 일치하는 쌍을 찾아 다음 스프라이트로 교체한다
        /// </summary>
        public void ChangeImage()
        {
            foreach (SpritePair pair in spriteChain)
            {
                if (buttonImage.sprite == pair.from)
                {
                    buttonImage.sprite = pair.to;
                    return;
                }
            }
        }

        /// <remarks>
        /// 알파 0인 Image도 레이캐스트를 받아, 숨긴 자리를 누르면 UI 클릭으로 걸러져 화면 클릭이 되지 않는다.
        /// 그래서 Button.interactable과 함께 raycastTarget도 끈다
        /// </remarks>
        private void ApplyVisible()
        {
            Color buttonColor = buttonImage.color;
            buttonColor.a = isVisible ? 1f : 0f;

            buttonImage.color = buttonColor;
            buttonImage.raycastTarget = isVisible;

            if (unityButton != null)
            {
                unityButton.interactable = isVisible;
            }
        }

        #endregion
    }
}
