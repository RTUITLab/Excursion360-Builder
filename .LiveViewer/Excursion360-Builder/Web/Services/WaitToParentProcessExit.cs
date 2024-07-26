using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Web.Models.Options;

namespace Web.Services;

public class WaitToParentProcessExit : BackgroundService
{
    private readonly IOptions<StartupOptions> options;
    private readonly IHostApplicationLifetime lifetime;
    private readonly ILogger<WaitToParentProcessExit> logger;

    public WaitToParentProcessExit(
        IOptions<StartupOptions> options,
        IHostApplicationLifetime lifetime,
        ILogger<WaitToParentProcessExit> logger)
    {
        this.options = options;
        this.lifetime = lifetime;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Process targetProcess;
        try
        {
            targetProcess = Process.GetProcessById(options.Value.ParentProcessId);
        }
        catch
        {
            logger.LogError($"not found process for id {options.Value.ParentProcessId}");
            return;
        }

        while (!stoppingToken.IsCancellationRequested && !targetProcess.HasExited)
        {
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);
        }
        if (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("parent process exited, closing app");
            lifetime.StopApplication();
        }
    }
}
