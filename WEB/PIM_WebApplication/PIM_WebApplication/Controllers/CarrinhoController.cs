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
        public async Task<IActionResult> Adicionar(int idProduto, string tamanho = "M", int quantidade = 1)
        {
            var produto = await _produtoService.ObterPorIdAsync(idProduto);
            if (produto == null)
            {
                TempData["MensagemToast"] = "Produto não encontrado.";
                return RedirectToAction("Index", "Home");
            }

            int estoqueDisponivel = produto.Estoque?.QuantidadeDisponivel ?? 0;
            if (estoqueDisponivel <= 0)
            {
                TempData["MensagemToast"] = "Produto fora de estoque!";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }

            var itens = ObterCarrinhoDaSessao();
            var itemExistente = itens.FirstOrDefault(i => i.IdProduto == idProduto);

            if (itemExistente != null)
            {
                if (itemExistente.Quantidade + quantidade > estoqueDisponivel)
                {
                    TempData["MensagemToast"] = "Quantidade solicitada excede o estoque disponível.";
                    return RedirectToAction(nameof(Index));
                }
                itemExistente.Quantidade += quantidade;
            }
            else
            {
                string emoji = "👕";
                var nomeLower = produto.TipoRoupa.ToLowerInvariant();
                if (nomeLower.Contains("moletom") || nomeLower.Contains("hoodie")) emoji = "🧥";
                else if (nomeLower.Contains("programação") || nomeLower.Contains("dev") || nomeLower.Contains("code")) emoji = "💻";
                else if (nomeLower.Contains("linux")) emoji = "🐧";
                else if (nomeLower.Contains("ia") || nomeLower.Contains("tech") || nomeLower.Contains("robot")) emoji = "🤖";
                else if (nomeLower.Contains("game")) emoji = "🎮";

                itens.Add(new ItemCarrinhoViewModel
                {
                    IdProduto = produto.IdProduto,
                    Nome = produto.TipoRoupa,
                    Preco = produto.Preco,
                    Tamanho = string.IsNullOrEmpty(tamanho) ? produto.Tamanho : tamanho,
                    Emoji = emoji,
                    Quantidade = Math.Min(quantidade, estoqueDisponivel),
                    EstoqueDisponivel = estoqueDisponivel
                });
            }

            SalvarCarrinhoNaSessao(itens);
            TempData["MensagemToast"] = "Produto adicionado ao carrinho!";
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
                    TempData["MensagemToast"] = "Estoque máximo atingido para este item.";
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
                TempData["MensagemToast"] = "Cupom inválido ou não elegível para esta compra.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Finalizar()
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                TempData["MensagemToast"] = "Faça seu login para concluir a compra!";
                return RedirectToAction("Login", "Conta", new { returnUrl = Url.Action("Index", "Carrinho") });
            }

            var itens = ObterCarrinhoDaSessao();
            if (!itens.Any())
            {
                TempData["MensagemToast"] = "Seu carrinho está vazio.";
                return RedirectToAction(nameof(Index));
            }

            var idClienteClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClienteClaim, out int idCliente))
            {
                TempData["MensagemToast"] = "Sessão inválida. Por favor, autentique-se novamente.";
                return RedirectToAction("Login", "Conta");
            }

            var dtos = itens.Select(i => new ItemCarrinhoDto
            {
                CodProduto = i.IdProduto,
                Quantidade = i.Quantidade
            }).ToList();

            var cupom = HttpContext.Session.GetString(SessionCouponKey);
            var (sucesso, mensagem, vendaId) = await _vendaService.FinalizarVendaAsync(idCliente, dtos, cupom);

            if (sucesso)
            {
                // Limpa carrinho e cupom
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
