using FluentValidation.TestHelper;

namespace Btms.Types.Alvs.Validation.Tests;

public class FinalisationHeaderValidatorTests
{
    private FinalisationHeaderValidator validator = new();

    [Theory]
    [ClassData(typeof(FinalisationHeaderValidatorTestData))]
    public void TheoryTests(FinalisationHeader model, ExpectedResult expectedResult)
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

    public class FinalisationHeaderValidatorTestData : TheoryData<FinalisationHeader, ExpectedResult>
    {
        public FinalisationHeaderValidatorTestData()
        {

            Add(CreateHeader(entryReference: "15GB1245fst7s8g9s4"), new ExpectedResult(nameof(FinalisationHeader.EntryReference), false));
            Add(CreateHeader(entryReference: "invalid"), new ExpectedResult(nameof(FinalisationHeader.EntryReference), true));

            Add(CreateHeader(entryVersionNumber: 0), new ExpectedResult(nameof(FinalisationHeader.EntryVersionNumber), true));
            Add(CreateHeader(entryVersionNumber: 100), new ExpectedResult(nameof(FinalisationHeader.EntryVersionNumber), true));

            Add(CreateHeader(manualAction: "P"), new ExpectedResult(nameof(FinalisationHeader.ManualAction), true));
        }

        private FinalisationHeader CreateHeader(string entryReference = "test", int entryVersionNumber = 1, string finalState = "F", string manualAction = "Y")
        {
            return new FinalisationHeader()
            {
                EntryVersionNumber = entryVersionNumber,
                EntryReference = entryReference,
                FinalState = finalState,
                ManualAction = manualAction
            };
        }
    }
}