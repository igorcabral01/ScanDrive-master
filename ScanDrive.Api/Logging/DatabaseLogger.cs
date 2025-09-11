using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ScanDrive.Infrastructure.Context;
using ScanDrive.Domain.Entities;

namespace ScanDrive.Api.Logging
{
    public class DatabaseLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DatabaseLogger(string categoryName, AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _categoryName = categoryName;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            var log = new Log
            {
                Level = logLevel.ToString(),
                Message = formatter(state, exception),
                Exception = exception?.Message,
                StackTrace = exception?.StackTrace,
                Source = _categoryName,
                Timestamp = DateTime.UtcNow
            };

            if (_httpContextAccessor.HttpContext != null)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                log.RequestPath = httpContext.Request.Path;
                log.RequestMethod = httpContext.Request.Method;
                log.RequestIp = httpContext.Connection.RemoteIpAddress?.ToString();
                log.RequestUserAgent = httpContext.Request.Headers["User-Agent"].ToString();
                log.ResponseStatusCode = httpContext.Response.StatusCode;
                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    log.UserId = httpContext.User.FindFirst("sub")?.Value;
                    log.UserName = httpContext.User.Identity.Name;
                }
            }

            Task.Run(async () =>
            {
                try
                {
                    await _context.Logs.AddAsync(log);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao salvar log: {ex.Message}");
                }
            });
        }
    }
} 