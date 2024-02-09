﻿namespace Kentico.Xperience.Algolia.Admin;

public class AlgoliaIndexStatisticsViewModel
{
    //
    // Summary:
    //     Index name.
    public string? Name { get; set; }

    //
    // Summary:
    //     Date of last update.
    public DateTime UpdatedAt { get; set; }

    //
    // Summary:
    //     Number of records contained in the index
    public long Entries { get; set; }

}
