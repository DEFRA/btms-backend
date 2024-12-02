using Cdms.Business.Services;
using Cdms.Types.Alvs;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SlimMessageBus.Host;
using TestDataGenerator;
using Xunit;

namespace Cdms.Consumers.Tests
{
    public class ClearanceRequestConsumerTests : ConsumerTests
    {
        [Fact]
        public async Task WhenNotificationNotExists_ThenShouldBeCreated()
        {
            // ARRANGE
            var clearanceRequest = CreateAlvsClearanceRequest();
            var dbContext = CreateDbContext();
            var mockLinkingService = Substitute.For<ILinkingService>();

            var consumer =
                new AlvsClearanceRequestConsumer(dbContext, mockLinkingService, NullLogger<AlvsClearanceRequestConsumer>.Instance);
            consumer.Context = new ConsumerContext()
            {
                Headers = new Dictionary<string, object>()
                {
                    { "messageId", clearanceRequest!.Header!.EntryReference! }
                }
            };

            // ACT
            await consumer.OnHandle(clearanceRequest);

            // ASSERT
            var savedMovement = await dbContext.Movements.Find(clearanceRequest!.Header!.EntryReference!);
            savedMovement.Should().NotBeNull();
            savedMovement.AuditEntries.Count.Should().Be(1);
            savedMovement.AuditEntries[0].Status.Should().Be("Created");
        }

        private static AlvsClearanceRequest CreateAlvsClearanceRequest()
        {
            return ClearanceRequestBuilder.Default()
                .WithValidDocumentReferenceNumbers().Build();
        }
    }
}