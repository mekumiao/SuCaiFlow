namespace SuCaiFlow.Abstractions;

public class SuCaiFlowAssetDescriptor {
    public string? TaskId { get; set; }

    public string? Name { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public Uri? OriginalUri { get; set; }

    public string? StorageName { get; set; }

    public string? ContentType { get; set; }

    public long Size { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? DownloadedAt { get; set; }

    public string? Status { get; set; }
}
