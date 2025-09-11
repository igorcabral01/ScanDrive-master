using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ScanDrive.Api.Logging
{
    public static class LoggingExtensions
    {
        public static ILoggingBuilder AddDatabaseLogging(this ILoggingBuilder builder)
        {
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            builder.Services.AddSingleton<ILoggerProvider, DatabaseLoggerProvider>(sp =>
            {
                var scopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                return new DatabaseLoggerProvider(scopeFactory);
            });
            return builder;
        }
    }
}