using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Services;

namespace PIM_WebApplication.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class EstoqueController : ControllerBase
    {
        private readonly IEstoqueService _estoqueService;

        public EstoqueController(IEstoqueService estoqueService)
        {
            _estoqueService = estoqueService;
        }

        [HttpPut("Adicionar/{idProduto}")]
        public async Task<IActionResult> AdicionarEstoque(int idProduto, [FromBody] int quantidade)
        {
            try
            {
                int novaQtd = await _estoqueService.AdicionarEstoqueAsync(idProduto, quantidade);
                return Ok(new { mensagem = "Estoque atualizado", novaQuantidade = novaQtd });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
