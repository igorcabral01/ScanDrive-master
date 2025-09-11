using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ScanDrive.Infrastructure.Context;
using ScanDrive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ScanDrive.Api.Logging
{
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseLoggerProvider(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new ScopedDatabaseLogger(categoryName, _scopeFactory);
        }

        public void Dispose() { }

        private class ScopedDatabaseLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly IServiceScopeFactory _scopeFactory;

            public ScopedDatabaseLogger(string categoryName, IServiceScopeFactory scopeFactory)
            {
                _categoryName = categoryName;
                _scopeFactory = scopeFactory;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel)
            {
                // Ignora apenas logs de nível Information
                return logLevel != LogLevel.Information;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (!IsEnabled(logLevel))
                    return;

                var scope = _scopeFactory.CreateScope();
                try
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var httpContext = scope.ServiceProvider.GetService<IHttpContextAccessor>()?.HttpContext;

                    var log = new Log
                    {
                        Level = logLevel.ToString(),
                        Message = formatter(state, exception),
                        Exception = exception?.ToString(),
                        StackTrace = exception?.StackTrace,
                        Source = _categoryName,
                        Timestamp = DateTime.UtcNow
                    };

                    if (httpContext != null)
                    {
                        log.RequestPath = httpContext.Request.Path;
                        log.RequestMethod = httpContext.Request.Method;
                        log.RequestIp = httpContext.Connection.RemoteIpAddress?.ToString();
                        log.RequestUserAgent = httpContext.Request.Headers["User-Agent"].ToString();
                        log.ResponseStatusCode = httpContext.Response.StatusCode;
                    }

                    if (httpContext?.User?.Identity?.IsAuthenticated == true)
                    {
                        log.UserId = httpContext.User.FindFirst("sub")?.Value;
                        log.UserName = httpContext.User.Identity.Name;
                    }

                    Task.Run(async () =>
                    {
                        try
                        {
                            context.Logs.Add(log);
                            await context.SaveChangesAsync();
                        }
                        catch (Exception ex)
                        {
                            // Silenciosamente ignora erros de logging para evitar loops infinitos
                            Console.WriteLine($"Erro ao salvar log: {ex.Message}");
                        }
                        finally
                        {
                            scope.Dispose();
                        }
                    });
                }
                catch
                {
                    scope.Dispose();
                    throw;
                }
            }
        }
    }
} 