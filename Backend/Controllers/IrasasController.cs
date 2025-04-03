using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    // POST: api/Irasas
    [HttpPost]
    public async Task<ActionResult<Irasas>> CreateIrasas(Irasas irasas)
    {
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

    // DELETE: api/Irasas/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteIrasas(int id)
    {
        var irasas = await _context.Irasas.FindAsync(id);
        if (irasas == null)
        {
            return NotFound();
        }

        _context.Irasas.Remove(irasas);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool IrasasExists(int id)
    {
        return _context.Irasas.Any(e => e.Id == id);
    }
}