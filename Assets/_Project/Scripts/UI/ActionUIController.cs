using UnityEngine;

namespace FMV
{
    /// <summary>
    /// Action 단계의 선택지·QTE UI를 켜고 끈다
    /// </summary>
    /// <remarks>
    /// 노드마다 선택지 수와 QTE 위치가 달라 정보를 받아 세팅해야 하므로,
    /// 버튼 하나만 다루는 MediaInteractUI와 달리 UI 제어를 이 파츠로 따로 둔다.
    /// UI 프리팹은 GameController가 생성한 뒤 SetSelectUI로 등록한다
    /// </remarks>
    public class ActionUIController : MonoBehaviour
    {
        #region Serialized Fields

        // DEBUG: 런타임에 SetSelectUI로 등록되는지 Inspector에서 확인하려고 노출한다
        [SerializeField] private CanvasGroup selectUIGroup;

        #endregion

        #region Private Fields

        private GameObject selectUIInstance;
        private SelectUIView selectUIView;

        #endregion

        #region Select

        /// <summary>
        /// 생성한 선택지 UI를 등록한다
        /// </summary>
        /// <param name="selectInstance"> 생성한 SelectUI 인스턴스. CanvasGroup과 SelectUIView가 있어야 한다 </param>
        public void SetSelectUI(GameObject selectInstance)
        {
            selectUIInstance = selectInstance;
            selectUIGroup = selectUIInstance.GetComponent<CanvasGroup>();
            selectUIView = selectUIInstance.GetComponent<SelectUIView>();
        }

        /// <summary>
        /// 노드 선택지로 버튼을 세팅한 뒤, 선택지 UI를 보이고 입력을 받게 한다
        /// </summary>
        /// <param name="options"> 표시할 노드 선택지 </param>
        public void ActiveSelectUI(JsonNodeOption[] options)
        {
            selectUIView.SetOptions(options);

            selectUIGroup.alpha = 1f;
            selectUIGroup.interactable = true;
            selectUIGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 선택지 UI를 숨기고 입력을 막는다
        /// </summary>
        public void ClearSelectUI()
        {
            selectUIGroup.alpha = 0f;
            selectUIGroup.interactable = false;
            selectUIGroup.blocksRaycasts = false;
        }

        #endregion
    }
}
