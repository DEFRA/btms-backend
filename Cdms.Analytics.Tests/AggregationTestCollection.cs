using Xunit;

namespace Cdms.Analytics.Tests;

[CollectionDefinition("Aggregation Test collection")]
public class AggregationTestCollection : ICollectionFixture<AggregationTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}