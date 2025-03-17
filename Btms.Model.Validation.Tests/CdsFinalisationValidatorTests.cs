using Btms.Model.Cds;
using Btms.Model.Ipaffs;
using Btms.Validation;
using FluentAssertions;
using FluentValidation.TestHelper;
using LinkStatus = Btms.Model.Cds.LinkStatus;

namespace Btms.Model.Validation.Tests
{
    public class CdsFinalisationValidatorTests
    {
        private readonly CdsFinalisationValidator validator = new();
        
        [Fact]
        public void Should_have_error_when_already_cancelled_is_null()
        {
            var model = new BtmsValidationPair<CdsFinalisation, Movement>(
                new CdsFinalisation()
                {
                    Header = new FinalisationHeader()
                    {
                        EntryVersionNumber = 1,
                        EntryReference = "123",
                        FinalState = FinalState.CancelledAfterArrival,
                        ManualAction = true
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

            result.Errors.Any(x => x.CustomState.Equals("ALVSVAL501")).Should().BeTrue();
        }

        [Fact]
        public void Should_not_have_error_when_not_already_cancelled_is_null()
        {
            var model = new BtmsValidationPair<CdsFinalisation, Movement>(
                new CdsFinalisation()
                {
                    Header = new FinalisationHeader()
                    {
                        EntryVersionNumber = 1,
                        EntryReference = "123",
                        FinalState = FinalState.CancelledAfterArrival,
                        ManualAction = true
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

            result.Errors.Any(x => x.CustomState.Equals("ALVSVAL501")).Should().BeFalse();
        }

        [Fact]
        public void Should_have_error_when_entry_version_do_not_match()
        {
            var model = new BtmsValidationPair<CdsFinalisation, Movement>(
                new CdsFinalisation()
                {
                    Header = new FinalisationHeader()
                    {
                        EntryVersionNumber = 2,
                        EntryReference = "123",
                        FinalState = FinalState.CancelledAfterArrival,
                        ManualAction = true
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

            result.Errors.Any(x => x.CustomState.Equals("ALVSVAL401")).Should().BeTrue();
        }

        [Fact]
        public void Should_not_have_error_when_entry_version_do_match_and_is_not_cancel()
        {
            var model = new BtmsValidationPair<CdsFinalisation, Movement>(
                new CdsFinalisation()
                {
                    Header = new FinalisationHeader()
                    {
                        EntryVersionNumber = 1,
                        EntryReference = "123",
                        FinalState = FinalState.Cleared,
                        ManualAction = true
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

            result.Errors.Any(x => x.CustomState.Equals("ALVSVAL401")).Should().BeFalse();
        }

        [Fact]
        public void Should_have_error_when_already_cancelled()
        {
            var model = new BtmsValidationPair<CdsFinalisation, Movement>(
                new CdsFinalisation()
                {
                    Header = new FinalisationHeader()
                    {
                        EntryVersionNumber = 2,
                        EntryReference = "123",
                        FinalState = FinalState.CancelledAfterArrival,
                        ManualAction = true
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

            result.Errors.Any(x => x.CustomState.Equals("ALVSVAL403")).Should().BeTrue();
        }

        [Fact]
        public void Should_have_error_when_not_already_cancelled()
        {
            var model = new BtmsValidationPair<CdsFinalisation, Movement>(
                new CdsFinalisation()
                {
                    Header = new FinalisationHeader()
                    {
                        EntryVersionNumber = 2,
                        EntryReference = "123",
                        FinalState = FinalState.CancelledAfterArrival,
                        ManualAction = true
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

            result.Errors.Any(x => x.CustomState.Equals("ALVSVAL403")).Should().BeFalse();
        }
    }
}