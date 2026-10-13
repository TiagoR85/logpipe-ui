namespace LogPipe.Ui.Services;

public sealed record FacetBucket(string Value, long Count);
public sealed record SearchFacets(List<FacetBucket> Levels, List<FacetBucket> Services);
public sealed record SearchHit(string Id, DateTimeOffset Timestamp, string Level, string Service,
    string Message, string? TraceId, string? SpanId, Dictionary<string, string>? Attributes);
public sealed record SearchResponse(long Total, List<SearchHit> Hits, SearchFacets Facets);
public sealed record TraceResponse(string TraceId, List<SearchHit> Entries);
