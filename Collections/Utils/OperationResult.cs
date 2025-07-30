namespace Collections.Utils
{
    public class OperationResult
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }

        public static OperationResult Ok() => new() { Success = true };
        public static OperationResult Fail(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };
    }
}
