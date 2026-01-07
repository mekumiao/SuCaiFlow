namespace SuCaiFlow.Abstractions;

public static class SuCaiFlowExceptions {
    public sealed class ConcurrencyException(string? message, Exception? exception)
        : Exception(message, exception) {
        public ConcurrencyException(string? message)
            : this(message, exception: null) {
        }
    }
}
