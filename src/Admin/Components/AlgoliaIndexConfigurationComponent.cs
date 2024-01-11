using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Xperience.Algolia.Admin;
using System;

[assembly: RegisterFormComponent(
    identifier: AlgoliaIndexConfigurationComponent.IDENTIFIER,
    componentType: typeof(AlgoliaIndexConfigurationComponent),
    name: "Algolia Search Index Configuration")]

namespace Kentico.Xperience.Algolia.Admin;

#pragma warning disable S2094 // intentionally empty class
public class AlgoliaIndexConfigurationComponentProperties : FormComponentProperties
{
}
#pragma warning restore

public class AlgoliaIndexConfigurationComponentClientProperties : FormComponentClientProperties<List<AlgoliaIndexIncludedPath>>
{
    public List<string>? PossibleItems { get; set; }
}

public sealed class AlgoliaIndexConfigurationComponentAttribute : FormComponentAttribute
{
}


[ComponentAttribute(typeof(AlgoliaIndexConfigurationComponentAttribute))]
public class AlgoliaIndexConfigurationComponent : FormComponent<AlgoliaIndexConfigurationComponentProperties, AlgoliaIndexConfigurationComponentClientProperties, List<AlgoliaIndexIncludedPath>>
{
    public const string IDENTIFIER = "kentico.xperience-integrations-algolia.algolia-index-configuration";

    public List<AlgoliaIndexIncludedPath>? Value { get; set; }

    public override string ClientComponentName => "@kentico/xperience-integrations-algolia/AlgoliaIndexConfiguration";

    public override List<AlgoliaIndexIncludedPath> GetValue() => Value ?? new();
    public override void SetValue(List<AlgoliaIndexIncludedPath> value) => Value = value;

    [FormComponentCommand]
    public Task<ICommandResponse<RowActionResult>> DeletePath(string path)
    {
        var toRemove = Value?.Find(x => Equals(x.AliasPath == path, StringComparison.OrdinalIgnoreCase));
        if (toRemove != null)
        {
            Value?.Remove(toRemove);
            return Task.FromResult(ResponseFrom(new RowActionResult(false)));
        }
        return Task.FromResult(ResponseFrom(new RowActionResult(false)));
    }

    [FormComponentCommand]
    public Task<ICommandResponse<RowActionResult>> SavePath(AlgoliaIndexIncludedPath path)
    {
        var value = Value?.SingleOrDefault(x => Equals(x.AliasPath == path.AliasPath, StringComparison.OrdinalIgnoreCase));

        if (value is not null)
        {
            Value?.Remove(value);
        }

        Value?.Add(path);

        return Task.FromResult(ResponseFrom(new RowActionResult(false)));
    }

    [FormComponentCommand]
    public Task<ICommandResponse<RowActionResult>> AddPath(string path)
    {
        if (Value?.Exists(x => x.AliasPath == path) ?? false)
        {
            return Task.FromResult(ResponseFrom(new RowActionResult(false)));
        }
        else
        {
            Value?.Add(new AlgoliaIndexIncludedPath(path));
            return Task.FromResult(ResponseFrom(new RowActionResult(false)));
        }
    }

    protected override Task ConfigureClientProperties(AlgoliaIndexConfigurationComponentClientProperties properties)
    {
        var allPageItems = DataClassInfoProvider
            .GetClasses()
            .WhereEquals(nameof(DataClassInfo.ClassContentTypeType), "Website")
            .ToList()
            .Select(x => x.ClassName)
            .ToList();

        properties.Value = Value;
        properties.PossibleItems = allPageItems;

        return base.ConfigureClientProperties(properties);
    }
}
