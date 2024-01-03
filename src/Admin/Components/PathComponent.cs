﻿using CMS.DataEngine;
using Kentico.Xperience.Admin.Base;
using Kentico.Xperience.Admin.Base.FormAnnotations;
using Kentico.Xperience.Admin.Base.Forms;
using Kentico.Xperience.Algolia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kentico.Xperience.Algolia.Admin.Components;

public class PathComponentProperties : FormComponentProperties
{
}

public class PathComponentClientProperties : FormComponentClientProperties<List<IncludedPath>>
{
    public IEnumerable<IncludedPath>? Value { get; set; }
    public List<string>? PossibleItems { get; set; }
}

public sealed class PathComponentAttribute : FormComponentAttribute
{
}


[ComponentAttribute(typeof(PathComponentAttribute))]
public class PathComponent : FormComponent<PathComponentProperties, PathComponentClientProperties, List<IncludedPath>>
{
    public List<IncludedPath>? Value { get; set; }

    public override string ClientComponentName => "@kentico/xperience-integrations-algolia/Path";

    public override List<IncludedPath> GetValue() => Value ?? [];
    public override void SetValue(List<IncludedPath> value) => Value = value;

    [FormComponentCommand]
    public async Task<ICommandResponse<RowActionResult>> DeletePath(string path)
    {
        var toRemove = Value?.FirstOrDefault(x => x.AliasPath == path);
        if (toRemove != null)
        {
            Value?.Remove(toRemove);
            return ResponseFrom(new RowActionResult(false));
        }
        return ResponseFrom(new RowActionResult(false));
    }

    [FormComponentCommand]
    public async Task<ICommandResponse<RowActionResult>> SavePath(IncludedPath path)
    {
        var value = Value?.SingleOrDefault(x => x.AliasPath == path.AliasPath);

        if (value is not null)
        {
            Value?.Remove(value);
        }

        Value?.Add(path);

        return ResponseFrom(new RowActionResult(false));
    }

    [FormComponentCommand]
    public async Task<ICommandResponse<RowActionResult>> AddPath(string path)
    {
        if (Value?.Any(x => x.AliasPath == path) ?? false)
        {
            return ResponseFrom(new RowActionResult(false));
        }
        else
        {
            Value?.Add(new IncludedPath(path));
            return ResponseFrom(new RowActionResult(false));
        }
    }

    protected override Task ConfigureClientProperties(PathComponentClientProperties properties)
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
