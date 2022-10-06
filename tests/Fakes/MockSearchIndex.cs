using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Algolia.Search.Clients;
using Algolia.Search.Http;
using Algolia.Search.Iterators;
using Algolia.Search.Models.Batch;
using Algolia.Search.Models.Common;
using Algolia.Search.Models.Rules;
using Algolia.Search.Models.Search;
using Algolia.Search.Models.Settings;
using Algolia.Search.Models.Synonyms;

namespace Kentico.Xperience.Algolia.Test
{
    internal class MockSearchIndex : ISearchIndex
    {
        public AlgoliaConfig Config => throw new NotImplementedException();


        public BatchResponse Batch<T>(IEnumerable<BatchOperation<T>> operations, RequestOptions requestOptions = null) where T : class
        {
            throw new NotImplementedException();
        }


        public BatchResponse Batch<T>(BatchRequest<T> request, RequestOptions requestOptions = null) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<BatchResponse> BatchAsync<T>(IEnumerable<BatchOperation<T>> operations, RequestOptions requestOptions = null, CancellationToken ct = default) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<BatchResponse> BatchAsync<T>(BatchRequest<T> request, RequestOptions requestOptions = null, CancellationToken ct = default) where T : class
        {
            throw new NotImplementedException();
        }


        public IndexIterator<T> Browse<T>(BrowseIndexQuery query) where T : class
        {
            throw new NotImplementedException();
        }


        public BrowseIndexResponse<T> BrowseFrom<T>(BrowseIndexQuery query, RequestOptions requestOptions = null) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<BrowseIndexResponse<T>> BrowseFromAsync<T>(BrowseIndexQuery query, RequestOptions requestOptions = null, CancellationToken ct = default) where T : class
        {
            throw new NotImplementedException();
        }


