using FluentValidation.TestHelper;
using System;

namespace Btms.Types.Alvs.Validation.Tests
{
    public class UnitCheckValidatorTests
    {
        private CheckValidator validator = new();

        [Fact]
        public void Should_have_error_when_CheckCode_is_null()
        {
            var model = new Check { CheckCode = null };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(p => p.CheckCode);
        }

        [Fact]
        public void Should_not_have_error_when_CheckCode_is_specified()
        {
            var model = new Check { CheckCode = "test" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(p => p.CheckCode);
        }

        [Fact]
        public void Should_have_error_when_DepartmentCode_is_null()
        {
            var model = new Check { DepartmentCode = null };
            var result = validator.TestValidate(model);
            result.ShouldHaveValidationErrorFor(p => p.DepartmentCode);
        }

        [Fact]
        public void Should_not_have_error_when_DepartmentCode_is_specified()
        {
            var model = new Check { DepartmentCode = "test" };
            var result = validator.TestValidate(model);
            result.ShouldNotHaveValidationErrorFor(p => p.DepartmentCode);
        }
    }
}