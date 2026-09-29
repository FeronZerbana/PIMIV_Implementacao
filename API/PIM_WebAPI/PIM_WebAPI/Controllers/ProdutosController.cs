using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM_WebAPI.Data;
using PIM_WebAPI.Models;
using PIM_WebAPI.Models.DTO;

namespace PIM_WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProdutosController> _logger;

        private static readonly string[] ExtensoesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanhoMaximoBytes = 5 * 1024 * 1024; // 5 MB

        public ProdutosController(AppDbContext context, IWebHostEnvironment environment, ILogger<ProdutosController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // GET: api/produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoDto>>> ObterTodos()
        {
            var produtos = await _context.Produtos
                .Include(p => p.Estoque)
                .AsNoTracking()
                .Select(p => new ProdutoDto
                {
                    ID = p.ID,
                    Tipo = p.Tipo,
                    Preco = p.Preco,
                    Descricao = p.Descricao,
                    ImgURL = p.ImgURL,
                    QuantidadeEstoque = p.Estoque != null ? p.Estoque.Quantidade : 0
                })
                .ToListAsync();

            return Ok(produtos);
        }

        // GET: api/produtos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoDto>> ObterPorId(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Estoque)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ID == id);

            if (produto == null)
            {
                return NotFound(new { mensagem = $"Produto com ID {id} nao encontrado." });
            }

            var dto = new ProdutoDto
            {
                ID = produto.ID,
                Tipo = produto.Tipo,
                Preco = produto.Preco,
                Descricao = produto.Descricao,
                ImgURL = produto.ImgURL,
                QuantidadeEstoque = produto.Estoque?.Quantidade ?? 0
            };

            return Ok(dto);
        }

        // POST: api/produtos
        [HttpPost]
        public async Task<ActionResult<ProdutoDto>> Criar([FromBody] CriarProdutoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var novoProduto = new Produto
            {
                Tipo = dto.Tipo,
                Preco = dto.Preco,
                Descricao = dto.Descricao ?? string.Empty
            };

            await _context.Produtos.AddAsync(novoProduto);
            await _context.SaveChangesAsync();

            // Criacao do saldo inicial de estoque associado (1:1)
            var estoque = new Estoque
            {
                ProdutoID = novoProduto.ID,
                Quantidade = dto.EstoqueInicial
            };

            await _context.Estoques.AddAsync(estoque);
            await _context.SaveChangesAsync();

            var resultado = new ProdutoDto
            {
                ID = novoProduto.ID,
                Tipo = novoProduto.Tipo,
                Preco = novoProduto.Preco,
                Descricao = novoProduto.Descricao,
                ImgURL = novoProduto.ImgURL,
                QuantidadeEstoque = estoque.Quantidade
            };

            return CreatedAtAction(nameof(ObterPorId), new { id = novoProduto.ID }, resultado);
        }

        // PUT: api/produtos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarProdutoDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound(new { mensagem = $"Produto com ID {id} nao encontrado." });
            }

            produto.Tipo = dto.Tipo;
            produto.Preco = dto.Preco;
            produto.Descricao = dto.Descricao ?? string.Empty;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/produtos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Excluir(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound(new { mensagem = $"Produto com ID {id} nao encontrado." });
            }

            // Exclusao da imagem física no disco caso exista
            RemoverArquivoImagemFisico(produto.ImgURL);

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
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

            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return NotFound(new { mensagem = $"Produto com ID {id} nao encontrado." });
            }

            try
            {
                // Diretorio wwwroot/uploads/produtos
                var pastaUploads = Path.Combine(_environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "produtos");
                if (!Directory.Exists(pastaUploads))
                {
                    Directory.CreateDirectory(pastaUploads);
                }

                // Remocao de imagem anterior se existir
                RemoverArquivoImagemFisico(produto.ImgURL);

                // Geracao de nome unico com GUID
                var nomeArquivo = $"{Guid.NewGuid():N}{extensao}";
                var caminhoCompleto = Path.Combine(pastaUploads, nomeArquivo);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await arquivo.CopyToAsync(stream);
                }

                // Caminho relativo para exposicao via URL
                var urlRelativa = $"/uploads/produtos/{nomeArquivo}";
                produto.ImgURL = urlRelativa;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Upload de imagem concluido para o produto ID {ProdutoId}: {Caminho}", id, urlRelativa);

                return Ok(new RespostaUploadImagemDto
                {
                    Mensagem = "Imagem enviada e vinculada com sucesso.",
                    ImgURL = urlRelativa
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar upload de imagem para o produto ID {ProdutoId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensagem = "Erro interno ao salvar a imagem." });
            }
        }

        private void RemoverArquivoImagemFisico(string? imgUrl)
        {
            if (string.IsNullOrWhiteSpace(imgUrl))
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
                    _logger.LogInformation("Arquivo de imagem removido: {CaminhoFisico}", caminhoFisico);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Nao foi possivel remover o arquivo de imagem fisico: {ImgURL}", imgUrl);
            }
        }
    }
}
