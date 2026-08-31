using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Models;
using PIM_WebApplication.Services;

namespace PIM_WebApplication.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProdutosController : ControllerBase
    {
        private readonly IProdutoService _produtoService;

        public ProdutosController(IProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            var produtos = await _produtoService.ObterTodosAsync();
            return Ok(produtos);
        }

        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto([FromBody] Produto produto)
        {
            var criado = await _produtoService.CriarProdutoAsync(produto, 0);
            return CreatedAtAction(nameof(GetProdutos), new { id = criado.IdProduto }, criado);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            try
            {
                bool excluido = await _produtoService.ExcluirProdutoAsync(id);
                if (!excluido)
                {
                    return NotFound(new { mensagem = "Produto não encontrado" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }
    }
}
