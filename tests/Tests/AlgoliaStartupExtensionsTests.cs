using System.Collections.Generic;
using System.Linq;

using Algolia.Search.Clients;

using Kentico.Xperience.Algolia.Extensions;
using Kentico.Xperience.Algolia.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using static Kentico.Xperience.Algolia.Tests.TestSearchModels;

namespace Kentico.Xperience.Algolia.Tests
{
    internal class AlgoliaStartupExtensionsTests
    {
        [TestFixture]
        internal class AddAlgoliaTests
        {
            private IServiceCollection services;
            private IConfiguration configuration;
            private const string APP_ID = "APP_ID";
            private const string API_KEY = "API_KEY";
            private const string CUSTOM_PARAM_NAME = "CUSTOM";


            [SetUp]
            public void AddAlgoliaTestsSetUp()
            {
                services = new ServiceCollection();
                configuration = new ConfigurationBuilder()
                    .AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>($"{AlgoliaOptions.SECTION_NAME}:applicationId", APP_ID),
                        new KeyValuePair<string, string>($"{AlgoliaOptions.SECTION_NAME}:apiKey", API_KEY),
                        new KeyValuePair<string, string>($"{AlgoliaOptions.SECTION_NAME}:objectIdParameterName", CUSTOM_PARAM_NAME)
                    })
                    .Build();
            }


            [Test]
            public void AddAlgolia_RegistersOptions()
            {
                services.AddAlgolia(configuration);
                var provider = services.BuildServiceProvider();
                var algoliaOptions = provider.GetRequiredService<IOptions<AlgoliaOptions>>();

                Assert.Multiple(() =>
                {
                    Assert.That(algoliaOptions, Is.Not.Null);
                    Assert.That(algoliaOptions.Value.ApplicationId, Is.EqualTo(APP_ID));
                    Assert.That(algoliaOptions.Value.ApiKey, Is.EqualTo(API_KEY));
                    Assert.That(algoliaOptions.Value.ObjectIdParameterName, Is.EqualTo(CUSTOM_PARAM_NAME));
                });
            }


            [Test]
            public void AddAlgolia_ValidIndexes_RegistersServices()
            {
                services.AddAlgolia(configuration);
                var provider = services.BuildServiceProvider();

                Assert.Multiple(() =>
                {
                    Assert.That(provider.GetRequiredService<ISearchClient>(), Is.InstanceOf<ISearchClient>());
                    Assert.That(provider.GetRequiredService<IInsightsClient>(), Is.InstanceOf<IInsightsClient>());
                });
            }


            [Test]
            public void AddAlgolia_ValidIndexes_RegistersIndex()
            {
                services.AddAlgolia(configuration, new AlgoliaIndex[] {
                    new AlgoliaIndex(typeof(ArticleEnSearchModel), nameof(ArticleEnSearchModel)),
                    new AlgoliaIndex(typeof(ProductsSearchModel), nameof(ProductsSearchModel))
                });

                Assert.That(IndexStore.Instance.GetAll().Count(), Is.EqualTo(2));
            }


            [TearDown]
            public void AddAlgoliaTearDown()
            {
                IndexStore.Instance.Clear();
            }
        }
    }
}
