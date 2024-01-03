using Kentico.Xperience.Algolia.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Services;

public interface IConfigurationStorageService
{
    Task<bool> TryEditIndex(AlgoliaConfigurationModel configuration);
    Task<bool> TryCreateIndex(AlgoliaConfigurationModel configuration);
    Task<bool> TryDeleteIndex(AlgoliaConfigurationModel configuration);
    Task<bool> TryDeleteIndex(int id);
    Task<AlgoliaConfigurationModel?> GetIndexDataOrNull(int indexId);
    Task<List<string>> GetExistingIndexNames();
    Task<List<int>> GetIndexIds();
    Task<IEnumerable<AlgoliaConfigurationModel>> GetAllIndexData();
}
