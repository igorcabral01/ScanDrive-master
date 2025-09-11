using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScanDrive.Domain.Entities;
using ScanDrive.Infrastructure.Context;
using System.Diagnostics;
using System.Text.Json;

namespace ScanDrive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MonitoringController : BaseController
{
    private readonly ILogger<MonitoringController> _logger;

    public MonitoringController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context,
        ILogger<MonitoringController> logger)
        : base(userManager, roleManager, context)
    {
        _logger = logger;
    }

    /// <summary>
    /// Verifica a saúde da API
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = typeof(MonitoringController).Assembly.GetName().Version?.ToString()
        });
    }

    /// <summary>
    /// Retorna métricas do sistema
    /// </summary>
    [HttpGet("metrics")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMetrics()
    {
        try
        {
            var process = Process.GetCurrentProcess();
            var startTime = process.StartTime;
            var uptime = DateTime.Now - startTime;

            // Obtém informações do sistema
            var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
            var usedMemory = process.WorkingSet64;
            var memoryUsagePercent = (double)usedMemory / totalMemory * 100;

            // Obtém informações do disco
            var drive = new DriveInfo(Path.GetPathRoot(Environment.CurrentDirectory) ?? "C:\\");
            var totalDiskSpace = drive.TotalSize;
            var freeDiskSpace = drive.AvailableFreeSpace;
            var usedDiskSpace = totalDiskSpace - freeDiskSpace;
            var diskUsagePercent = (double)usedDiskSpace / totalDiskSpace * 100;

            // Obtém informações da CPU
            var cpuUsage = GetCpuUsage();

            return Ok(new
            {
                status = "success",
                timestamp = DateTime.UtcNow,
                system = new
                {
                    uptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m {uptime.Seconds}s",
                    memory = new
                    {
                        total = FormatBytes(totalMemory),
                        used = FormatBytes(usedMemory),
                        free = FormatBytes(totalMemory - usedMemory),
                        usagePercent = Math.Round(memoryUsagePercent, 2)
                    },
                    disk = new
                    {
                        total = FormatBytes(totalDiskSpace),
                        used = FormatBytes(usedDiskSpace),
                        free = FormatBytes(freeDiskSpace),
                        usagePercent = Math.Round(diskUsagePercent, 2)
                    },
                    cpu = new
                    {
                        usagePercent = Math.Round(cpuUsage, 2)
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter métricas do sistema");
            return StatusCode(500, new
            {
                status = "error",
                message = "Erro ao obter métricas do sistema",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Retorna status do banco de dados
    /// </summary>
    [HttpGet("db-status")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetDatabaseStatus()
    {
        try
        {
            // Obtém o tamanho das tabelas
            var tableSizes = await _context.Database.SqlQueryRaw<TableSize>(
                @"SELECT 
                    table_name as TableName,
                    ROUND((data_length + index_length) / 1024 / 1024, 2) as SizeInMB,
                    table_rows as RowCount
                FROM information_schema.tables 
                WHERE table_schema = DATABASE()
                ORDER BY (data_length + index_length) DESC"
            ).ToListAsync();

            var dbStatus = new
            {
                status = "success",
                timestamp = DateTime.UtcNow,
                database = new
                {
                    connection = _context.Database.GetConnectionString()?.Split(';').FirstOrDefault() ?? "unknown",
                    state = _context.Database.GetDbConnection().State.ToString(),
                    canConnect = await _context.Database.CanConnectAsync(),
                    pendingMigrations = (await _context.Database.GetPendingMigrationsAsync()).ToList(),
                    appliedMigrations = (await _context.Database.GetAppliedMigrationsAsync()).ToList(),
                    tables_size_in_mb_total = $"{tableSizes.Sum(t => t.SizeInMB)} MB",
                    tables_row_count = new
                    {
                        users = await _context.Users.CountAsync(),
                        shops = await _context.Shops.CountAsync(),
                        qrCodes = await _context.QRCodes.CountAsync(),
                        vehicles = await _context.Vehicles.CountAsync(),
                        testDrives = await _context.TestDrives.CountAsync(),
                        leads = await _context.Leads.CountAsync(),
                        chatSessions = await _context.ChatSessions.CountAsync(),
                        chatMessages = await _context.ChatMessages.CountAsync()
                    },
                    tables_size_in_mb = tableSizes.Select(t => new
                    {
                        tableName = t.TableName,
                        size = $"{t.SizeInMB} MB",
                        rowCount = t.RowCount
                    }).ToList()
                }
            };

            return Ok(dbStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar status do banco de dados");
            return StatusCode(500, new
            {
                status = "error",
                message = "Erro ao verificar status do banco de dados",
                details = ex.Message
            });
        }
    }

    private static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n2} {suffixes[counter]}";
    }

    private double GetCpuUsage()
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            Thread.Sleep(1000);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds * Environment.ProcessorCount;
            var cpuUsageTotal = cpuUsedMs / totalMsPassed * 100;

            return cpuUsageTotal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível obter informações da CPU");
            return 0;
        }
    }

    private class TableSize
    {
        public string TableName { get; set; } = string.Empty;
        public decimal SizeInMB { get; set; }
        public long RowCount { get; set; }
    }
} 