using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Data;
using PIM_WebApplication.Models;
using PIM_WebApplication.Services;
using PIM_WebApplication.ViewModels;

namespace PIM_WebApplication.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IProdutoService _produtoService;
        private readonly IEstoqueService _estoqueService;
        private readonly ICupomService _cupomService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly AppDbContext _context;

        public AdminController(
            IProdutoService produtoService, 
            IEstoqueService estoqueService,
            ICupomService cupomService,
            IWebHostEnvironment webHostEnvironment,
            AppDbContext context)
        {
            _produtoService = produtoService;
            _estoqueService = estoqueService;
            _cupomService = cupomService;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var produtos = await _produtoService.ObterTodosAdminAsync();
            var cupons = await _cupomService.ObterTodosAsync();

            var auditorias = new List<AuditoriaVendaDto>();
            try
            {
                var conn = _context.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open)
                {
                    await conn.OpenAsync();
                }

                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    IF EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AUDITORIA_VENDAS')
                    BEGIN
                        SELECT TOP 10 id_Auditoria, id_Venda, Cod_do_Cliente, Data_Acao, Tipo_Acao, Usuario_SO 
                        FROM AUDITORIA_VENDAS 
                        ORDER BY id_Auditoria DESC;
                    END";

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    auditorias.Add(new AuditoriaVendaDto
                    {
                        IdAuditoria = reader.GetInt32(0),
                        IdVenda = reader.GetInt32(1),
                        CodCliente = reader.GetInt32(2),
                        DataAcao = reader.GetDateTime(3),
                        TipoAcao = reader.GetString(4),
                        UsuarioSO = reader.IsDBNull(5) ? string.Empty : reader.GetString(5)
                    });
                }
            }
            catch
            {
                // Resiliencia caso a tabela de auditoria ainda nao esteja populada
            }

            var viewModel = new AdminViewModel
            {
                ProdutosCadastrados = produtos,
                NovoProduto = new NovoProdutoViewModel(),
                EditarProduto = new EditarProdutoViewModel(),
                AlterarEstoque = new AlterarEstoqueViewModel(),
                CuponsCadastrados = cupons,
                NovoCupom = new NovoCupomViewModel(),
                RegistrosAuditoria = auditorias
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastrarProduto([Bind(Prefix = "NovoProduto")] NovoProdutoViewModel? model)
        {
            model ??= new NovoProdutoViewModel();

            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                model.Nome = Request.Form["NovoProduto.Nome"].FirstOrDefault() 
                          ?? Request.Form["Nome"].FirstOrDefault() 
                          ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(model.Modelo))
            {
                model.Modelo = Request.Form["NovoProduto.Modelo"].FirstOrDefault() 
                            ?? Request.Form["Modelo"].FirstOrDefault() 
                            ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(model.Descricao))
            {
                model.Descricao = Request.Form["NovoProduto.Descricao"].FirstOrDefault() 
                               ?? Request.Form["Descricao"].FirstOrDefault();
            }

            if (model.Preco <= 0)
            {
                var precoRaw = Request.Form["NovoProduto.Preco"].FirstOrDefault() 
                            ?? Request.Form["Preco"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(precoRaw))
                {
                    precoRaw = precoRaw.Replace(",", ".");
                    if (decimal.TryParse(precoRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal pVal))
                    {
                        model.Preco = pVal;
                    }
                }
            }

            if (model.EstoqueInicial <= 0)
            {
                var estRaw = Request.Form["NovoProduto.EstoqueInicial"].FirstOrDefault() 
                          ?? Request.Form["EstoqueInicial"].FirstOrDefault();
                if (int.TryParse(estRaw, out int estVal))
                {
                    model.EstoqueInicial = estVal;
                }
            }

            if (string.IsNullOrWhiteSpace(model.ImagemUrl))
            {
                model.ImagemUrl = Request.Form["NovoProduto.ImagemUrl"].FirstOrDefault() 
                               ?? Request.Form["ImagemUrl"].FirstOrDefault();
            }

            var arquivoUpload = model.ImagemArquivo 
                             ?? Request.Form.Files["NovoProduto.ImagemArquivo"] 
                             ?? Request.Form.Files["ImagemArquivo"] 
                             ?? (Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null);

            if (string.IsNullOrWhiteSpace(model.Nome) || string.IsNullOrWhiteSpace(model.Modelo) || model.Preco <= 0)
            {
                TempData["MensagemToast"] = "Por favor, preencha a categoria, a descricao/modelo e um preco valido maior que zero.";
                return RedirectToAction(nameof(Index));
            }

            string? finalImagemUrl = !string.IsNullOrWhiteSpace(model.ImagemUrl) ? model.ImagemUrl.Trim() : null;

            if (arquivoUpload != null && arquivoUpload.Length > 0)
            {
                var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var extensao = Path.GetExtension(arquivoUpload.FileName).ToLowerInvariant();

                if (!extensoesPermitidas.Contains(extensao))
                {
                    TempData["MensagemToast"] = "Formato de imagem invalido. Sao permitidos apenas arquivos .jpg, .jpeg, .png, .webp e .gif.";
                    return RedirectToAction(nameof(Index));
                }

                if (arquivoUpload.Length > 5 * 1024 * 1024)
                {
                    TempData["MensagemToast"] = "A imagem excede o tamanho maximo de 5MB.";
                    return RedirectToAction(nameof(Index));
                }

                var webRoot = _webHostEnvironment.WebRootPath 
                           ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "img", "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var nomeArquivoUnico = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extensao}";
                var caminhoCompleto = Path.Combine(uploadsFolder, nomeArquivoUnico);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await arquivoUpload.CopyToAsync(stream);
                }

                finalImagemUrl = $"/img/uploads/{nomeArquivoUnico}";
            }

            var produto = new Produto
            {
                TipoInstrumento = model.Nome.Trim(),
                Preco = model.Preco,
                Modelo = !string.IsNullOrWhiteSpace(model.Modelo) ? model.Modelo.Trim() : (model.Tamanho ?? string.Empty),
                Descricao = !string.IsNullOrWhiteSpace(model.Descricao) ? model.Descricao.Trim() : null,
                ImagemUrl = finalImagemUrl,
                Ativo = true
            };

            try
            {
                await _produtoService.CriarProdutoAsync(produto, model.EstoqueInicial);
                TempData["MensagemToast"] = $"Instrumento '{produto.Modelo}' cadastrado com sucesso na categoria '{produto.TipoInstrumento}' com {model.EstoqueInicial} unidades em estoque!";
            }
            catch (Exception ex)
            {
                TempData["MensagemToast"] = $"Erro ao cadastrar instrumento: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProduto([Bind(Prefix = "EditarProduto")] EditarProdutoViewModel? model)
        {
            model ??= new EditarProdutoViewModel();

            if (model.IdProduto <= 0)
            {
                var idStr = Request.Form["EditarProduto.IdProduto"].FirstOrDefault() 
                         ?? Request.Form["IdProduto"].FirstOrDefault();
                if (int.TryParse(idStr, out int idVal)) model.IdProduto = idVal;
            }

            if (string.IsNullOrWhiteSpace(model.Nome))
            {
                model.Nome = Request.Form["EditarProduto.Nome"].FirstOrDefault() 
                          ?? Request.Form["Nome"].FirstOrDefault() 
                          ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(model.Modelo))
            {
                model.Modelo = Request.Form["EditarProduto.Modelo"].FirstOrDefault() 
                            ?? Request.Form["Modelo"].FirstOrDefault() 
                            ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(model.Descricao))
            {
                model.Descricao = Request.Form["EditarProduto.Descricao"].FirstOrDefault() 
                               ?? Request.Form["Descricao"].FirstOrDefault();
            }

            if (model.Preco <= 0)
            {
                var precoRaw = Request.Form["EditarProduto.Preco"].FirstOrDefault() 
                            ?? Request.Form["Preco"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(precoRaw))
                {
                    precoRaw = precoRaw.Replace(",", ".");
                    if (decimal.TryParse(precoRaw, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal pVal))
                    {
                        model.Preco = pVal;
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(model.ImagemUrl))
            {
                model.ImagemUrl = Request.Form["EditarProduto.ImagemUrl"].FirstOrDefault() 
                               ?? Request.Form["ImagemUrl"].FirstOrDefault();
            }

            var arquivoUpload = model.ImagemArquivo 
                             ?? Request.Form.Files["EditarProduto.ImagemArquivo"] 
                             ?? Request.Form.Files["ImagemArquivo"] 
                             ?? (Request.Form.Files.Count > 0 ? Request.Form.Files[0] : null);

            if (model.IdProduto <= 0 || string.IsNullOrWhiteSpace(model.Nome) || string.IsNullOrWhiteSpace(model.Modelo) || model.Preco <= 0)
            {
                TempData["MensagemToast"] = "Por favor, preencha a categoria, a especificacao/modelo e um preco valido maior que zero.";
                return RedirectToAction(nameof(Index));
            }

            var produtoExistente = await _produtoService.ObterPorIdAsync(model.IdProduto);
            if (produtoExistente == null)
            {
                TempData["MensagemToast"] = "Produto nao encontrado para edicao.";
                return RedirectToAction(nameof(Index));
            }

            string? finalImagemUrl = !string.IsNullOrWhiteSpace(model.ImagemUrl) ? model.ImagemUrl.Trim() : produtoExistente.ImagemUrl;

            if (arquivoUpload != null && arquivoUpload.Length > 0)
            {
                var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var extensao = Path.GetExtension(arquivoUpload.FileName).ToLowerInvariant();

                if (!extensoesPermitidas.Contains(extensao))
                {
                    TempData["MensagemToast"] = "Formato de imagem invalido. Sao permitidos apenas arquivos .jpg, .jpeg, .png, .webp e .gif.";
                    return RedirectToAction(nameof(Index));
                }

                if (arquivoUpload.Length > 5 * 1024 * 1024)
                {
                    TempData["MensagemToast"] = "A imagem excede o tamanho maximo de 5MB.";
                    return RedirectToAction(nameof(Index));
                }

                var webRoot = _webHostEnvironment.WebRootPath 
                           ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
                var uploadsFolder = Path.Combine(webRoot, "img", "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var nomeArquivoUnico = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extensao}";
                var caminhoCompleto = Path.Combine(uploadsFolder, nomeArquivoUnico);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await arquivoUpload.CopyToAsync(stream);
                }

                finalImagemUrl = $"/img/uploads/{nomeArquivoUnico}";
            }

            produtoExistente.TipoInstrumento = model.Nome.Trim();
            produtoExistente.Modelo = model.Modelo.Trim();
            produtoExistente.Preco = model.Preco;
            produtoExistente.Descricao = !string.IsNullOrWhiteSpace(model.Descricao) ? model.Descricao.Trim() : null;
            produtoExistente.ImagemUrl = finalImagemUrl;

            var ativoForm = Request.Form["EditarProduto.Ativo"].FirstOrDefault() 
                         ?? Request.Form["Ativo"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(ativoForm))
            {
                if (bool.TryParse(ativoForm, out bool ativoBool))
                {
                    produtoExistente.Ativo = ativoBool;
                }
            }
            else
            {
                produtoExistente.Ativo = model.Ativo;
            }

            try
            {
                await _produtoService.AtualizarProdutoAsync(produtoExistente.IdProduto, produtoExistente);
                TempData["MensagemToast"] = $"Instrumento '{produtoExistente.Modelo}' atualizado com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["MensagemToast"] = $"Erro ao atualizar instrumento: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarEstoque([Bind(Prefix = "AlterarEstoque")] AlterarEstoqueViewModel? model)
        {
            model ??= new AlterarEstoqueViewModel();

            if (model.IdProduto <= 0)
            {
                var idStr = Request.Form["AlterarEstoque.IdProduto"].FirstOrDefault() 
                         ?? Request.Form["AdicionarEstoque.IdProduto"].FirstOrDefault() 
                         ?? Request.Form["IdProduto"].FirstOrDefault();
                if (int.TryParse(idStr, out int idVal)) model.IdProduto = idVal;
            }

            var tipoOperacao = Request.Form["AlterarEstoque.TipoOperacao"].FirstOrDefault() 
                            ?? Request.Form["TipoOperacao"].FirstOrDefault() 
                            ?? (string.IsNullOrWhiteSpace(model.TipoOperacao) ? "Incrementar" : model.TipoOperacao);

            var qtdStr = Request.Form["AlterarEstoque.Quantidade"].FirstOrDefault() 
                      ?? Request.Form["AdicionarEstoque.Quantidade"].FirstOrDefault() 
                      ?? Request.Form["Quantidade"].FirstOrDefault();
            if (int.TryParse(qtdStr, out int qtdVal))
            {
                model.Quantidade = qtdVal;
            }

            if (model.IdProduto <= 0)
            {
                TempData["MensagemToast"] = "Selecione um produto para alterar o estoque.";
                return RedirectToAction(nameof(Index));
            }

            if (tipoOperacao.Equals("Definir", StringComparison.OrdinalIgnoreCase))
            {
                if (model.Quantidade < 0)
                {
                    TempData["MensagemToast"] = "O novo saldo de estoque nao pode ser negativo.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    int novoSaldo = await _estoqueService.DefinirEstoqueAsync(model.IdProduto, model.Quantidade);
                    TempData["MensagemToast"] = $"Estoque redefinido com sucesso! Novo saldo absoluto: {novoSaldo} un.";
                }
                catch (Exception ex)
                {
                    TempData["MensagemToast"] = $"Erro ao definir estoque: {ex.Message}";
                }
            }
            else
            {
                if (model.Quantidade <= 0)
                {
                    TempData["MensagemToast"] = "A quantidade a adicionar deve ser maior que zero.";
                    return RedirectToAction(nameof(Index));
                }

                try
                {
                    int novoSaldo = await _estoqueService.AdicionarEstoqueAsync(model.IdProduto, model.Quantidade);
                    TempData["MensagemToast"] = $"Estoque incrementado com sucesso! Novo saldo: {novoSaldo} un.";
                }
                catch (Exception ex)
                {
                    TempData["MensagemToast"] = $"Erro ao incrementar estoque: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdicionarEstoque([Bind(Prefix = "AdicionarEstoque")] AdicionarEstoqueViewModel? model)
        {
            var alterarModel = new AlterarEstoqueViewModel
            {
                IdProduto = model?.IdProduto ?? 0,
                Quantidade = model?.Quantidade ?? 0,
                TipoOperacao = "Incrementar"
            };
            return await AlterarEstoque(alterarModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatusProduto(int id)
        {
            try
            {
                bool alterado = await _produtoService.AlternarStatusProdutoAsync(id);
                if (alterado)
                {
                    TempData["MensagemToast"] = "Status do produto atualizado com sucesso!";
                }
                else
                {
                    TempData["MensagemToast"] = "Produto nao encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemToast"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirProduto(int id)
        {
            try
            {
                bool excluido = await _produtoService.ExcluirProdutoAsync(id);
                if (excluido)
                {
                    TempData["MensagemToast"] = "Produto desativado com sucesso (soft delete).";
                }
                else
                {
                    TempData["MensagemToast"] = "Produto nao encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemToast"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastrarCupom([Bind(Prefix = "NovoCupom")] NovoCupomViewModel? model)
        {
            model ??= new NovoCupomViewModel();

            if (string.IsNullOrWhiteSpace(model.Valor))
            {
                model.Valor = Request.Form["NovoCupom.Valor"].FirstOrDefault() 
                           ?? Request.Form["Valor"].FirstOrDefault() 
                           ?? string.Empty;
            }

            var descStr = Request.Form["NovoCupom.Desconto"].FirstOrDefault() 
                       ?? Request.Form["Desconto"].FirstOrDefault() 
                       ?? (model.Desconto > 0 ? model.Desconto.ToString() : string.Empty);

            if (!string.IsNullOrWhiteSpace(descStr))
            {
                descStr = descStr.Replace("%", "").Trim();
                if (int.TryParse(descStr, out int descVal))
                {
                    model.Desconto = descVal;
                }
            }

            // Normaliza escala x10 caso seja informado valor em escala multiplicada (ex: 500 para 50%)
            if (model.Desconto >= 100 && model.Desconto <= 990)
            {
                model.Desconto = (int)Math.Round(model.Desconto / 10.0);
            }

            if (string.IsNullOrWhiteSpace(model.Valor) || model.Desconto < 1 || model.Desconto > 99)
            {
                TempData["MensagemToast"] = "Por favor, preencha o codigo do cupom e o desconto corretamente (ex: digite 50 para conceder 50% de desconto).";
                return RedirectToAction(nameof(Index));
            }

            var (sucesso, mensagem) = await _cupomService.CriarCupomAsync(model.Valor, model.Desconto);
            TempData["MensagemToast"] = mensagem;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatusCupom(int id)
        {
            try
            {
                bool alterado = await _cupomService.AlternarStatusCupomAsync(id);
                if (alterado)
                {
                    TempData["MensagemToast"] = "Status do cupom atualizado com sucesso!";
                }
                else
                {
                    TempData["MensagemToast"] = "Cupom nao encontrado.";
                }
            }
            catch (Exception ex)
            {
                TempData["MensagemToast"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirCupom(int id)
        {
            bool excluido = await _cupomService.ExcluirCupomAsync(id);
            if (excluido)
            {
                TempData["MensagemToast"] = "Cupom desativado com sucesso (soft delete).";
            }
            else
            {
                TempData["MensagemToast"] = "Cupom nao encontrado.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
