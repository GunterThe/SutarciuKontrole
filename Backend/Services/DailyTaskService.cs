using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

public class DailyTaskService : BackgroundService
{
    private readonly ILogger<DailyTaskService> _logger;
    private readonly IServiceScopeFactory _scopeFactory; // Use IServiceScopeFactory to create a scope
    private readonly EmailService _emailService;
    private readonly TimeSpan _targetTime = new TimeSpan(8, 0, 0);
    private readonly IConfiguration _configuration;

    public DailyTaskService(ILogger<DailyTaskService> logger, IServiceScopeFactory scopeFactory, EmailService emailService, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _emailService = emailService;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _scopeFactory.CreateScope()) // Create a new scope
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>(); // Resolve AppDbContext

            if (!context.Naudotojas.Any())
            {
                _logger.LogInformation("Database is empty. Updating database with data from API.");
                await UpdateDB(context); // Pass the context to UpdateDB
            }
        }

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
                using (var scope = _scopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    await UpdateDB(context);
                    await PerformTaskAsync(context);
                }
            }
        }
    }

    private async Task UpdateDB(AppDbContext context)
    {
        try
        {
            using var httpClient = new HttpClient();
            string url = _configuration["DBApi:url"] + _configuration["DBApi:key"];
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var xmlContent = await response.Content.ReadAsStringAsync();
                var xmlDoc = XDocument.Parse(xmlContent);
                var naudotojas = xmlDoc.Descendants("buh_irasas")
                    .Where(x => !string.IsNullOrWhiteSpace(x.Element("pareigos")?.Value) && x.Element("pareigos").Value.Trim().ToLower() != "atleistieji")
                    .Select(x => new Naudotojas
                    {
                        El_pastas = x.Element("tableio_nr").Value.Trim(),
                        Vardas = x.Element("vardas").Value.Trim(),
                        Pavarde = x.Element("pavarde").Value.Trim(),
                        Pareigos = x.Element("pareigos").Value.Trim(),
                        Id = x.Element("vardas").Value + x.Element("pavarde").Value.Trim(),
                        Gimimo_data = DateTime.Now,
                        Adminas = false
                    }).GroupBy(n => n.Id)
                    .Select(g => g.OrderByDescending(n => int.Parse(n.El_pastas)).First())
                    .ToList();

                foreach (var n in naudotojas)
                {
                    var existingUser = await context.Naudotojas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == n.Id);
                    if (existingUser == null)
                    {
                        context.Naudotojas.Add(n);
                    }
                    else if (int.Parse(n.El_pastas) < int.Parse(existingUser.El_pastas))
                    {
                        context.Entry(existingUser).State = EntityState.Detached;
                        context.Naudotojas.Attach(n);
                        context.Entry(n).State = EntityState.Modified;

                        var irasai = await context.Irasas
                            .Include(i => i.Naudotojai)
                            .Where(i => i.Naudotojai.Any(inu => inu.NaudotojasId == existingUser.Id && inu.Prekes_Adminas))
                            .ToListAsync();

                        foreach (var irasas in irasai)
                        {
                            var subject = $"{irasas.Pavadinimas} prekes administratorius pakeitė pareigas";
                            var message = $"Sveiki,\n\nPranešame, kad {existingUser.Vardas} {existingUser.Pavarde} pakeitė pareigas ir dabar yra {existingUser.Pareigos}.\n\nGeros dienos!";
                            await _emailService.SendEmailAsync(irasas.Pastas_kreiptis, subject, message);
                        }
                    }
                }

                await context.SaveChangesAsync();
            }
            else
            {
                _logger.LogError($"Failed to fetch data from the API. Status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while updating the database: {ex.Message}");
        }
    }

    private async Task PerformTaskAsync(AppDbContext context)
    {
        try
        {
            var today = DateTime.Today;
            var irasai = await context.Irasas
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

            await context.SaveChangesAsync();
            _logger.LogInformation("Daily task executed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred while executing the daily task: {ex.Message}");
        }
    }
}
