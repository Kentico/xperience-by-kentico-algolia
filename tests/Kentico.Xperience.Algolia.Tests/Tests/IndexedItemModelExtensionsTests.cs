using CMS.Core;
using CMS.Tests;

using DancingGoat.Models;

using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Indexing;
using Kentico.Xperience.Algolia.Tests.Base;
namespace Kentico.Xperience.Algolia.Tests.Tests;

internal class IndexedItemModelExtensionsTests : UnitTests
{
    [Test]
    public void IsIndexedByIndex()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        Assert.That(model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WildCard()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();
        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AlgoliaIndexIncludedPath("/%") { ContentTypes = [new(ArticlePage.CONTENT_TYPE_NAME, nameof(ArticlePage))] };

        index.IncludedPaths = new List<AlgoliaIndexIncludedPath>() { path };

        AlgoliaIndexStore.Instance.AddIndex(index);

        Assert.That(model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongWildCard()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AlgoliaIndexIncludedPath("/Index/%") { ContentTypes = [new("contentType", "contentType")] };

        index.IncludedPaths = new List<AlgoliaIndexIncludedPath>() { path };

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongPath()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.WebPageItemTreePath = "/Home";

        var index = MockDataProvider.Index;
        var path = new AlgoliaIndexIncludedPath("/Index") { ContentTypes = [new("contentType", "contentType")] };

        index.IncludedPaths = new List<AlgoliaIndexIncludedPath>() { path };

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongContentType()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.ContentTypeName = "DancingGoat.HomePage";

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }

    [Test]
    public void WrongIndex()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        Assert.That(!model.IsIndexedByIndex(log, "NewIndex", MockDataProvider.EventName));
    }

    [Test]
    public void WrongLanguage()
    {
        Service.InitializeContainer();
        var log = Substitute.For<IEventLogService>();

        var fixture = new Fixture();
        var item = fixture.Create<IndexEventWebPageItemModel>();

        var model = MockDataProvider.WebModel(item);
        model.LanguageName = "sk";

        AlgoliaIndexStore.Instance.SetIndicies(new List<AlgoliaConfigurationModel>());
        AlgoliaIndexStore.Instance.AddIndex(MockDataProvider.Index);

        Assert.That(!model.IsIndexedByIndex(log, MockDataProvider.DefaultIndex, MockDataProvider.EventName));
    }
}
