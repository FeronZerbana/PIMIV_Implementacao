using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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

        public AdminController(
            IProdutoService produtoService, 
            IEstoqueService estoqueService,
            ICupomService cupomService,
            IWebHostEnvironment webHostEnvironment)
        {
            _produtoService = produtoService;
            _estoqueService = estoqueService;
            _cupomService = cupomService;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var produtos = await _produtoService.ObterTodosAsync();
            var cupons = await _cupomService.ObterTodosAsync();

            var viewModel = new AdminViewModel
            {
                ProdutosCadastrados = produtos,
                NovoProduto = new NovoProdutoViewModel(),
                AdicionarEstoque = new AdicionarEstoqueViewModel(),
                CuponsCadastrados = cupons,
                NovoCupom = new NovoCupomViewModel()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CadastrarProduto([Bind(Prefix = "NovoProduto")] NovoProdutoViewModel? model)
        {
            model ??= new NovoProdutoViewModel();

            // Extração manual de contingência caso o model binder não tenha mapeado pelo prefixo
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
                TempData["MensagemToast"] = "Por favor, preencha a categoria, a descrição/modelo e um preço válido maior que zero.";
                return RedirectToAction(nameof(Index));
            }

            string? finalImagemUrl = !string.IsNullOrWhiteSpace(model.ImagemUrl) ? model.ImagemUrl.Trim() : null;

            if (arquivoUpload != null && arquivoUpload.Length > 0)
            {
                var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                var extensao = Path.GetExtension(arquivoUpload.FileName).ToLowerInvariant();

                if (!extensoesPermitidas.Contains(extensao))
                {
                    TempData["MensagemToast"] = "Formato de imagem inválido. São permitidos apenas arquivos .jpg, .jpeg, .png, .webp e .gif.";
                    return RedirectToAction(nameof(Index));
                }

                if (arquivoUpload.Length > 5 * 1024 * 1024)
                {
                    TempData["MensagemToast"] = "A imagem excede o tamanho máximo de 5MB.";
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
                ImagemUrl = finalImagemUrl
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
        public async Task<IActionResult> AdicionarEstoque([Bind(Prefix = "AdicionarEstoque")] AdicionarEstoqueViewModel? model)
        {
            model ??= new AdicionarEstoqueViewModel();

            if (model.IdProduto <= 0)
            {
                var idStr = Request.Form["AdicionarEstoque.IdProduto"].FirstOrDefault() 
                         ?? Request.Form["IdProduto"].FirstOrDefault();
                if (int.TryParse(idStr, out int idVal)) model.IdProduto = idVal;
            }

            if (model.Quantidade <= 0)
            {
                var qtdStr = Request.Form["AdicionarEstoque.Quantidade"].FirstOrDefault() 
                          ?? Request.Form["Quantidade"].FirstOrDefault();
                if (int.TryParse(qtdStr, out int qtdVal)) model.Quantidade = qtdVal;
            }

            if (model.IdProduto <= 0 || model.Quantidade <= 0)
            {
                TempData["MensagemToast"] = "Selecione um produto e informe uma quantidade positiva para o estoque.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                int novaQuantidade = await _estoqueService.AdicionarEstoqueAsync(model.IdProduto, model.Quantidade);
                TempData["MensagemToast"] = $"Estoque atualizado com sucesso! Novo saldo: {novaQuantidade} un.";
            }
            catch (Exception ex)
            {
                TempData["MensagemToast"] = $"Erro ao atualizar estoque: {ex.Message}";
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
                    TempData["MensagemToast"] = "Produto removido com sucesso.";
                }
                else
                {
                    TempData["MensagemToast"] = "Produto não encontrado.";
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

            if (model.Desconto <= 0)
            {
                var descStr = Request.Form["NovoCupom.Desconto"].FirstOrDefault() 
                           ?? Request.Form["Desconto"].FirstOrDefault();
                if (int.TryParse(descStr, out int descVal)) model.Desconto = descVal;
            }

            if (string.IsNullOrWhiteSpace(model.Valor) || model.Desconto < 1 || model.Desconto > 99)
            {
                TempData["MensagemToast"] = "Por favor, preencha o código e o percentual de desconto (1% a 99%) corretamente.";
                return RedirectToAction(nameof(Index));
            }

            var (sucesso, mensagem) = await _cupomService.CriarCupomAsync(model.Valor, model.Desconto);
            TempData["MensagemToast"] = mensagem;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirCupom(int id)
        {
            bool excluido = await _cupomService.ExcluirCupomAsync(id);
            if (excluido)
            {
                TempData["MensagemToast"] = "Cupom removido com sucesso do banco de dados.";
            }
            else
            {
                TempData["MensagemToast"] = "Cupom não encontrado.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
