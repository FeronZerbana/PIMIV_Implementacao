using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Helpers;
using PIM_WebApplication.Models;
using PIM_WebApplication.Services;
using PIM_WebApplication.ViewModels;

namespace PIM_WebApplication.Controllers
{
    public class CarrinhoController : Controller
    {
        private const string SessionCartKey = "SessionCart";
        private const string SessionCouponKey = "SessionCoupon";

        private readonly IProdutoService _produtoService;
        private readonly IVendaService _vendaService;
        private readonly ICupomService _cupomService;
        private readonly IUsuarioService _usuarioService;

        public CarrinhoController(
            IProdutoService produtoService, 
            IVendaService vendaService,
            ICupomService cupomService,
            IUsuarioService usuarioService)
        {
            _produtoService = produtoService;
            _vendaService = vendaService;
            _cupomService = cupomService;
            _usuarioService = usuarioService;
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

            int? idCliente = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var idClienteClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(idClienteClaim, out int id))
                {
                    idCliente = id;
                    var cliente = await _usuarioService.ObterPorIdAsync(id);
                    if (cliente?.FirstTime == true)
                    {
                        viewModel.PrimeiraCompraElegivel = true;
                        viewModel.DescontoPrimeiraCompra = Math.Round(viewModel.Subtotal * 0.50m, 2);
                    }
                }
            }

            if (!string.IsNullOrEmpty(cupom))
            {
                var (valido, msg, _) = await _cupomService.ValidarCupomClienteAsync(cupom, idCliente);
                viewModel.CupomValido = valido;
                viewModel.MensagemCupom = msg;

                if (valido)
                {
                    decimal desconto = await _vendaService.CalcularDescontoCupomAsync(cupom, viewModel.Subtotal, idCliente);
                    viewModel.DescontoCupom = desconto;
                }
                else
                {
                    HttpContext.Session.Remove(SessionCouponKey);
                    viewModel.CupomAplicado = null;
                }
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Adicionar(
            int idProduto = 0, 
            string nome = "", 
            decimal preco = 0m, 
            string tamanho = "", 
            string modelo = "", 
            string imagemUrl = "", 
            int quantidade = 1)
        {
            if (quantidade <= 0)
            {
                quantidade = 1;
            }

            Produto? produto = null;
            if (idProduto > 0)
            {
                produto = await _produtoService.ObterPorIdAsync(idProduto);
            }

            if (produto == null && !string.IsNullOrWhiteSpace(nome))
            {
                var todos = await _produtoService.ObterTodosAsync();
                produto = todos.FirstOrDefault(p => 
                    p.TipoInstrumento.Equals(nome, StringComparison.OrdinalIgnoreCase) ||
                    p.Modelo.Equals(nome, StringComparison.OrdinalIgnoreCase));
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
        public IActionResult Remover(int idProduto)
        {
            var itens = ObterCarrinhoDaSessao();
            var item = itens.FirstOrDefault(i => i.IdProduto == idProduto);
            if (item != null)
            {
                itens.Remove(item);
                SalvarCarrinhoNaSessao(itens);
                TempData["MensagemToast"] = $"'{item.Nome}' removido do carrinho.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult AlterarQuantidade(int idProduto, int delta)
        {
            return AtualizarQuantidade(idProduto, delta);
        }

        [HttpPost]
        public IActionResult AtualizarQuantidade(int idProduto, int delta)
        {
            var itens = ObterCarrinhoDaSessao();
            var item = itens.FirstOrDefault(i => i.IdProduto == idProduto);

            if (item != null)
            {
                int novaQuantidade = item.Quantidade + delta;
                if (novaQuantidade > item.EstoqueDisponivel)
                {
                    TempData["MensagemToast"] = "Limite de estoque atingido para este item.";
                    return RedirectToAction(nameof(Index));
                }

                if (novaQuantidade <= 0)
                {
                    itens.Remove(item);
                    TempData["MensagemToast"] = $"'{item.Nome}' removido do carrinho.";
                }
                else
                {
                    item.Quantidade = novaQuantidade;
                }

                SalvarCarrinhoNaSessao(itens);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AplicarCupom(string codigoCupom)
        {
            if (string.IsNullOrWhiteSpace(codigoCupom))
            {
                HttpContext.Session.Remove(SessionCouponKey);
                TempData["MensagemToast"] = "Cupom removido.";
                return RedirectToAction(nameof(Index));
            }

            int? idCliente = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(claimId, out int id))
                {
                    idCliente = id;
                }
            }

            var (valido, mensagem, _) = await _cupomService.ValidarCupomClienteAsync(codigoCupom, idCliente);
            if (valido)
            {
                HttpContext.Session.SetString(SessionCouponKey, codigoCupom.Trim().ToUpperInvariant());
                TempData["MensagemToast"] = mensagem;
            }
            else
            {
                HttpContext.Session.Remove(SessionCouponKey);
                TempData["MensagemToast"] = mensagem;
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
