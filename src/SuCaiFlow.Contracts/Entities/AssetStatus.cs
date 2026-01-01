namespace SuCaiFlow.Contracts.Entities
{
    public enum AssetStatus
    {
        Pending = 0,      // 待采集
        InProgress = 1,   // 采集中
        Completed = 2,    // 已完成
        Failed = 3        // 采集失败
    }
}
