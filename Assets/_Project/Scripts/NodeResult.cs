namespace FMV
{
    /// <summary>
    /// 분기 하나가 어떻게 끝났는지를 나타내는 결과
    /// </summary>
    /// <remarks>
    /// Cladogram 에셋에 숫자로 저장되므로 값을 명시한다.
    /// 중간에 끼워 넣으면 저장된 데이터가 밀리니, 새 값은 맨 끝에만 추가한다
    /// </remarks>
    public enum NodeResult
    {
        Finished = 0,
        Success = 1,
        Fail = 2,
        Option1 = 3,
        Option2 = 4,
        Option3 = 5,
    }
}
