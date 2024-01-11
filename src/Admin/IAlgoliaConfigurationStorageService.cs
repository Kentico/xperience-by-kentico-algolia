using System.Collections.Generic;

namespace Kentico.Xperience.Algolia.Admin;

public interface IAlgoliaConfigurationStorageService
{
    bool TryCreateIndex(AlgoliaConfigurationModel configuration);

    bool TryEditIndex(AlgoliaConfigurationModel configuration);
    bool TryDeleteIndex(AlgoliaConfigurationModel configuration);
    bool TryDeleteIndex(int id);
    AlgoliaConfigurationModel? GetIndexDataOrNull(int indexId);
    List<string> GetExistingIndexNames();
    List<int> GetIndexIds();
    IEnumerable<AlgoliaConfigurationModel> GetAllIndexData();
}
