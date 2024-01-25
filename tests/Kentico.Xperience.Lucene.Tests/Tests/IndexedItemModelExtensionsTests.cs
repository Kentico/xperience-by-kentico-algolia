using CMS.Core;
using DancingGoat.Models;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;
using Kentico.Xperience.Algolia.Tests.Base;
namespace Kentico.Xperience.Algolia.Tests.Tests;

internal class MockEventLogService : IEventLogService
{
    public void LogEvent(EventLogData eventLogData) { }
}

internal class IndexedItemModelExtensionsTests
{
    private readonly IEventLogService log;

    public IndexedItemModelExtensionsTests() => log = new MockEventLogService();

    [Test]
    public void IsIndexedByIndex()
    {
        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(MockDataProvider.WebModel.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WildCard()
    {
        var model = MockDataProvider.WebModel;
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AlgoliaIndexIncludedPath("/%") { ContentTypes = [ArticlePage.CONTENT_TYPE_NAME] };

        index.IncludedPaths = new List<AlgoliaIndexIncludedPath>() { path };

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(index);

        Assert.That(model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongWildCard()
    {
        var model = MockDataProvider.WebModel;
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AlgoliaIndexIncludedPath("/Index/%") { ContentTypes = [ArticlePage.CONTENT_TYPE_NAME] };

        index.IncludedPaths = new List<AlgoliaIndexIncludedPath>() { path };

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongPath()
    {
        var model = MockDataProvider.WebModel;
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AlgoliaIndexIncludedPath("/Index") { ContentTypes = [ArticlePage.CONTENT_TYPE_NAME] };

        index.IncludedPaths = new List<AlgoliaIndexIncludedPath>() { path };

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongContentType()
    {
        var model = MockDataProvider.WebModel;
        model.ContentTypeName = "DancingGoat.HomePage";

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongIndex()
    {
        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!MockDataProvider.WebModel.IsIndexedByIndex(log, "NewIndex", MockDataProvider.EventName));
    }

    [Test]
    public void WrongLanguage()
    {
        var model = MockDataProvider.WebModel;
        model.LanguageName = "sk";

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }
}
