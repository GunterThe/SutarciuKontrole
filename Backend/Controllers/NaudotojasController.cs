using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class NaudotojasController : ControllerBase
{
    private readonly AppDbContext _context;

    public NaudotojasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/Naudotojas
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Naudotojas>>> GetNaudotojas()
    {
        return await _context.Naudotojas.ToListAsync();
    }

    // GET: api/Naudotojas/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Naudotojas>> GetNaudotojas(string id)
    {
        var naudotojas = await _context.Naudotojas.FindAsync(id);

        if (naudotojas == null)
        {
            return NotFound();
        }

        return naudotojas;
    }

    // POST: api/Naudotojas
    [HttpPost]
    public async Task<ActionResult<Naudotojas>> CreateNaudotojas(Naudotojas naudotojas)
    {
        _context.Naudotojas.Add(naudotojas);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNaudotojas), new { id = naudotojas.Id }, naudotojas);
    }

    // PUT: api/Naudotojas/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateNaudotojas(string id, Naudotojas naudotojas)
    {
        if (id != naudotojas.Id)
        {
            return BadRequest();
        }

        _context.Entry(naudotojas).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!NaudotojasExists(id))
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

    // DELETE: api/Naudotojas/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteNaudotojas(string id)
    {
        var naudotojas = await _context.Naudotojas.FindAsync(id);
        if (naudotojas == null)
        {
            return NotFound();
        }

        _context.Naudotojas.Remove(naudotojas);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool NaudotojasExists(string id)
    {
        return _context.Naudotojas.Any(e => e.Id == id);
    }
}