﻿using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Admin.Providers
{
    public class IndexingStrategyOptionsProvider : IDropDownOptionsProvider
    {
        public async Task<IEnumerable<DropDownOptionItem>> GetOptionItems() =>
          StrategyStorage.Strategies.Keys.Select(x => new DropDownOptionItem()
          {
              Value = x,
              Text = x
          });
    }
}
