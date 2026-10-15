using UnityEngine;
using UnityEngine.EventSystems;

namespace FMV
{
    /// <summary>
    /// 이번 프레임의 입력 여부를 돌려준다
    /// </summary>
    /// <remarks>
    /// 입력에 따라 무엇을 할지는 GameController가 단계를 보고 정하고, 여기서는 값만 돌려준다.
    /// TODO: 마감 전에 구형 Input을 신규 Input System으로 교체
    /// </remarks>
    public class GameInput
    {
        #region Click

        /// <summary>
        /// 이번 프레임에 UI가 아닌 화면을 좌클릭했는지 돌려준다
        /// </summary>
        /// <remarks>
        /// UI 버튼을 누른 클릭까지 화면 클릭으로 잡으면, 버튼을 누를 때 UI 표시도 같이 토글되므로 걸러낸다
        /// </remarks>
        public bool IsScreenClicked()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return false;
            }

            return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
        }

        #endregion
    }
}
