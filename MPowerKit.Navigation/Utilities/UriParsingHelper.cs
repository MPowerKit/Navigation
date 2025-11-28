using System.Web;

namespace MPowerKit.Navigation.Utilities;

public static class UriParsingHelper
{
    private const string __onePageBack = "..";
    public const string _onePageBack = nameof(__onePageBack);

    public static Uri ReplaceDotsAndParseUri(string uri)
    {
        return Parse(uri.Replace(__onePageBack, _onePageBack));
    }

    public static Uri EnsureAbsolute(Uri uri)
    {
        if (uri.IsAbsoluteUri) return uri;

        string delimiter = "/";

        return new Uri($"app://MPowerKit.github{(uri.OriginalString.StartsWith('/') ? "" : delimiter)}{uri}", UriKind.Absolute);
    }

    public static Uri Parse(string uri)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uri, nameof(uri));

        if (uri.StartsWith('/'))
        {
            return new Uri("app://MPowerKit.github" + uri, UriKind.Absolute);
        }
        else
        {
            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }
    }

    public static List<string> GetUriSegments(ref Uri uri)
    {
        List<string> segmentStack = [];

        if (!uri.IsAbsoluteUri)
        {
            uri = EnsureAbsolute(uri);
        }

        var segs = uri.Segments.ToList();
        segs.RemoveAll(static s => s == "/");
        segs = [.. segs.Select(static s => Uri.UnescapeDataString(s).Replace("/", ""))];

        return segs;
    }

    public static (Queue<string> Segments, List<(string, Uri)> QueryParameters) ProcessUri(Uri uri)
    {
        var segments = GetUriSegments(ref uri);
        var query = HttpUtility.ParseQueryString(uri.Query);

        Stack<string> realPages = [];

        for (var i = 0; i < segments.Count; i++)
        {
            var segment = segments[i];

            if (segment == _onePageBack && realPages.Count > 0 && realPages.Peek() != _onePageBack)
            {
                realPages.Pop();
                continue;
            }

            realPages.Push(segment);
        }

        if (realPages.Count == 0)
        {
            throw new ArgumentException("Uri path is not balanced. It should contain at least one page", nameof(uri));
        }

        List<(string, Uri)> queryParams = [];
        for (int i = 0; i < query.Count; i++)
        {
            var values = query.GetValues(i);
            var key = query.GetKey(i);
            foreach (var value in values!)
            {
                queryParams.Add((key!, ReplaceDotsAndParseUri(Uri.UnescapeDataString(value!).Replace('|', '/'))));
            }
        }

        return (new(realPages.Reverse()), queryParams);
    }
}