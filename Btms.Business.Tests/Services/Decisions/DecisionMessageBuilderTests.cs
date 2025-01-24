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
    public async Task When_building_items_and_checks_for_a_set_of_decision_codes_Then_should_Build_correctly()
    {
        var decisionResult = CreateDecisionResult();
        var decisionContext = CreateDecisionContext();

        var decisions = await DecisionMessageBuilder.Build(decisionContext, decisionResult);
        
        decisions.Should().HaveCount(2);
        decisions[0].Items.Should().HaveCount(2);
        decisions[0].Items?[0].Checks.Should().HaveCount(2);
        decisions[0].Items?[0].Checks?[0].CheckCode.Should().Be("H222");
        decisions[0].Items?[0].Checks?[0].DecisionCode.Should().Be("C03");
        decisions[0].Items?[0].Checks?[0].DecisionReasons.Should().HaveCount(1);
        decisions[0].Items?[0].Checks?[0].DecisionReasons?[0].Should().Be("reason-1");
        
        decisions[0].Items?[0].Checks?[1].CheckCode.Should().Be("H224");
        decisions[0].Items?[0].Checks?[1].DecisionCode.Should().Be("C05");
        decisions[0].Items?[0].Checks?[1].DecisionReasons.Should().HaveCount(1);
        decisions[0].Items?[0].Checks?[1].DecisionReasons?[0].Should().Be("reason-2");
        
        decisions[0].Items?[1].Checks.Should().HaveCount(1);
        decisions[0].Items?[1].Checks?[0].CheckCode.Should().Be("H111");
        decisions[0].Items?[1].Checks?[0].DecisionCode.Should().Be("H01");
        decisions[0].Items?[1].Checks?[0].DecisionReasons.Should().HaveCount(1);
        decisions[0].Items?[1].Checks?[0].DecisionReasons?[0].Should().Be("reason-3");
        
        decisions[1].Items.Should().HaveCount(2);
        decisions[1].Items?[0].Checks.Should().HaveCount(0);
        decisions[1].Items?[1].Checks.Should().HaveCount(0);
    }

    private static DecisionContext CreateDecisionContext()
    {
        return new DecisionContext
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
                                    CheckCode = "H222"
                                },
                                new Check
                                {
                                    CheckCode = "H224"
                                }                            ]
                        },
                        new Items
                        {
                            ItemNumber = 2,
                            Checks =
                            [
                                new Check
                                {
                                    CheckCode = "H111"
                                }
                            ]
                        }
                    ]
                },
                new Movement
                {
                    Id = "movement-2",
                    BtmsStatus = MovementStatus.Default(),
                    Items =
                    [
                        new Items
                        {
                            ItemNumber = 1,
                            Checks = []
                        },
                        new Items
                        {
                            ItemNumber = 2,
                            Checks = null
                        }
                    ]
                }
            ],
            new MatchingResult()
        );
    }

    private static DecisionResult CreateDecisionResult()
    {
        var decisionResult = new DecisionResult();
        decisionResult.AddDecision("movement-1", 1, "", "H222" ,DecisionCode.C03, "reason-1");
        decisionResult.AddDecision("movement-1", 1, "", "H224" ,DecisionCode.C05, "reason-2");
        decisionResult.AddDecision("movement-1", 2, "", "H111" ,DecisionCode.H01, "reason-3");
        decisionResult.AddDecision("movement-1", 2, "", "H111" ,DecisionCode.C06, "reason-4");
        decisionResult.AddDecision("movement-2", 1, "", null ,DecisionCode.X00);
        decisionResult.AddDecision("movement-2", 2, "", null ,DecisionCode.E97);
        return decisionResult;
    }
}