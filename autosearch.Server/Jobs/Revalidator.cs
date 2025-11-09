using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
namespace autosearch.Services;

public class Revalidator : IHostedService, IDisposable
{
    private readonly ILogger<Revalidator> _logger;
    private readonly ICacheService _cache;
    private Timer? _timer;

    private readonly TimeSpan _schedule = TimeSpan.FromMinutes(1);

    public Revalidator(
        ILogger<Revalidator> logger,
        ICacheService cache
    )
    {
        _logger = logger;
        _cache = cache;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Revalidator is starting...");

        // Run DoWork immediately, then every hour (adjust interval as needed)
        _timer = new Timer(async _ => await DoWork(null), null, TimeSpan.Zero, _schedule);

        return Task.CompletedTask;
    }

    private async Task DoWork(object? state)
    {
        try
        {
            _logger.LogInformation("Revalidator started at: {time}", DateTimeOffset.Now);

            var services = new ICarService[]
            {
                new HVWService(_cache),
                new TradeMeService(_cache),
                new TradeInClearanceCarsService(_cache)
            };

            var tasks = services.Select(s => s.RevalidateAsync());
            await Task.WhenAll(tasks);

            _logger.LogInformation("Revalidator completed at: {time}", DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during revalidation.");
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Revalidator stopped");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}