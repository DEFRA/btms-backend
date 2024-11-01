using Cdms.Types.Alvs;
using FluentAssertions;
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


            var consumer =
                new AlvsClearanceRequestConsumer(dbContext);
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

        private AlvsClearanceRequest CreateAlvsClearanceRequest()
        {
            return ClearanceRequestBuilder.Default()
                .WithValidDocumentReferenceNumbers().Build();
        }
    }
}