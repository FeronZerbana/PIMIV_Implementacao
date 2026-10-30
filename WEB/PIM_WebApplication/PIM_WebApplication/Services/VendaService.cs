using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Data;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public interface IVendaService
    {
        Task<(bool Sucesso, string Mensagem, int VendaId)> FinalizarVendaAsync(int codCliente, List<ItemCarrinhoDto> itens, string? codigoCupom = null);
        Task<decimal> CalcularDescontoCupomAsync(string codigoCupom, decimal subtotal, int? codCliente = null);
        Task<List<VendaHistoricoDto>> ObterHistoricoPorClienteAsync(int codCliente);
    }

    public class VendaService : IVendaService
    {
        private readonly AppDbContext _context;
        private readonly ICupomService _cupomService;

        public VendaService(AppDbContext context, ICupomService cupomService)
        {
            _context = context;
            _cupomService = cupomService;
        }

        public async Task<decimal> CalcularDescontoCupomAsync(string codigoCupom, decimal subtotal, int? codCliente = null)
        {
            if (string.IsNullOrWhiteSpace(codigoCupom) || subtotal <= 0)
            {
                return 0m;
            }

            var (valido, _, percentualDesconto) = await _cupomService.ValidarCupomClienteAsync(codigoCupom, codCliente);
            if (!valido || percentualDesconto <= 0)
            {
                return 0m;
            }

            decimal desconto = Math.Round(subtotal * (percentualDesconto / 100m), 2, MidpointRounding.AwayFromZero);

            // Regra RN-PREC-04: Desconto maximo permitido de 50% do subtotal
            decimal maxDesconto = Math.Round(subtotal * 0.50m, 2, MidpointRounding.AwayFromZero);
            if (desconto > maxDesconto)
            {
                desconto = maxDesconto;
            }

            return desconto;
        }

        public async Task<(bool Sucesso, string Mensagem, int VendaId)> FinalizarVendaAsync(int codCliente, List<ItemCarrinhoDto> itens, string? codigoCupom = null)
        {
            if (itens == null || !itens.Any())
            {
                return (false, "O carrinho esta vazio.", 0);
            }

            // Validar cliente existente
            var cliente = await _context.Clientes.FindAsync(codCliente);
            if (cliente == null)
            {
                return (false, "Cliente nao encontrado.", 0);
            }

            // Validacao prévia de cupom se informado
            if (!string.IsNullOrWhiteSpace(codigoCupom))
            {
                var (cupomValido, msgCupom, _) = await _cupomService.ValidarCupomClienteAsync(codigoCupom, codCliente);
                if (!cupomValido)
                {
                    return (false, msgCupom, 0);
                }
            }

            // Executar em transacao atomica
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venda = new Venda
                {
                    CodCliente = codCliente,
                    DataVenda = DateTime.Now
                };

                _context.Vendas.Add(venda);
                await _context.SaveChangesAsync();

                decimal subtotalTotal = 0m;

                foreach (var item in itens)
                {
                    var produto = await _context.Produtos.FindAsync(item.CodProduto);
                    if (produto == null)
                    {
                        produto = new Produto
                        {
                            TipoInstrumento = !string.IsNullOrWhiteSpace(item.Nome) ? item.Nome : "Outros",
                            Preco = item.PrecoUnitario > 0 ? item.PrecoUnitario : 999.00m,
                            Modelo = "Padrao"
                        };
                        _context.Produtos.Add(produto);
                        await _context.SaveChangesAsync();
                        item.CodProduto = produto.IdProduto;
                    }

                    var estoque = await _context.Estoques.FirstOrDefaultAsync(e => e.CodProduto == item.CodProduto);
                    if (estoque == null)
                    {
                        estoque = new Estoque
                        {
                            CodProduto = item.CodProduto,
                            QuantidadeDisponivel = 50
                        };
                        _context.Estoques.Add(estoque);
                        await _context.SaveChangesAsync();
                    }

                    if (estoque.QuantidadeDisponivel < item.Quantidade)
                    {
                        throw new InvalidOperationException($"Estoque insuficiente para o instrumento '{produto.TipoInstrumento}'. Disponivel: {estoque.QuantidadeDisponivel}.");
                    }

                    decimal valorItem = produto.Preco;
                    decimal totalLinha = Math.Round(valorItem * item.Quantidade, 2, MidpointRounding.AwayFromZero);
                    subtotalTotal += totalLinha;

                    var itemVenda = new ItemVenda
                    {
                        CodVenda = venda.IdVenda,
                        CodProduto = item.CodProduto,
                        Quantidade = item.Quantidade,
                        ValorUnitario = valorItem,
                        ValorTotal = totalLinha
                    };

                    // A baixa definitiva e atomica de saldo e executada pelo trigger relacional 'trg_AtualizarEstoque_ItemVenda'
                    _context.ItemVendas.Add(itemVenda);
                }

                // Registro do uso unico do cupom
                if (!string.IsNullOrWhiteSpace(codigoCupom))
                {
                    await _cupomService.RegistrarUsoCupomAsync(codCliente, codigoCupom, venda.IdVenda);
                }

                // Atualizacao da regra de primeira compra (FirstTime)
                if (cliente.FirstTime)
                {
                    cliente.FirstTime = false;
                    _context.Clientes.Update(cliente);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Venda realizada com sucesso!", venda.IdVenda);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, ex.Message, 0);
            }
        }

        public async Task<List<VendaHistoricoDto>> ObterHistoricoPorClienteAsync(int codCliente)
        {
            var vendas = await _context.Vendas
                .Include(v => v.Itens)
                    .ThenInclude(i => i.Produto)
                .Where(v => v.CodCliente == codCliente)
                .OrderByDescending(v => v.DataVenda)
                .AsNoTracking()
                .ToListAsync();

            return vendas.Select(v => new VendaHistoricoDto
            {
                IdVenda = v.IdVenda,
                DataVenda = v.DataVenda,
                TotalVenda = v.Itens.Sum(i => i.ValorTotal),
                Itens = v.Itens.Select(i => new ItemVendaDetalheDto
                {
                    IdItemVenda = i.IdItemVenda,
                    CodProduto = i.CodProduto,
                    NomeProduto = i.Produto?.TipoInstrumento ?? $"Instrumento #{i.CodProduto}",
                    Quantidade = i.Quantidade,
                    ValorUnitario = i.ValorUnitario,
                    ValorTotal = i.ValorTotal
                }).ToList()
            }).ToList();
        }
    }
}
