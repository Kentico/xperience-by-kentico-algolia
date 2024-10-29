namespace Kentico.Xperience.Algolia.Indexing;

/// <summary>
/// Algolia integration options.
/// </summary>
public sealed class AlgoliaOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string CMS_ALGOLIA_SECTION_NAME = "CMSAlgoliaSearch";

    /// <summary>
    /// /// Turn off functionality if application is not configured in the appsettings
    /// </summary>
    public bool IsConfigured
    {
        get;
        set;
    } = false;

    /// <summary>
    /// Algolia application ID.
    /// </summary>
    public string ApplicationId
    {
        get;
        set;
    } = "NO_APP";


    /// <summary>
    /// Algolia API key.
    /// </summary>
    public string ApiKey
    {
        get;
        set;
    } = "NO_KEY";


    /// <summary>
    /// Public API key used for searching only.
    /// </summary>
    public string SearchKey
    {
        get;
        set;
    } = string.Empty;


    /// <summary>
    /// The Algolia crawler API key.
    /// </summary>
    public string CrawlerApiKey
    {
        get;
        set;
    } = string.Empty;


    /// <summary>
    /// The Algolia crawler user ID.
    /// </summary>
    public string CrawlerUserId
    {
        get;
        set;
    } = string.Empty;


    /// <summary>
    /// The interval at which <see cref="AlgoliaQueueWorker"/> runs, in milliseconds.
    /// </summary>
    public int CrawlerInterval
    {
        get;
        set;
    }

    public string ObjectIdParameterName
    {
        get;
        set;
    } = "object";

    public string QueryIdParameterName
    {
        get;
        set;
    } = "query";

    public string PositionParameterName
    {
        get;
        set;
    } = "pos";
}
