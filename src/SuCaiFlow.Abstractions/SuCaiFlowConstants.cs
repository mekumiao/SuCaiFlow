namespace SuCaiFlow.Abstractions;

public static class SuCaiFlowConstants {
    public static class TaskStatuses {
        public const string Pending = "Pending";
        public const string Running = "Running";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Canceled = "Canceled";
    }

    public static class DownloadStatuses {
        public const string Pending = "Pending";
        public const string Running = "Running";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }
}
