namespace Btms.Business.Services.Decisions;

public enum DecisionType
{
    None,
    Ched,
    Iuu
}

public static class DecisionTypeExtensions
{
    private static readonly string[] IuuCheckCodes = ["H224", "C673"];

    public static bool ForCheckCode(this DecisionType decisionType, string? checkCode) => (IuuCheckCodes.Contains(checkCode) && decisionType == DecisionType.Iuu) 
                                                                                          || (!IuuCheckCodes.Contains(checkCode) && decisionType != DecisionType.Iuu);
} 