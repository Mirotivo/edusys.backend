using System.Collections.Generic;

namespace Backend.Utilities
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }
        public string TraceId { get; set; }

        public ApiResponse(bool success, string message, T data = default!, Dictionary<string, List<string>> errors = null!, string traceId = default!)
        {
            Success = success;
            Message = message;
            Data = data;
            Errors = errors ?? new Dictionary<string, List<string>>();
            TraceId = traceId;
        }
    }
}
