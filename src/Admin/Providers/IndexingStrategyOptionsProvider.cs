using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Algolia.Indexing;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Admin;

public class IndexingStrategyOptionsProvider : IDropDownOptionsProvider
{
    public Task<IEnumerable<DropDownOptionItem>> GetOptionItems() =>
    Task.FromResult(StrategyStorage.Strategies.Keys.Select(x => new DropDownOptionItem()
    {
        Value = x,
        Text = x
    }));
}
