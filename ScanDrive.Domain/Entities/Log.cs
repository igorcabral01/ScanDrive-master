using System;

namespace ScanDrive.Domain.Entities
{
    public class Log
    {
        public int Id { get; set; }
        public string Level { get; set; } = null!; // Info, Warning, Error, etc
        public string Message { get; set; } = null!;
        public string? Exception { get; set; }
        public string? StackTrace { get; set; }
        public string Source { get; set; } = null!;
        public DateTime Timestamp { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public string? RequestIp { get; set; }
        public string? RequestUserAgent { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public int? ResponseStatusCode { get; set; }
        public double? ExecutionTime { get; set; }
    }
} 