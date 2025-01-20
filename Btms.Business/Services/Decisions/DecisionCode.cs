namespace Btms.Business.Services.Decisions;

public enum DecisionCode
{
    H01,
    H02,
    E03,
    C03,
    C05,
    C06,
    C07,
    C08,
    N02,
    N03,
    N04,
    N07,
    X00,
    
    E94,    // IUU not indicated in PartTwo?.ControlAuthority?.IuuCheckRequired but "H224" requested in Items[]?.Checks[]?.CheckCode
    E95,    // Unexpected value in PartTwo?.Decision?.IuuOption
    E96,    // Unexpected value in PartTwo?.Decision?.DecisionEnum
    E97,    // Unexpected value in PartTwo?.Decision?.NotAcceptableAction
    E98,    // Not implemented
    E99     // Other unexpected data error
}