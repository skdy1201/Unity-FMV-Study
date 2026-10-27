namespace FMV
{
    /// <summary>
    /// 분기도 카드의 진행 상태
    /// </summary>
    /// <remarks>
    /// save.json의 기록으로 정한다. watched에 있으면 Watched, visited에만 있으면 Opened, 둘 다 없으면 Locked다
    /// </remarks>
    public enum CardState
    {
        Locked,
        Opened,
        Watched,
    }
}
