using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Helpers;
using PIM_WebApplication.Models;
using PIM_WebApplication.Services;
using PIM_WebApplication.ViewModels;

namespace PIM_WebApplication.Controllers
{
    public class CarrinhoController : Controller
    {
        private const string SessionCartKey = "SHOPNOW_CART_ITEMS";
        private const string SessionCouponKey = "SHOPNOW_COUPON_CODE";

        private readonly IProdutoService _produtoService;
        private readonly IVendaService _vendaService;

        public CarrinhoController(IProdutoService produtoService, IVendaService vendaService)
        {
            _produtoService = produtoService;
            _vendaService = vendaService;
        }

        private List<ItemCarrinhoViewModel> ObterCarrinhoDaSessao()
        {
            return HttpContext.Session.GetObject<List<ItemCarrinhoViewModel>>(SessionCartKey) ?? new List<ItemCarrinhoViewModel>();
        }

        private void SalvarCarrinhoNaSessao(List<ItemCarrinhoViewModel> itens)
        {
            HttpContext.Session.SetObject(SessionCartKey, itens);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var itens = ObterCarrinhoDaSessao();
            var cupom = HttpContext.Session.GetString(SessionCouponKey);

            var viewModel = new CarrinhoViewModel
            {
                Itens = itens,
                CupomAplicado = cupom
            };

            if (!string.IsNullOrEmpty(cupom))
            {
                decimal desconto = await _vendaService.CalcularDescontoCupomAsync(cupom, viewModel.Subtotal);
                viewModel.Desconto = desconto;
                viewModel.CupomValido = desconto > 0;
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Adicionar(
            int idProduto = 0, 
            string nome = "", 
            decimal preco = 0, 
            string tamanho = "", 
            string modelo = "", 
            string imagemUrl = "", 
            int quantidade = 1)
        {
            if (quantidade <= 0) quantidade = 1;

            Produto? produto = null;
            if (idProduto > 0)
            {
                produto = await _produtoService.ObterPorIdAsync(idProduto);
            }

            int finalId;
            string finalNome;
            decimal finalPreco;
            string finalVariacao;
            int estoqueDisponivel;

            if (produto != null)
            {
                finalId = produto.IdProduto;
                finalNome = produto.TipoInstrumento;
                finalPreco = produto.Preco;
                finalVariacao = !string.IsNullOrWhiteSpace(modelo) ? modelo : (!string.IsNullOrWhiteSpace(tamanho) ? tamanho : produto.Modelo);
                if (string.IsNullOrWhiteSpace(imagemUrl) && !string.IsNullOrWhiteSpace(produto.ImagemUrl))
                {
                    imagemUrl = produto.ImagemUrl;
                }
                estoqueDisponivel = (produto.Estoque != null && produto.Estoque.QuantidadeDisponivel > 0)
                    ? produto.Estoque.QuantidadeDisponivel
                    : 20;
            }
            else if (!string.IsNullOrWhiteSpace(nome) && preco > 0)
            {
                finalId = idProduto > 0 ? idProduto : Math.Abs((nome + (modelo ?? "")).GetHashCode());
                finalNome = nome;
                finalPreco = preco;
                finalVariacao = !string.IsNullOrWhiteSpace(modelo) ? modelo : (!string.IsNullOrWhiteSpace(tamanho) ? tamanho : "Padrao");
                estoqueDisponivel = 20;
            }
            else
            {
                TempData["MensagemToast"] = "Produto nao encontrado.";
                return RedirectToAction("Index", "Home");
            }

            var itens = ObterCarrinhoDaSessao();
            var itemExistente = itens.FirstOrDefault(i => i.IdProduto == finalId);

            if (itemExistente != null)
            {
                if (itemExistente.Quantidade + quantidade > estoqueDisponivel)
                {
                    TempData["MensagemToast"] = "Quantidade solicitada excede o estoque disponivel.";
                    return RedirectToAction(nameof(Index));
                }
                itemExistente.Quantidade += quantidade;
            }
            else
            {
                itens.Add(new ItemCarrinhoViewModel
                {
                    IdProduto = finalId,
                    Nome = finalNome,
                    Preco = finalPreco,
                    Tamanho = finalVariacao,
                    ImagemUrl = imagemUrl,
                    Emoji = string.Empty,
                    Quantidade = Math.Min(quantidade, estoqueDisponivel),
                    EstoqueDisponivel = estoqueDisponivel
                });
            }

            SalvarCarrinhoNaSessao(itens);
            TempData["MensagemToast"] = $"'{finalNome}' adicionado ao carrinho!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AlterarQuantidade(int idProduto, int delta)
        {
            var itens = ObterCarrinhoDaSessao();
            var item = itens.FirstOrDefault(i => i.IdProduto == idProduto);

            if (item != null)
            {
                item.Quantidade += delta;
                if (item.Quantidade <= 0)
                {
                    itens.Remove(item);
                }
                else if (item.Quantidade > item.EstoqueDisponivel)
                {
                    item.Quantidade = item.EstoqueDisponivel;
                    TempData["MensagemToast"] = "Estoque maximo atingido para este item.";
                }
                SalvarCarrinhoNaSessao(itens);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remover(int idProduto)
        {
            var itens = ObterCarrinhoDaSessao();
            var item = itens.FirstOrDefault(i => i.IdProduto == idProduto);
            if (item != null)
            {
                itens.Remove(item);
                SalvarCarrinhoNaSessao(itens);
                TempData["MensagemToast"] = "Item removido do carrinho.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AplicarCupom(string codigoCupom)
        {
            var itens = ObterCarrinhoDaSessao();
            decimal subtotal = itens.Sum(i => i.Subtotal);

            if (string.IsNullOrWhiteSpace(codigoCupom))
            {
                HttpContext.Session.Remove(SessionCouponKey);
                TempData["MensagemToast"] = "Cupom removido.";
                return RedirectToAction(nameof(Index));
            }

            decimal desconto = await _vendaService.CalcularDescontoCupomAsync(codigoCupom, subtotal);
            if (desconto > 0)
            {
                HttpContext.Session.SetString(SessionCouponKey, codigoCupom.Trim().ToUpperInvariant());
                TempData["MensagemToast"] = $"Cupom {codigoCupom.ToUpperInvariant()} aplicado com sucesso! Desconto de R$ {desconto:N2}";
            }
            else
            {
                HttpContext.Session.Remove(SessionCouponKey);
                TempData["MensagemToast"] = "Cupom invalido ou nao elegivel para esta compra.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Finalizar()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["MensagemToast"] = "Faca seu login para concluir a compra!";
                return RedirectToAction("Login", "Conta", new { returnUrl = Url.Action("Index", "Carrinho") });
            }

            var itens = ObterCarrinhoDaSessao();
            if (!itens.Any())
            {
                TempData["MensagemToast"] = "Seu carrinho esta vazio.";
                return RedirectToAction(nameof(Index));
            }

            var idClienteClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClienteClaim, out int idCliente))
            {
                TempData["MensagemToast"] = "Sessao invalida. Por favor, autentique-se novamente.";
                return RedirectToAction("Login", "Conta");
            }

            var dtos = itens.Select(i => new ItemCarrinhoDto
            {
                CodProduto = i.IdProduto,
                Quantidade = i.Quantidade,
                Nome = i.Nome,
                PrecoUnitario = i.Preco
            }).ToList();

            var cupom = HttpContext.Session.GetString(SessionCouponKey);
            var (sucesso, mensagem, vendaId) = await _vendaService.FinalizarVendaAsync(idCliente, dtos, cupom);

            if (sucesso)
            {
                HttpContext.Session.Remove(SessionCartKey);
                HttpContext.Session.Remove(SessionCouponKey);

                TempData["MensagemToast"] = $"Pedido #{vendaId} realizado com sucesso! Obrigado pela compra.";
                return RedirectToAction("Index", "Home");
            }

            TempData["MensagemToast"] = $"Erro ao finalizar compra: {mensagem}";
            return RedirectToAction(nameof(Index));
        }
    }
}
