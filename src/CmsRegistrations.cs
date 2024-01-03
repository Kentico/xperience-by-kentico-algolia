using System.Runtime.CompilerServices;

using CMS;
using CMS.Core;

using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Admin.Base.UIPages;
using Kentico.Xperience.Algolia;
using Kentico.Xperience.Algolia.Admin;
using Kentico.Xperience.Algolia.Admin.Components;
using Kentico.Xperience.Algolia.Admin.UIPages;
using Kentico.Xperience.Algolia.Services;
using Kentico.Xperience.Algolia.Services.Implementations;

// Allows the Algolia test project to read internal members
[assembly: InternalsVisibleTo("Kentico.Xperience.Algolia.Tests")]

// Modules
[assembly: RegisterModule(typeof(AlgoliaSearchModule))]
[assembly: RegisterModule(typeof(AlgoliaAdminModule))]

// UI applications
[assembly: UIApplication(AlgoliaApplication.IDENTIFIER, typeof(AlgoliaApplication), "algolia", "Search", BaseApplicationCategories.DEVELOPMENT, Icons.Magnifier, TemplateNames.SECTION_LAYOUT)]

// Admin UI pages
[assembly: UIPage(typeof(AlgoliaApplication), "Indexes", typeof(IndexListing), "List of registered Algolia indices", TemplateNames.LISTING, UIPageOrder.First)]
[assembly: UIPage(typeof(IndexListing), PageParameterConstants.PARAMETERIZED_SLUG, typeof(EditIndex), "Edit Index", TemplateNames.EDIT, UIPageOrder.First)]

[assembly: RegisterFormComponent(nameof(PathComponent), typeof(PathComponent), "Algolia Path component")]

// Default service implementations
[assembly: RegisterImplementation(typeof(IAlgoliaClient), typeof(DefaultAlgoliaClient), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IAlgoliaIndexService), typeof(DefaultAlgoliaIndexService), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IAlgoliaInsightsService), typeof(DefaultAlgoliaInsightsService), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IAlgoliaTaskLogger), typeof(DefaultAlgoliaTaskLogger), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IConfigurationStorageService), typeof(DefaultAlgoliaConfigurationStorageService), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
[assembly: RegisterImplementation(typeof(IAlgoliaTaskProcessor), typeof(DefaultAlgoliaTaskProcessor), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
