using FluentValidation.TestHelper;
using System;

namespace Btms.Types.Alvs.Validation.Tests
{
    public class CheckValidatorTests
    {
        private CheckValidator validator = new("123");

        [Theory]
        [ClassData(typeof(CheckValidatorTestData))]
        public void TheoryTests(Check check, ExpectedResult expectedResult)
        {
            var result = validator.TestValidate(check);

            if (expectedResult.HasValidationError)
            {
                result.ShouldHaveValidationErrorFor(expectedResult.PropertyName);
            }
            else
            {
                result.ShouldNotHaveValidationErrorFor(expectedResult.PropertyName);
            }
        }

        public class CheckValidatorTestData : TheoryData<Check, ExpectedResult>
        {
            public CheckValidatorTestData()
            {
                Add(new Check { DepartmentCode = "test" }, new ExpectedResult(nameof(Check.DepartmentCode), false));
                Add(new Check { DepartmentCode = "qwertyuip" }, new ExpectedResult(nameof(Check.DepartmentCode), true));
                Add(new Check { DepartmentCode = null }, new ExpectedResult(nameof(Check.DepartmentCode), true));

                Add(new Check { CheckCode = "test" }, new ExpectedResult(nameof(Check.CheckCode), false));
                Add(new Check { CheckCode = "qwerty" }, new ExpectedResult(nameof(Check.CheckCode), true));
                Add(new Check { CheckCode = null }, new ExpectedResult(nameof(Check.CheckCode), true));
            }
        }
    }
}