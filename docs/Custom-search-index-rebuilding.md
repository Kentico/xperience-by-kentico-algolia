# Rebuilding an index outside of the Xperience by Kentico admin UI.

Reindexing can be started outside of the administration UI. 

The `AlgoliaIndexItemRebuildHook` provides an option to store a key belonging to an index in the database. The key can be compared against a http request and act as an api key used to authorize a rebuild request. Therefore it is a useful tool for automation comming from the outside.

## Implement an endpoint which triggers the rebuild.

Define a custom endpoint which calls the `Rebuild` method of the `IAlgoliaClient`. Secure the endpoint with a key and compare the key against the value of the `AlgoliaIndexItemRebuildHook` of your indexx

```csharp
private readonly IAlgoliaClient algoliaClient;
private readonly IEventLogService eventLogService;
private readonly IInfoProvider<AlgoliaIndexItemInfo> algoliaIndexItemProvider;

public SearchRebuildController(IAlgoliaClient algoliaClient, IEventLogService eventLogService,
IInfoProvider<AlgoliaIndexItemInfo> algoliaIndexItemProvider)
{
    this.algoliaClient = algoliaClient;
    this.algoliaIndexItemProvider = algoliaIndexItemProvider;
    this.eventLogService = eventLogService;
}

[HttpPost]
public async Task<IActionResult> Rebuild([FromBody] RebuildSearchIndexRequest request)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.IndexName))
        {
            return NotFound($"IndexName is required");
        }

        var index = AlgoliaIndexStore.Instance.GetIndex(indexName);

        if (index == null)
        {
            return NotFound($"Index not found: {request.IndexName}");
        }

        string rebuildHook = algoliaIndexItemProvider
            .Get()
            .WhereEquals(nameof(AlgoliaIndexItemInfo.AlgoliaIndexItemIndexName), request.IndexName)
            .First()
            .AlgoliaIndexItemRebuildHook;

        if (request.Secret != rebuildHook)
        {
            return Unauthorized("Invalid Secret");
        }

        await algoliaClient.Rebuild(index.IndexName, null);

        return Ok("Index rebuild started");
    }
    catch (Exception ex)
    {
        eventLogService.LogException(nameof(SearchRebuildController), nameof(Rebuild), ex, $"IndexName: {request.IndexName}");

        return Problem("Index rebuild failed");
    }
}
   
```