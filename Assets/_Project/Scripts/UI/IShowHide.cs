namespace FMV
{
    /// <summary>
    /// 보이고 숨길 수 있는 UI의 계약
    /// </summary>
    /// <remarks>
    /// 켜고 끄는 방식은 타입마다 다를 수 있어(목록은 바로 토글, 팝업은 연출 등) 구현에 맡긴다.
    /// Unity Inspector는 인터페이스 필드를 직렬화하지 못하므로, 참조는 구현한 구체 컴포넌트로 들고 호출만 이 계약으로 한다
    /// </remarks>
    public interface IShowHide
    {
        /// <summary>
        /// UI를 보인다
        /// </summary>
        void Show();

        /// <summary>
        /// UI를 숨긴다
        /// </summary>
        void Hide();
    }
}
