﻿using CMS.ContentEngine;
using CMS.DataEngine;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Admin.Providers;

public class ChannelOptionsProvider : IDropDownOptionsProvider
{
    private readonly IInfoProvider<ChannelInfo> channelInfoProvider;

    public ChannelOptionsProvider(IInfoProvider<ChannelInfo> channelInfoProvider) => this.channelInfoProvider = channelInfoProvider;

    public async Task<IEnumerable<DropDownOptionItem>> GetOptionItems() =>
        (await channelInfoProvider.Get()
            .WhereEquals(nameof(ChannelInfo.ChannelType), nameof(ChannelType.Website))
            .GetEnumerableTypedResultAsync())
            .Select(x => new DropDownOptionItem()
            {
                Value = x.ChannelName,
                Text = x.ChannelDisplayName
            });
}
