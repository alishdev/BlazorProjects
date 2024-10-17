

using System.Text.Json.Serialization;

namespace UpdateWPPost;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
public class GoogleSearchResponse
{
    [JsonPropertyName("kind")] public string Kind { get; set; }

    [JsonPropertyName("url")] public Url Url { get; set; }

    [JsonPropertyName("queries")] public Queries Queries { get; set; }

    [JsonPropertyName("context")] public Context Context { get; set; }

    [JsonPropertyName("searchInformation")]
    public SearchInformation SearchInformation { get; set; }

    [JsonPropertyName("items")] public List<Item> Items { get; set; }
}

public class Url
{
    [JsonPropertyName("type")] public string Type { get; set; }

    [JsonPropertyName("template")] public string Template { get; set; }
}

public class Queries
{
    [JsonPropertyName("request")] public List<Request> Request { get; set; }

    [JsonPropertyName("nextPage")] public List<NextPage> NextPage { get; set; }
}

public class Request
{
    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("totalResults")] public string TotalResults { get; set; }

    [JsonPropertyName("searchTerms")] public string SearchTerms { get; set; }

    [JsonPropertyName("count")] public int Count { get; set; }

    [JsonPropertyName("startIndex")] public int StartIndex { get; set; }

    [JsonPropertyName("inputEncoding")] public string InputEncoding { get; set; }

    [JsonPropertyName("outputEncoding")] public string OutputEncoding { get; set; }

    [JsonPropertyName("safe")] public string Safe { get; set; }

    [JsonPropertyName("cx")] public string Cx { get; set; }
}

public class NextPage
{
    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("totalResults")] public string TotalResults { get; set; }

    [JsonPropertyName("searchTerms")] public string SearchTerms { get; set; }

    [JsonPropertyName("count")] public int Count { get; set; }

    [JsonPropertyName("startIndex")] public int StartIndex { get; set; }

    [JsonPropertyName("inputEncoding")] public string InputEncoding { get; set; }

    [JsonPropertyName("outputEncoding")] public string OutputEncoding { get; set; }

    [JsonPropertyName("safe")] public string Safe { get; set; }

    [JsonPropertyName("cx")] public string Cx { get; set; }
}

public class Context
{
    [JsonPropertyName("title")] public string Title { get; set; }
}

public class SearchInformation
{
    [JsonPropertyName("searchTime")] public double SearchTime { get; set; }

    [JsonPropertyName("formattedSearchTime")]
    public string FormattedSearchTime { get; set; }

    [JsonPropertyName("totalResults")] public string TotalResults { get; set; }

    [JsonPropertyName("formattedTotalResults")]
    public string FormattedTotalResults { get; set; }
}

public class Item
{
    [JsonPropertyName("kind")] public string Kind { get; set; }

    [JsonPropertyName("title")] public string Title { get; set; }

    [JsonPropertyName("htmlTitle")] public string HtmlTitle { get; set; }

    [JsonPropertyName("link")] public string Link { get; set; }

    [JsonPropertyName("displayLink")] public string DisplayLink { get; set; }

    [JsonPropertyName("snippet")] public string Snippet { get; set; }

    [JsonPropertyName("htmlSnippet")] public string HtmlSnippet { get; set; }

    [JsonPropertyName("formattedUrl")] public string FormattedUrl { get; set; }

    [JsonPropertyName("htmlFormattedUrl")] public string HtmlFormattedUrl { get; set; }

    [JsonPropertyName("pagemap")] public PageMap PageMap { get; set; }
}

public class PageMap
{
    [JsonPropertyName("cse_thumbnail")] public List<CseThumbnail> CseThumbnail { get; set; }

    [JsonPropertyName("metatags")] public List<MetaTag> MetaTags { get; set; }

    [JsonPropertyName("cse_image")] public List<CseImage> CseImage { get; set; }
}

public class CseThumbnail
{
    [JsonPropertyName("src")] public string Src { get; set; }

    [JsonPropertyName("width")] public string Width { get; set; }

    [JsonPropertyName("height")] public string Height { get; set; }
}

public class MetaTag
{
    [JsonPropertyName("msapplication-tilecolor")]
    public string MsApplicationTileColor { get; set; }

    [JsonPropertyName("apple-itunes-app")] public string AppleItunesApp { get; set; }

    [JsonPropertyName("ogsitename")] public string OgSiteName { get; set; }

    [JsonPropertyName("ogimage")] public string OgImage { get; set; }

    [JsonPropertyName("theme-color")] public string ThemeColor { get; set; }

    [JsonPropertyName("viewport")] public string Viewport { get; set; }

    [JsonPropertyName("ogtype")] public string OgType { get; set; }

    [JsonPropertyName("ogtitle")] public string OgTitle { get; set; }

    [JsonPropertyName("ogdescription")] public string OgDescription { get; set; }

    [JsonPropertyName("ogurl")] public string OgUrl { get; set; }
}

public class CseImage
{
    [JsonPropertyName("src")] public string Src { get; set; }
}

#pragma warning restore CS8618