using Kentico.Xperience.AlgoliaSearch.Models;

using System;
using System.Collections.Generic;

namespace Kentico.Xperience.AlgoliaSearch.Services
{
    /// <summary>
    /// Default implementation of <see cref="IAlgoliaIndexStore"/>.
    /// </summary>
    public class DefaultAlgoliaIndexStore : IAlgoliaIndexStore
    {
        private readonly Stack<AlgoliaIndex> indexes = new Stack<AlgoliaIndex>();


        public IAlgoliaIndexStore Add<TModel>(string indexName) where TModel : AlgoliaSearchModel
        {
            indexes.Push(new AlgoliaIndex
            {
                IndexName = indexName,
                Type = typeof(TModel)
            });

            return this;
        }


        public AlgoliaIndex Pop()
        {
            try
            {
                return indexes.Pop();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
