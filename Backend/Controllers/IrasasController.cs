using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class IrasasController : ControllerBase
{
    private readonly AppDbContext _context;

    public IrasasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Irasas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Irasas>>> GetIrasas()
    {
        return await _context.Irasas.ToListAsync();
    }

    // GET: api/Irasas/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Irasas>> GetIrasas(int id)
    {
        var irasas = await _context.Irasas.FindAsync(id);

        if (irasas == null)
        {
            return NotFound();
        }

        return irasas;
    }

    // POST: api/Irasas/{id}/Archive
    [HttpPost("{id}/Archive")]
    public async Task<ActionResult<Irasas>> ArchiveIrasas(int id)
    {
        var irasas = await _context.Irasas.FindAsync(id);
        if(irasas == null)
        {
            return NotFound();
        }

        irasas.Archyvuotas = true;
        _context.Entry(irasas).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return irasas;
    }

    // GET: api/Irasas/{id}
    [HttpGet("{id}/Naudotojai")]
    public async Task<ActionResult<Irasas>> GetIrasasNaudotojai(int id)
    {
        var irasas = await _context.Irasas
        .Include(i => i.Naudotojai.Where(inu => inu.Prekes_Adminas))
            .ThenInclude(inu => inu.Naudotojas)
        .FirstOrDefaultAsync(i => i.Id == id);

        if (irasas == null)
        {
            return NotFound();
        }

        var Naudotojai = irasas.Naudotojai.Select(n => new
        {
            n.Naudotojas.Id,
            n.Naudotojas.Vardas,
            n.Naudotojas.Pavarde,
            n.Naudotojas.Pareigos
        });

        return Ok(Naudotojai);
    }

    // POST: api/Irasas
    [HttpPost]
    public async Task<ActionResult<Irasas>> CreateIrasas([FromBody] Irasas irasas)
    {
        irasas.Id = 0;
        Console.WriteLine(irasas);
        if(irasas == null)
        {
            return BadRequest("Irasas object is null");
        }
        if (irasas.Dienos_pries > 0){
            irasas.Kita_data = irasas.Pabaigos_data.Subtract(TimeSpan.FromDays(irasas.Dienos_pries));
        }
        else
        {
            irasas.Kita_data = DateTime.Now.AddDays(irasas.Dienu_daznumas);
        }
        _context.Irasas.Add(irasas);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetIrasas), new { id = irasas.Id }, irasas);
    }

    // PUT: api/Irasas/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIrasas(int id, Irasas irasas)
    {
        if (id != irasas.Id)
        {
            return BadRequest();
        }

        _context.Entry(irasas).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!IrasasExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    private bool IrasasExists(int id)
    {
        return _context.Irasas.Any(e => e.Id == id);
    }
}