using Entregas.API.Data;
using Entregas.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Entregas.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EntregasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EntregasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/entregas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EntregaModel>>> GetEntregas()
        {
            return await _context.Entregas.ToListAsync();
        }

        // GET: api/entregas/ENT-1234
        [HttpGet("{codigo}")]
        public async Task<ActionResult<EntregaModel>> GetEntrega(string codigo)
        {
            var entrega = await _context.Entregas.FirstOrDefaultAsync(e => e.CodigoEntrega == codigo);

            if (entrega == null)
            {
                return NotFound();
            }

            return entrega;
        }

        // POST: api/entregas
        [HttpPost]
        public async Task<ActionResult<EntregaModel>> PostEntrega(EntregaModel entrega)
        {
            _context.Entregas.Add(entrega);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEntrega), new { codigo = entrega.CodigoEntrega }, entrega);
        }
    }
}