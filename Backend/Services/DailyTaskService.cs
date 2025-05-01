using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;

public class DailyTaskService : BackgroundService
{
    private readonly ILogger<DailyTaskService> _logger;
    private readonly AppDbContext _context;
    private readonly EmailService _emailService; 
    private readonly TimeSpan _targetTime = new TimeSpan(8, 0, 0);
    private readonly IConfiguration _configuration;

    public DailyTaskService(ILogger<DailyTaskService> logger, AppDbContext context, EmailService emailService,IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _emailService = emailService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_context.Naudotojas.Any())
        {
            _logger.LogInformation("Database is empty. Updating database with data from API.");
            await UpdateDB();
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
                await UpdateDB();
                await PerformTaskAsync();
            }
        }
    }
    private async Task UpdateDB()
    {
        try
        {
            using var httpClient = new HttpClient();
            string url = _configuration["DBApi:url"]+_configuration["DBApi:key"];
            var response = await httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var xmlContent = await response.Content.ReadAsStringAsync();
                var xmlDoc = XDocument.Parse(xmlContent);
                var naudotojas = xmlDoc.Descendants("buh_irasas")
                    .Select(x => new Naudotojas
                    {
                        El_pastas = x.Element("tableio_nr").Value.Trim(),
                        Vardas = x.Element("vardas").Value.Trim(),
                        Pavarde = x.Element("pavarde").Value.Trim(),
                        Pareigos = x.Element("pareigos").Value.Trim(),
                        Id = x.Element("vardas").Value + x.Element("pavarde").Value.Trim(),
                        Gimimo_data = DateTime.Now,
                        Adminas = false
                    }).ToList();
                
                foreach (var n in naudotojas)
                {
                    var existingUser = await _context.Naudotojas.FirstOrDefaultAsync(u => u.Id == n.Id);
                    if (existingUser == null)
                    {
                        _context.Naudotojas.Add(n);
                    }
                    else if (int.Parse(n.El_pastas) < int.Parse(existingUser.El_pastas))
                    {
                        existingUser.Vardas = n.Vardas;
                        existingUser.Pavarde = n.Pavarde;
                        existingUser.Pareigos = n.Pareigos;
                        existingUser.El_pastas = n.El_pastas;

                        var irasai = await _context.Irasas
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
