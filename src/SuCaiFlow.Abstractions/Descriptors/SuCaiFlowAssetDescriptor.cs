namespace SuCaiFlow.Abstractions;

public class SuCaiFlowAssetDescriptor {
    public string? TaskId { get; set; }

    public string? SiteIdentifier { get; set; }

    public int OrderNo { get; set; }

    public string? Name { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? OriginalUrl { get; set; }

    public string? LandingUrl { get; set; }

    public string? StorageName { get; set; }

    public string? ContentType { get; set; }

    public long Size { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? DownloadedAt { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    /// <remarks>
    /// <see cref="SuCaiFlowConstants.DownloadStatuses"/>
    /// </remarks>
    public string? Status { get; set; }
}
