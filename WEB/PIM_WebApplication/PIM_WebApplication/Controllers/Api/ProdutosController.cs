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
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProdutosController> _logger;

        private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB

        public ProdutosController(
            IProdutoService produtoService,
            IWebHostEnvironment environment,
            ILogger<ProdutosController> logger)
        {
            _produtoService = produtoService;
            _environment = environment;
            _logger = logger;
        }

        // GET: api/produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutos()
        {
            var produtos = await _produtoService.ObterTodosAsync();
            var dtos = produtos.Select(p => MapearParaDto(p));
            return Ok(dtos);
        }

        // GET: api/produtos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDto>> GetProdutoPorId(int id)
        {
            var produto = await _produtoService.ObterPorIdAsync(id);
            if (produto == null)
            {
                return NotFound(new { mensagem = $"Instrumento com ID {id} nao encontrado." });
            }

            return Ok(MapearParaDto(produto));
        }

        // GET: api/produtos/categoria/{categoria}
        [HttpGet("categoria/{categoria}")]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> GetProdutosPorCategoria(string categoria)
        {
            var produtos = await _produtoService.ObterPorCategoriaAsync(categoria);
            var dtos = produtos.Select(p => MapearParaDto(p));
            return Ok(dtos);
        }

        // POST: api/produtos
        [HttpPost]
        public async Task<ActionResult<ProdutoDto>> PostProduto([FromBody] CriarProdutoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var produto = new Produto
            {
                TipoInstrumento = dto.TipoInstrumento.Trim(),
                Preco = dto.Preco,
                Modelo = dto.Modelo?.Trim() ?? string.Empty,
                ImagemUrl = !string.IsNullOrWhiteSpace(dto.ImagemUrl) ? dto.ImagemUrl.Trim() : null
            };

            var criado = await _produtoService.CriarProdutoAsync(produto, dto.EstoqueInicial);
            var resultadoDto = MapearParaDto(criado);

            return CreatedAtAction(nameof(GetProdutoPorId), new { id = criado.IdProduto }, resultadoDto);
        }

        // PUT: api/produtos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, [FromBody] AtualizarProdutoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var produtoAtualizado = new Produto
            {
                TipoInstrumento = dto.TipoInstrumento.Trim(),
                Preco = dto.Preco,
                Modelo = dto.Modelo?.Trim() ?? string.Empty,
                ImagemUrl = !string.IsNullOrWhiteSpace(dto.ImagemUrl) ? dto.ImagemUrl.Trim() : null
            };

            var resultado = await _produtoService.AtualizarProdutoAsync(id, produtoAtualizado);
            if (resultado == null)
            {
                return NotFound(new { mensagem = $"Instrumento com ID {id} nao encontrado." });
            }

            return NoContent();
        }

        // DELETE: api/produtos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            try
            {
                bool excluido = await _produtoService.ExcluirProdutoAsync(id);
                if (!excluido)
                {
                    return NotFound(new { mensagem = $"Instrumento com ID {id} nao encontrado." });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensagem = ex.Message });
            }
        }

        // POST: api/produtos/{id}/imagem (Upload multipart/form-data)
        [HttpPost("{id}/imagem")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<RespostaUploadImagemDto>> UploadImagem(int id, IFormFile? arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
            {
                return BadRequest(new { mensagem = "Nenhum arquivo de imagem foi enviado." });
            }

            if (arquivo.Length > TamanhoMaximoBytes)
            {
                return BadRequest(new { mensagem = "O tamanho da imagem excede o limite maximo de 5 MB." });
            }

            var extensao = Path.GetExtension(arquivo.FileName).ToLowerInvariant();
            if (!ExtensoesPermitidas.Contains(extensao))
            {
                return BadRequest(new { mensagem = "Formato de arquivo invalido. Formatos aceitos: .jpg, .jpeg, .png, .webp." });
            }

            var produto = await _produtoService.ObterPorIdAsync(id);
            if (produto == null)
            {
                return NotFound(new { mensagem = $"Instrumento com ID {id} nao encontrado." });
            }

            try
            {
                var pastaUploads = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "produtos");
                if (!Directory.Exists(pastaUploads))
                {
                    Directory.CreateDirectory(pastaUploads);
                }

                // Remove imagem física anterior se tiver sido gerada por upload
                RemoverArquivoImagemFisico(produto.ImagemUrl);

                var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
                var caminhoCompleto = Path.Combine(pastaUploads, nomeArquivo);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await arquivo.CopyToAsync(stream);
                }

                var urlRelativa = $"/uploads/produtos/{nomeArquivo}";
                await _produtoService.AtualizarImagemAsync(id, urlRelativa);

                _logger.LogInformation("Upload de foto concluido para o produto ID {ProdutoId}: {Caminho}", id, urlRelativa);

                return Ok(new RespostaUploadImagemDto
                {
                    Mensagem = "Imagem enviada e vinculada com sucesso.",
                    ImgURL = urlRelativa
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar upload de imagem para o instrumento ID {ProdutoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensagem = "Erro interno ao salvar a imagem." });
            }
        }

        private void RemoverArquivoImagemFisico(string? imgUrl)
        {
            if (string.IsNullOrWhiteSpace(imgUrl) || !imgUrl.StartsWith("/uploads/"))
            {
                return;
            }

            try
            {
                var caminhoLimpo = imgUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
                var basePath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var caminhoFisico = Path.Combine(basePath, caminhoLimpo);

                if (System.IO.File.Exists(caminhoFisico))
                {
                    System.IO.File.Delete(caminhoFisico);
                    _logger.LogInformation("Arquivo de imagem fisico removido: {CaminhoFisico}", caminhoFisico);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nao foi possivel remover arquivo de imagem anterior: {ImgURL}", imgUrl);
            }
        }

        private static ProdutoDto MapearParaDto(Produto p)
        {
            return new ProdutoDto
            {
                Id = p.IdProduto,
                TipoInstrumento = p.TipoInstrumento,
                Nome = p.TipoInstrumento,
                Preco = p.Preco,
                Modelo = p.Modelo,
                ImagemUrl = p.ImagemUrl,
                QuantidadeEstoque = p.Estoque?.QuantidadeDisponivel ?? 0
            };
        }
    }
}
