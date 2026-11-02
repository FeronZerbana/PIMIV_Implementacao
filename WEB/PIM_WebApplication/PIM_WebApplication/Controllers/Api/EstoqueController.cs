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

        // GET: api/estoque/{idProduto}
        [HttpGet("{idProduto}")]
        public async Task<IActionResult> ObterEstoque(int idProduto)
        {
            var estoque = await _estoqueService.ObterPorProdutoAsync(idProduto);
            if (estoque == null)
            {
                return NotFound(new { mensagem = $"Registro de estoque para o produto ID {idProduto} nao encontrado." });
            }

            return Ok(new
            {
                idEstoque = estoque.IdEstoque,
                codProduto = estoque.CodProduto,
                quantidadeDisponivel = estoque.QuantidadeDisponivel
            });
        }

        // PUT: api/estoque/adicionar/{idProduto}
        [HttpPut("Adicionar/{idProduto}")]
        public async Task<IActionResult> AdicionarEstoque(int idProduto, [FromBody] int quantidade)
        {
            try
            {
                int novaQtd = await _estoqueService.AdicionarEstoqueAsync(idProduto, quantidade);
                return Ok(new { mensagem = "Estoque atualizado com sucesso", novaQuantidade = novaQtd });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // PUT: api/estoque/definir/{idProduto}
        [HttpPut("Definir/{idProduto}")]
        public async Task<IActionResult> DefinirEstoque(int idProduto, [FromBody] int novaQuantidade)
        {
            try
            {
                int novaQtd = await _estoqueService.DefinirEstoqueAsync(idProduto, novaQuantidade);
                return Ok(new { mensagem = "Estoque redefinido com sucesso", novaQuantidade = novaQtd });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
