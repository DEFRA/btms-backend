using FluentValidation.TestHelper;

namespace Btms.Types.Alvs.Validation.Tests;

public class DocumentValidatorTests
{
    private DocumentValidator validator = new(1, "123");

    [Theory]
    [ClassData(typeof(DocumentValidatorTestData))]
    public void TheoryTests(Document model, ExpectedResult expectedResult)
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

    public class DocumentValidatorTestData : TheoryData<Document, ExpectedResult>
    {
        public DocumentValidatorTestData()
        {
            Add(new Document { DocumentCode = "C633" }, new ExpectedResult(nameof(Document.DocumentCode), false));
            Add(new Document { DocumentCode = null }, new ExpectedResult(nameof(Document.DocumentCode), true));

            Add(new Document { DocumentControl = "AE" }, new ExpectedResult(nameof(Document.DocumentControl), false));
            Add(new Document { DocumentControl = "AEE" }, new ExpectedResult(nameof(Document.DocumentControl), true));
            Add(new Document { DocumentControl = null }, new ExpectedResult(nameof(Document.DocumentControl), true));

            Add(new Document { DocumentStatus = "P" }, new ExpectedResult(nameof(Document.DocumentStatus), false));
            Add(new Document { DocumentStatus = "PP" }, new ExpectedResult(nameof(Document.DocumentStatus), true));
            Add(new Document { DocumentStatus = null }, new ExpectedResult(nameof(Document.DocumentStatus), true));
        }
    }
}