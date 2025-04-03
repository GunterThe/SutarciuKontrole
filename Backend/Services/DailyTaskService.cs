using Microsoft.EntityFrameworkCore;

public class DailyTaskService : BackgroundService
{
    private readonly ILogger<DailyTaskService> _logger;
    private readonly AppDbContext _context;
    private readonly EmailService _emailService; 
    private readonly TimeSpan _targetTime = new TimeSpan(7, 0, 0);

    public DailyTaskService(ILogger<DailyTaskService> logger, AppDbContext context, EmailService emailService)
    {
        _logger = logger;
        _context = context;
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var timeToWait = _targetTime - now.TimeOfDay;

            if (timeToWait < TimeSpan.Zero)
            {
                timeToWait += TimeSpan.FromDays(1);
            }

            _logger.LogInformation($"Waiting for {timeToWait.TotalMinutes} minutes until the next execution.");
            await Task.Delay(timeToWait, stoppingToken);

            if (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Executing daily task...");
                await PerformTaskAsync();
            }
        }
    }

    private async Task PerformTaskAsync()
    {
        try
        {
            var today = DateTime.Today;
            var irasai = await _context.Irasas
                .Where(i => i.Kita_data.Date == today)
                .ToListAsync();

            foreach (var irasas in irasai)
            {
                if (irasas.Archyvuotas == false && irasas.Dienu_daznumas != 0)
                {
                    if (irasas.Kita_data.AddDays(irasas.Dienu_daznumas) <= irasas.Pabaigos_data)
                    {
                        var subject = $"Priminmas apie {irasas.Pavadinimas}";
                        var message = $"Sveiki,\n\nPrimename, kad {irasas.Pavadinimas} galioja iki {irasas.Pabaigos_data.ToShortDateString()}.\n\nGeros dienos!";
                        await _emailService.SendEmailAsync(irasas.Pastas_kreiptis, subject, message);
                        irasas.Kita_data = irasas.Kita_data.AddDays(irasas.Dienu_daznumas);
                    }
                    else
                    {
                        irasas.Kita_data = irasas.Pabaigos_data;
                    }
                }
                else if (irasas.Archyvuotas == false && irasas.Dienos_pries != 0)
                {
                    var subject = $"Priminmas apie {irasas.Pavadinimas}";
                    var message = $"Sveiki,\n\nPrimename, kad {irasas.Pavadinimas} galioja iki {irasas.Pabaigos_data.ToShortDateString()}.\n\nGeros dienos!";
                    irasas.Kita_data = irasas.Pabaigos_data;
                }
                else if (irasas.Kita_data == irasas.Pabaigos_data && irasas.Archyvuotas == false)
                {
                    var subject = $"Baigesi sutartis: {irasas.Pavadinimas}";
                    var message = $"Sveiki,\n\nPranešame, kad {irasas.Pavadinimas} pasibaigė galioti {irasas.Pabaigos_data.ToShortDateString()} šitą sutartį galite matyti savo archyve.\n\nGeros dienos!";
                    irasas.Archyvuotas = true;
                }
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Daily task executed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while executing the daily task: {ex.Message}");
        }
    }
}
