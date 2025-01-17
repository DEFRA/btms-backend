using Btms.Business.Services.Decisions;
using Btms.Business.Services.Matching;
using Btms.Model;
using Btms.Model.Cds;
using FluentAssertions;
using Xunit;
using DecisionContext = Btms.Business.Services.Decisions.DecisionContext;

namespace Btms.Business.Tests.Services.Decisions;

public class DecisionMessageBuilderTests
{
    [Fact]
    public async Task Test()
    {
        var decisionResult = new DecisionResult();
        decisionResult.AddDecision("movement-1", 1, "", DecisionCode.C03, DecisionType.Ched, "reason-1");
        var decisionContext = new DecisionContext
        (
            [],
            [
                new Movement
                {
                    Id = "movement-1",
                    BtmsStatus = MovementStatus.Default(),
                    Items =
                    [
                        new Items
                        {
                            ItemNumber = 1,
                            Checks =
                            [
                                new Check
                                {
                                    CheckCode = "A111"
                                }
                            ]
                        }
                    ]
                }
            ],
            new MatchingResult()
        );

        var decisions = await DecisionMessageBuilder.Build(decisionContext, decisionResult);

        decisions.Should().HaveCount(1);
        decisions[0].Items.Should().HaveCount(1);
        decisions[0].Items?[0].Checks.Should().HaveCount(1);
        decisions[0].Items?[0].Checks?[0].CheckCode.Should().Be("A111");
        decisions[0].Items?[0].Checks?[0].DecisionCode.Should().Be("C03");
        decisions[0].Items?[0].Checks?[0].DecisionReasons.Should().HaveCount(1);
        decisions[0].Items?[0].Checks?[0].DecisionReasons?[0].Should().Be("reason-1");
    }
}