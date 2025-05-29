# Rebuilding an index outside of the Xperience by Kentico admin UI.

Reindexing can be started outside of the administration UI. 

The `AlgoliaIndexItemRebuildHook` provides an option to store a key belonging to an index in the database. The key can be compared against a http request and act as an api key used to authorize a rebuild request. Therefore it is a useful tool for automation coming from the outside.

## Implement an endpoint which triggers the rebuild.

Define a custom endpoint which calls the `Rebuild` method of the `IAlgoliaClient`. Secure the endpoint with a key and compare the key against the value of the `AlgoliaIndexItemRebuildHook` of your indexx

```csharp
private readonly IAlgoliaClient algoliaClient;
private readonly IEventLogService eventLogService;
private readonly IInfoProvider<AlgoliaIndexItemInfo> algoliaIndexItemProvider;

// Controller for rebuilding.
public SearchRebuildController(IAlgoliaClient algoliaClient, IEventLogService eventLogService,
IInfoProvider<AlgoliaIndexItemInfo> algoliaIndexItemProvider)
{
    this.algoliaClient = algoliaClient;
    this.algoliaIndexItemProvider = algoliaIndexItemProvider;
    this.eventLogService = eventLogService;
}

// Rebuild method specifying endpoint which can be called from the outside instead of using the admin UI.
[HttpPost]
public async Task<IActionResult> Rebuild([FromBody] RebuildSearchIndexRequest request)
{
    try
    {
        // The rebuild request can be used for a rebuild of different indexes, so we check whether the name of the index is specified.
        if (string.IsNullOrWhiteSpace(request.IndexName))
        {
            return NotFound($"IndexName is required");
        }

        // Retrieve the desired index by its name.
        var index = AlgoliaIndexStore.Instance.GetIndex(indexName);

        if (index is null)
        {
            return NotFound($"Index not found: {request.IndexName}");
        }

        // Retrieve the Rebuild Hook of the index.
        string rebuildHook = algoliaIndexItemProvider
            .Get()
            .WhereEquals(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemIndexName), request.IndexName)
            .First()
            .AlgoliaIndexItemRebuildHook;

        // Check whether the hook and key match.
        if (request.Secret != rebuildHook)
        {
            return Unauthorized("Invalid Secret");
        }

        // Call a rebuild on the index.
        await algoliaClient.Rebuild(index.IndexName, null);

        return Ok("Index rebuild started");
    }
    catch (Exception ex)
    {
        // Log an exception to a log service in case of a problem.
        eventLogService.LogException(nameof(SearchRebuildController), nameof(Rebuild), ex, $"IndexName: {request.IndexName}");

        return Problem("Index rebuild failed");
    }
}
   
```