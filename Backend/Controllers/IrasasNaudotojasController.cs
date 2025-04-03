using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class IrasasNaudotojasController : ControllerBase
{
    private readonly AppDbContext _context;

    public IrasasNaudotojasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/IrasasNaudotojas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<IrasasNaudotojas>>> GetIrasasNaudotojai()
    {
        return await _context.IrasasNaudotojas
            .Include(inu => inu.Irasas)
            .Include(inu => inu.Naudotojas)
            .ToListAsync();
    }

    // GET: api/IrasasNaudotojas/{irasaId}/{naudotojasId}
    [HttpGet("{irasaId}/{naudotojasId}")]
    public async Task<ActionResult<IrasasNaudotojas>> GetIrasasNaudotojas(int irasaId, string naudotojasId)
    {
        var irasasNaudotojas = await _context.IrasasNaudotojas
            .Include(inu => inu.Irasas)
            .Include(inu => inu.Naudotojas)
            .FirstOrDefaultAsync(inu => inu.IrasasId == irasaId && inu.NaudotojasId == naudotojasId);

        if (irasasNaudotojas == null)
        {
            return NotFound();
        }

        return irasasNaudotojas;
    }

    // POST: api/IrasasNaudotojas
    [HttpPost]
    public async Task<ActionResult<IrasasNaudotojas>> CreateIrasasNaudotojas(IrasasNaudotojas irasasNaudotojas)
    {
        _context.IrasasNaudotojas.Add(irasasNaudotojas);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetIrasasNaudotojas), new { irasaId = irasasNaudotojas.IrasasId, naudotojasId = irasasNaudotojas.NaudotojasId }, irasasNaudotojas);
    }

    // PUT: api/IrasasNaudotojas/{irasaId}/{naudotojasId}
    [HttpPut("{irasaId}/{naudotojasId}")]
    public async Task<IActionResult> UpdateIrasasNaudotojas(int irasaId, string naudotojasId, IrasasNaudotojas irasasNaudotojas)
    {
        if (irasaId != irasasNaudotojas.IrasasId || naudotojasId != irasasNaudotojas.NaudotojasId)
        {
            return BadRequest();
        }

        _context.Entry(irasasNaudotojas).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!IrasasNaudotojasExists(irasaId, naudotojasId))
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

    // DELETE: api/IrasasNaudotojas/{irasaId}/{naudotojasId}
    [HttpDelete("{irasaId}/{naudotojasId}")]
    public async Task<IActionResult> DeleteIrasasNaudotojas(int irasaId, string naudotojasId)
    {
        var irasasNaudotojas = await _context.IrasasNaudotojas
            .FirstOrDefaultAsync(inu => inu.IrasasId == irasaId && inu.NaudotojasId == naudotojasId);

        if (irasasNaudotojas == null)
        {
            return NotFound();
        }

        _context.IrasasNaudotojas.Remove(irasasNaudotojas);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool IrasasNaudotojasExists(int irasaId, string naudotojasId)
    {
        return _context.IrasasNaudotojas.Any(inu => inu.IrasasId == irasaId && inu.NaudotojasId == naudotojasId);
    }
}