using Xunit;

namespace Btms.Analytics.Tests.Fixtures;

[CollectionDefinition(nameof(MultiItemDataTestCollection))]
public class MultiItemDataTestCollection : ICollectionFixture<MultiItemDataTestFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}