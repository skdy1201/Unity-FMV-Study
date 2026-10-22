using System;

namespace FMV
{
    /// <summary>
    /// 업적이 열리는 조건의 종류
    /// </summary>
    /// <remarks>
    /// 업적 하나가 실패 횟수와 엔딩 조건을 함께 가질 수 있어 Flags로 묶는다.
    /// 그래서 switch로 하나만 고르지 않고, 켜진 조건을 if로 하나씩 검사한다
    /// </remarks>
    [Flags]
    public enum AchievementCondition
    {
        None = 0,
        FallCount = 1 << 0,
        Ending = 1 << 1,
    }
}
