using System.Net.Sockets;
using System.Reflection.Metadata;
using IssueHunter.Configuration;
using IssueHunter.Data;
using IssueHunter.Dtos.GitHub;
using IssueHunter.Models;
using IssueHunter.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IssueHunter.Workers;

public class IssuePollingWorker : BackgroundService
{
    private readonly ILogger<IssuePollingWorker> _logger;
    private readonly IOptionsMonitor<IssuePollingOptions> _options;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public IssuePollingWorker(
        ILogger<IssuePollingWorker> logger, 
        IOptionsMonitor<IssuePollingOptions> options,
        IServiceScopeFactory serviceScopeFactory
        )
        
    {
        _logger = logger;
        _options = options;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = _options.CurrentValue;
        
        if (!settings.Enabled)
        {
            _logger.LogInformation("Issue polling is disabled via configuration.");
            await Task.Delay(Timeout.Infinite, stoppingToken);
            return;
        }

        var interval = TimeSpan.FromMinutes(settings.IntervalMinutes);
        if (!settings.RunOnStartup)
        {
            await Task.Delay(interval, stoppingToken);
        };
        
        _logger.LogInformation("Issue polling worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<IIssuePollingOrchestrator>();
                await orchestrator.PollDueSearchesAsync(stoppingToken);

            }
            catch (Exception ex)
            {
                _logger.LogInformation("Error occurred when polling GitHub: " + ex.Message);
            }

            settings = _options.CurrentValue;
            interval = TimeSpan.FromMinutes(settings.IntervalMinutes);
            
            await Task.Delay(interval, stoppingToken);
        }
    }
}
