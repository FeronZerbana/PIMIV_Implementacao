using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Models;
using PIM_WebApplication.Services;

namespace PIM_WebApplication.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class VendasController : ControllerBase
    {
        private readonly IVendaService _vendaService;

        public VendasController(IVendaService vendaService)
        {
            _vendaService = vendaService;
        }

        [HttpPost("finalizar")]
        public async Task<IActionResult> FinalizarVenda([FromBody] CarrinhoRequest request)
        {
            var (sucesso, mensagem, vendaId) = await _vendaService.FinalizarVendaAsync(
                request.CodCliente,
                request.Itens,
                request.Cupom);

            if (!sucesso)
            {
                return BadRequest(new { erro = mensagem });
            }

            return Ok(new
            {
                mensagem = "Compra realizada com sucesso",
                vendaID = vendaId
            });
        }
    }
}
