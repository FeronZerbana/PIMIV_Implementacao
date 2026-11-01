using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Services;

namespace PIM_WebApplication.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class CuponsController : ControllerBase
    {
        private readonly ICupomService _cupomService;

        public CuponsController(ICupomService cupomService)
        {
            _cupomService = cupomService;
        }

        // GET: api/cupons/validar?codigo=ROCK15&codCliente=1
        [HttpGet("validar")]
        public async Task<IActionResult> Validar([FromQuery] string codigo, [FromQuery] int? codCliente = null)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return BadRequest(new { valido = false, mensagem = "Codigo do cupom e obrigatorio.", desconto = 0 });
            }

            var (valido, mensagem, desconto) = await _cupomService.ValidarCupomClienteAsync(codigo, codCliente);
            if (!valido)
            {
                return BadRequest(new { valido = false, mensagem, desconto = 0 });
            }

            return Ok(new
            {
                valido = true,
                codigo = codigo.Trim().ToUpperInvariant(),
                desconto,
                mensagem
            });
        }
    }
}
