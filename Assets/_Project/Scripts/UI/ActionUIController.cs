using System;
using UnityEngine;

namespace FMV
{
    /// <summary>
    /// Action 단계의 선택지·QTE UI를 켜고 끈다
    /// </summary>
    /// <remarks>
    /// 노드마다 선택지 수와 QTE 위치가 달라 정보를 받아 세팅해야 하므로,
    /// 버튼 하나만 다루는 MediaInteractUI와 달리 UI 제어를 이 파츠로 따로 둔다.
    /// UI 프리팹은 GameController가 생성한 뒤 SetSelectUI·SetQTEUI로 등록한다
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

        private CanvasGroup qteUIGroup;
        private QTEUIView qteUIView;

        #endregion

        #region Events

        /// <summary>
        /// 선택지를 고르면, 선택지 UI를 숨긴 뒤 그 선택지의 결과를 넘겨 호출된다
        /// </summary>
        public event Action<NodeResult> OptionSelected;

        /// <summary>
        /// QTE를 판정하면, QTE UI를 숨긴 뒤 성공 여부를 넘겨 호출된다
        /// </summary>
        public event Action<bool> OnQTEChecked;

        #endregion

        #region Select

        /// <summary>
        /// 생성한 선택지 UI를 등록하고, 버튼 선택을 받도록 구독한다
        /// </summary>
        /// <param name="selectInstance"> 생성한 SelectUI 인스턴스. CanvasGroup과 SelectUIView가 있어야 한다 </param>
        public void SetSelectUI(GameObject selectInstance)
        {
            selectUIInstance = selectInstance;
            selectUIGroup = selectUIInstance.GetComponent<CanvasGroup>();
            selectUIView = selectUIInstance.GetComponent<SelectUIView>();
            selectUIView.OptionSelected += SelectOption;
        }

        /// <summary>
        /// 노드 선택지로 버튼을 세팅한 뒤, 선택지 UI를 보이고 입력을 받게 한다
        /// </summary>
        /// <param name="options"> 표시할 노드 선택지 </param>
        public void ActiveSelectUI(JsonNodeOption[] options)
        {
            selectUIView.SetOptions(options);

            SetGroupVisible(selectUIGroup, true);
        }

        /// <summary>
        /// 선택지 UI를 숨기고 입력을 막는다
        /// </summary>
        public void ClearSelectUI()
        {
            SetGroupVisible(selectUIGroup, false);
        }

        /// <remarks>
        /// 숨기기를 먼저 해서 interactable을 꺼, 같은 선택지 UI에서 두 번 눌려 결과가 두 번 넘어가지 않게 한다
        /// </remarks>
        private void SelectOption(NodeResult result)
        {
            ClearSelectUI();
            OptionSelected?.Invoke(result);
        }

        #endregion

        #region QTE

        /// <summary>
        /// 생성한 QTE UI를 등록한다
        /// </summary>
        /// <param name="qteInstance"> 생성한 QTEUI 인스턴스. CanvasGroup과 QTEUIView가 있어야 한다 </param>
        public void SetQTEUI(GameObject qteInstance)
        {
            qteUIGroup = qteInstance.GetComponent<CanvasGroup>();
            qteUIView = qteInstance.GetComponent<QTEUIView>();
        }

        /// <summary>
        /// 제한 시간으로 타이머와 타이밍 원을 시작한 뒤, QTE UI를 보인다
        /// </summary>
        /// <param name="duration"> 제한 시간(초) </param>
        public void ActiveQTEUI(float duration)
        {
            qteUIView.ActiveQTE(duration);

            SetGroupVisible(qteUIGroup, true);
        }

        /// <summary>
        /// QTE UI를 숨긴다
        /// </summary>
        public void ClearQTEUI()
        {
            SetGroupVisible(qteUIGroup, false);
        }

        /// <summary>
        /// 지금 타이밍 원의 크기로 QTE를 판정하고, QTE UI를 숨긴 뒤 결과를 알린다
        /// </summary>
        /// <remarks>
        /// 선택지와 같이 숨기기를 먼저 해서, 결과를 받은 쪽이 다음 분기로 넘어갈 때 QTE UI가 남아 있지 않게 한다
        /// </remarks>
        public void CheckQTE()
        {
            bool isSuccess = qteUIView.CheckQTE();

            ClearQTEUI();
            OnQTEChecked?.Invoke(isSuccess);
        }

        #endregion

        #region Helpers

        /// <remarks>
        /// 선택지와 QTE UI 모두 CanvasGroup으로 켜고 끄므로, 보임·입력·레이캐스트를 한 번에 맞춘다
        /// </remarks>
        private void SetGroupVisible(CanvasGroup group, bool visible)
        {
            group.alpha = visible ? 1f : 0f;
            group.interactable = visible;
            group.blocksRaycasts = visible;
        }

        #endregion
    }
}