        public RulesIterator BrowseRules(RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public SynonymsIterator BrowseSynonyms(RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public DeleteResponse ClearObjects(RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<DeleteResponse> ClearObjectsAsync(RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public DeleteResponse ClearRules(RequestOptions requestOptions = null, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public Task<DeleteResponse> ClearRulesAsync(RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public ClearSynonymsResponse ClearSynonyms(RequestOptions requestOptions = null, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public Task<ClearSynonymsResponse> ClearSynonymsAsync(RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public DeleteResponse Delete(RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<DeleteResponse> DeleteAsync(RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public DeleteResponse DeleteBy(Query query, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<DeleteResponse> DeleteByAsync(Query query, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public DeleteResponse DeleteObject(string objectId, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<DeleteResponse> DeleteObjectAsync(string objectId, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public BatchIndexingResponse DeleteObjects(IEnumerable<string> objectIds, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<BatchIndexingResponse> DeleteObjectsAsync(IEnumerable<string> objectIds, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            return Task.FromResult(new BatchIndexingResponse
            {
                Responses = new List<BatchResponse>
                {
                    new BatchResponse
                    {
                        ObjectIDs = objectIds
                    }
                }
            });
        }


        public DeleteResponse DeleteRule(string objectId, RequestOptions requestOptions = null, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public Task<DeleteResponse> DeleteRuleAsync(string objectId, RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public DeleteResponse DeleteSynonym(string synonymObjectId, RequestOptions requestOptions = null, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public Task<DeleteResponse> DeleteSynonymAsync(string synonymObjectId, RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public bool Exists()
        {
            throw new NotImplementedException();
        }


        public Task<bool> ExistsAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public HitWithPosition<T> FindFirstObject<T>(Func<T, bool> match, Query query, bool doNotPaginate = false, RequestOptions requestOptions = null) where T : class
        {
            throw new NotImplementedException();
        }


        public HitWithPosition<T> FindObject<T>(Func<T, bool> match, Query query, bool paginate = true, RequestOptions requestOptions = null) where T : class
        {
            throw new NotImplementedException();
        }


        public T GetObject<T>(string objectId, RequestOptions requestOptions = null, IEnumerable<string> attributesToRetrieve = null) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<T> GetObjectAsync<T>(string objectId, RequestOptions requestOptions = null, CancellationToken ct = default, IEnumerable<string> attributesToRetrieve = null) where T : class
        {
            throw new NotImplementedException();
        }


        public IEnumerable<T> GetObjects<T>(IEnumerable<string> objectIDs, RequestOptions requestOptions = null, IEnumerable<string> attributesToRetrieve = null) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<IEnumerable<T>> GetObjectsAsync<T>(IEnumerable<string> objectIDs, RequestOptions requestOptions = null, CancellationToken ct = default, IEnumerable<string> attributesToRetrieve = null) where T : class
        {
            throw new NotImplementedException();
        }


        public Rule GetRule(string objectId, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<Rule> GetRuleAsync(string objectId, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public IndexSettings GetSettings(RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }

        public Task<IndexSettings> GetSettingsAsync(RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public Synonym GetSynonym(string synonymObjectId, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<Synonym> GetSynonymAsync(string synonymObjectId, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public TaskStatusResponse GetTask(long taskId, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<TaskStatusResponse> GetTaskAsync(long taskId, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public MoveIndexResponse MoveFrom(string sourceIndex, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<MoveIndexResponse> MoveFromAsync(string sourceIndex, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public UpdateObjectResponse PartialUpdateObject<T>(T data, RequestOptions requestOptions = null, bool createIfNotExists = false) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<UpdateObjectResponse> PartialUpdateObjectAsync<T>(T data, RequestOptions requestOptions = null, CancellationToken ct = default, bool createIfNotExists = false) where T : class
        {
            throw new NotImplementedException();
        }


        public BatchIndexingResponse PartialUpdateObjects<T>(IEnumerable<T> data, RequestOptions requestOptions = null, bool createIfNotExists = false) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<BatchIndexingResponse> PartialUpdateObjectsAsync<T>(IEnumerable<T> data, RequestOptions requestOptions = null, CancellationToken ct = default, bool createIfNotExists = false) where T : class
        {
            return Task.FromResult(new BatchIndexingResponse
            {
                Responses = new List<BatchResponse>
                {
                    new BatchResponse
                    {
                        ObjectIDs = new string[data.Count()]
                    }
                }
            });
        }

        public MultiResponse ReplaceAllObjects<T>(IEnumerable<T> data, RequestOptions requestOptions = null, bool safe = false) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<MultiResponse> ReplaceAllObjectsAsync<T>(IEnumerable<T> data, RequestOptions requestOptions = null, CancellationToken ct = default, bool safe = false) where T : class
        {
            throw new NotImplementedException();
        }


        public BatchResponse ReplaceAllRules(IEnumerable<Rule> rules, RequestOptions requestOptions = null, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public Task<BatchResponse> ReplaceAllRulesAsync(IEnumerable<Rule> rules, RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public SaveSynonymResponse ReplaceAllSynonyms(IEnumerable<Synonym> synonyms, RequestOptions requestOptions = null, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public Task<SaveSynonymResponse> ReplaceAllSynonymsAsync(IEnumerable<Synonym> synonyms, RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public BatchIndexingResponse SaveObject<T>(T data, RequestOptions requestOptions = null, bool autoGenerateObjectId = false) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<BatchIndexingResponse> SaveObjectAsync<T>(T data, RequestOptions requestOptions = null, CancellationToken ct = default, bool autoGenerateObjectId = false) where T : class
        {
            throw new NotImplementedException();
        }


        public BatchIndexingResponse SaveObjects<T>(IEnumerable<T> data, RequestOptions requestOptions = null, bool autoGenerateObjectId = false) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<BatchIndexingResponse> SaveObjectsAsync<T>(IEnumerable<T> data, RequestOptions requestOptions = null, CancellationToken ct = default, bool autoGenerateObjectId = false) where T : class
        {
            throw new NotImplementedException();
        }


        public SaveRuleResponse SaveRule(Rule rule, RequestOptions requestOptions = null, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public Task<SaveRuleResponse> SaveRuleAsync(Rule rule, RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public BatchResponse SaveRules(IEnumerable<Rule> rules, RequestOptions requestOptions = null, bool forwardToReplicas = false, bool clearExistingRules = false)
        {
            throw new NotImplementedException();
        }


        public Task<BatchResponse> SaveRulesAsync(IEnumerable<Rule> rules, RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false, bool clearExistingRules = false)
        {
            throw new NotImplementedException();
        }


        public SaveSynonymResponse SaveSynonym(Synonym synonym, RequestOptions requestOptions = null, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public Task<SaveSynonymResponse> SaveSynonymAsync(Synonym synonym, RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public SaveSynonymResponse SaveSynonyms(IEnumerable<Synonym> synonyms, RequestOptions requestOptions = null, bool forwardToReplicas = false, bool replaceExistingSynonyms = false)
        {
            throw new NotImplementedException();
        }


        public Task<SaveSynonymResponse> SaveSynonymsAsync(IEnumerable<Synonym> synonyms, RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false, bool replaceExistingSynonyms = false)
        {
            throw new NotImplementedException();
        }


        public SearchResponse<T> Search<T>(Query query, RequestOptions requestOptions = null) where T : class
        {
            throw new NotImplementedException();
        }


        public Task<SearchResponse<T>> SearchAsync<T>(Query query, RequestOptions requestOptions = null, CancellationToken ct = default) where T : class
        {
            throw new NotImplementedException();
        }


        public SearchForFacetResponse SearchForFacetValue(SearchForFacetRequest query, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<SearchForFacetResponse> SearchForFacetValueAsync(SearchForFacetRequest query, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public SearchResponse<Rule> SearchRule(RuleQuery query = null, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<SearchResponse<Rule>> SearchRuleAsync(RuleQuery query = null, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public SearchResponse<Synonym> SearchSynonyms(SynonymQuery query, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task<SearchResponse<Synonym>> SearchSynonymsAsync(SynonymQuery query, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }


        public SetSettingsResponse SetSettings(IndexSettings settings, RequestOptions requestOptions = null, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public Task<SetSettingsResponse> SetSettingsAsync(IndexSettings settings, RequestOptions requestOptions = null, CancellationToken ct = default, bool forwardToReplicas = false)
        {
            throw new NotImplementedException();
        }


        public void WaitTask(long taskId, int timeToWait = 100, RequestOptions requestOptions = null)
        {
            throw new NotImplementedException();
        }


        public Task WaitTaskAsync(long taskId, int timeToWait = 100, RequestOptions requestOptions = null, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
