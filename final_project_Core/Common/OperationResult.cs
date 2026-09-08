namespace final_project_Core.Common
{
    public enum OperationStatus
    {
        Success,
        NotFound,
        Conflict,
        ValidationError,
        Forbidden
    }

    public class OperationResult<T>
    {
        public OperationStatus Status { get; }
        public T? Data { get; }
        public string? Message { get; }

        private OperationResult(OperationStatus status, T? data, string? message)
        {
            Status = status;
            Data = data;
            Message = message;
        }

        public static OperationResult<T> Success(T data) =>
            new(OperationStatus.Success, data, null);

        public static OperationResult<T> NotFound(string message) =>
            new(OperationStatus.NotFound, default, message);

        public static OperationResult<T> Conflict(string message) =>
            new(OperationStatus.Conflict, default, message);

        public static OperationResult<T> ValidationError(string message) =>
            new(OperationStatus.ValidationError, default, message);

        public static OperationResult<T> Forbidden(string message) =>
            new(OperationStatus.Forbidden, default, message);
    }
}
