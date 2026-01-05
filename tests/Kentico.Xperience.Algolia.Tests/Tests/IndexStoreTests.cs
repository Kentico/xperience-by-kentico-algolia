using CMS.Tests;

using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;
using Kentico.Xperience.Algolia.Tests.Base;

namespace Kentico.Xperience.Algolia.Tests.Tests;
internal class IndexStoreTests : UnitTests
{

    [Test]
    public void AddAndGetIndex()
    {
        AlgoliaIndexStore.Instance.SetIndicies([]);

        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.GetIndex("TestIndex", 1));

        Assert.Multiple(() =>
        {
            Assert.That(AlgoliaIndexStore.Instance.GetIndex("TestIndex") is not null);
            Assert.That(AlgoliaIndexStore.Instance.GetIndex(MockDataProvider.DefaultIndex) is not null);
        });
    }

    [Test]
    public void AddIndex_AlreadyExists()
    {
        AlgoliaIndexStore.Instance.SetIndicies([]);
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);

        bool hasThrown = false;

        try
        {
            AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);
        }
        catch
        {
            hasThrown = true;
        }

        Assert.That(hasThrown);
    }

    [Test]
    public void SetIndicies()
    {
        var defaultIndex = new AlgoliaConfigurationModel { IndexName = "DefaultIndex", Id = 0 };
        var simpleIndex = new AlgoliaConfigurationModel { IndexName = "SimpleIndex", Id = 1 };

        AlgoliaIndexStore.Instance.SetIndicies([defaultIndex, simpleIndex]);

        Assert.Multiple(() =>
        {
            Assert.That(AlgoliaIndexStore.Instance.GetIndex(defaultIndex.IndexName) is not null);
            Assert.That(AlgoliaIndexStore.Instance.GetIndex(simpleIndex.IndexName) is not null);
        });
    }

    [TearDown]
    public void TearDown() => AlgoliaIndexStore.Instance.SetIndicies([]);
}
