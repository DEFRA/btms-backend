using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Btms.Validation;
using FluentAssertions;
using FluentValidation.TestHelper;
using LinkStatus = Btms.Model.Cds.LinkStatus;

namespace Btms.Model.Validation.Tests
{
    public class CdsClearanceRequestValidatorTests
    {
        private readonly CdsClearanceRequestValidator validator = new();
        
        [Fact]
        public void Should_have_error_when_already_cancelled()
        {
            var model = new BtmsValidationPair<CdsClearanceRequest, Movement>(
                new CdsClearanceRequest()
                {
                    Header = new Header()
                    {
                        EntryVersionNumber = 1,
                        EntryReference = "123"
                    }
                },
                new Movement()
                {
                    EntryVersionNumber = 1,
                    Status = new MovementStatus()
                    {
                        ChedTypes = [ImportNotificationTypeEnum.Ced], LinkStatus = LinkStatus.AllLinked
                    },
                    Finalisation = new Finalisation()
                    {
                        FinalState = FinalState.CancelledAfterArrival,
                        ManualAction = true,
                    }
                });
            var result = validator.TestValidate(model);

            result.Errors.Any(x => x.CustomState == "ALVSVAL324").Should().BeTrue();
        }

        [Fact]
        public void Should_not_have_error_when_not_already_cancelled()
        {
            var model = new BtmsValidationPair<CdsClearanceRequest, Movement>(
                new CdsClearanceRequest()
                {
                    Header = new Header()
                    {
                        EntryVersionNumber = 1,
                        EntryReference = "123"
                    }
                },
                new Movement()
                {
                    EntryVersionNumber = 1,
                    Status = new MovementStatus()
                    {
                        ChedTypes = [ImportNotificationTypeEnum.Ced],
                        LinkStatus = LinkStatus.AllLinked
                    },
                    Finalisation = new Finalisation()
                    {
                        FinalState = FinalState.Cleared,
                        ManualAction = true,
                    }
                });
            var result = validator.TestValidate(model);

            result.Errors.Any(x => x.CustomState == "ALVSVAL324").Should().BeFalse();
        }

        [Fact]
        public void Should_have_error_when_duplicate_version()
        {
            var model = new BtmsValidationPair<CdsClearanceRequest, Movement>(
                new CdsClearanceRequest()
                {
                    Header = new Header()
                    {
                        EntryVersionNumber = 1,
                        EntryReference = "123"
                    }
                },
                new Movement()
                {
                    EntryVersionNumber = 1,
                    Status = new MovementStatus()
                    {
                        ChedTypes = [ImportNotificationTypeEnum.Ced],
                        LinkStatus = LinkStatus.AllLinked
                    },
                    Finalisation = new Finalisation()
                    {
                        FinalState = FinalState.CancelledAfterArrival,
                        ManualAction = true,
                    }
                });
            var result = validator.TestValidate(model);

            result.Errors.Any(x => x.CustomState == "ALVSVAL303").Should().BeTrue();
        }

        [Fact]
        public void Should_not_have_error_when_not_duplicate_version()
        {
            var model = new BtmsValidationPair<CdsClearanceRequest, Movement>(
                new CdsClearanceRequest()
                {
                    Header = new Header()
                    {
                        EntryVersionNumber = 2,
                        EntryReference = "123"
                    }
                },
                new Movement()
                {
                    EntryVersionNumber = 1,
                    Status = new MovementStatus()
                    {
                        ChedTypes = [ImportNotificationTypeEnum.Ced],
                        LinkStatus = LinkStatus.AllLinked
                    },
                    Finalisation = new Finalisation()
                    {
                        FinalState = FinalState.CancelledAfterArrival,
                        ManualAction = true,
                    }
                });
            var result = validator.TestValidate(model);

            result.Errors.Any(x => x.CustomState == "ALVSVAL303").Should().BeFalse();
        }

    }
}