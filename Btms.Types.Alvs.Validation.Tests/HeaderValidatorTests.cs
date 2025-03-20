using FluentValidation;
using FluentValidation.TestHelper;

namespace Btms.Types.Alvs.Validation.Tests;

public class HeaderValidatorTests
{
    private HeaderValidator validator = new("123");

    [Theory]
    [ClassData(typeof(HeaderValidatorTestData))]
    public void TheoryTests(Header model, ExpectedResult expectedResult)
    {
        var result = validator.TestValidate(model);

        if (expectedResult.HasValidationError)
        {
            result.ShouldHaveValidationErrorFor(expectedResult.PropertyName);
        }
        else
        {
            result.ShouldNotHaveValidationErrorFor(expectedResult.PropertyName);
        }
    }

    public class HeaderValidatorTestData : TheoryData<Header, ExpectedResult>
    {
        public HeaderValidatorTestData()
        {
            Add(new Header { EntryReference = "15GB1245fst7s8g9s4" }, new ExpectedResult(nameof(Header.EntryReference), false));
            Add(new Header { EntryReference = "invalid" }, new ExpectedResult(nameof(Header.EntryReference), true));
            Add(new Header { EntryReference = null }, new ExpectedResult(nameof(Header.EntryReference), true));

            Add(new Header { EntryVersionNumber = 2 }, new ExpectedResult(nameof(Header.EntryVersionNumber), false));
            Add(new Header { EntryVersionNumber = -1 }, new ExpectedResult(nameof(Header.EntryVersionNumber), true));
            Add(new Header { EntryVersionNumber = 100 }, new ExpectedResult(nameof(Header.EntryVersionNumber), true));
            Add(new Header { EntryVersionNumber = null }, new ExpectedResult(nameof(Header.EntryVersionNumber), true));

            Add(new Header { EntryVersionNumber = 2, PreviousVersionNumber = 1 }, new ExpectedResult(nameof(Header.EntryVersionNumber), false));
            Add(new Header { EntryVersionNumber = 1, PreviousVersionNumber = 2 }, new ExpectedResult(nameof(Header.EntryVersionNumber), false));

            Add(new Header { DeclarationUcr = "Valid" }, new ExpectedResult(nameof(Header.DeclarationUcr), false));
            Add(new Header { DeclarationUcr = "1234567891234567891233fghytfcdsertgy" }, new ExpectedResult(nameof(Header.DeclarationUcr), true));
            Add(new Header { DeclarationUcr = null }, new ExpectedResult(nameof(Header.DeclarationUcr), true));

            Add(new Header { DeclarationType = "F" }, new ExpectedResult(nameof(Header.DeclarationType), false));
            Add(new Header { DeclarationType = "T" }, new ExpectedResult(nameof(Header.DeclarationType), true));
            Add(new Header { DeclarationType = null }, new ExpectedResult(nameof(Header.DeclarationType), true));

            Add(new Header { DeclarantId = "valid" }, new ExpectedResult(nameof(Header.DeclarantId), false));
            Add(new Header { DeclarantId = "1234567891234567891233fghytfcdsertgy" }, new ExpectedResult(nameof(Header.DeclarantId), true));
            Add(new Header { DeclarantId = null }, new ExpectedResult(nameof(Header.DeclarantId), true));

            Add(new Header { DeclarantName = "valid" }, new ExpectedResult(nameof(Header.DeclarantName), false));
            Add(new Header { DeclarantName = "1234567891234567891233fghytfcdsertgy" }, new ExpectedResult(nameof(Header.DeclarantName), true));
            Add(new Header { DeclarantName = null }, new ExpectedResult(nameof(Header.DeclarantName), true));

            Add(new Header { DispatchCountryCode = "GB" }, new ExpectedResult(nameof(Header.DispatchCountryCode), false));
            Add(new Header { DispatchCountryCode = "T" }, new ExpectedResult(nameof(Header.DispatchCountryCode), true));
            Add(new Header { DispatchCountryCode = "GBB" }, new ExpectedResult(nameof(Header.DispatchCountryCode), true));
            Add(new Header { DispatchCountryCode = null }, new ExpectedResult(nameof(Header.DispatchCountryCode), true));
        }
    }
}