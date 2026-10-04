namespace Hospital_Management_System.Helpers
{
    /// <summary>
    /// Uniform response envelope. Kept intentionally thin — most
    /// controller actions return the DTO or list directly (matching what
    /// api.js's mockResolve() shapes look like), but this wraps error
    /// cases so the frontend's catch blocks can always read err.message
    /// the same way whether the failure came from mock mode or the real API.
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }

        public static ApiResponse<T> Ok(T data, string? message = null) =>
            new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string message) =>
            new() { Success = false, Message = message, Data = default };
    }

    /// <summary>Non-generic error shape returned by global exception handling
    /// and validation failures, matching what api.js's `request()` reads
    /// as `data.message` on a non-OK response.</summary>
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string[]>? Errors { get; set; }
    }
}