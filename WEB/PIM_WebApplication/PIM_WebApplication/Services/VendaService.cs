using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Data;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public interface IVendaService
    {
        Task<(bool Sucesso, string Mensagem, int VendaId)> FinalizarVendaAsync(int codCliente, List<ItemCarrinhoDto> itens, string? codigoCupom = null, decimal? valorFrete = null);
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

        public async Task<(bool Sucesso, string Mensagem, int VendaId)> FinalizarVendaAsync(
            int codCliente, 
            List<ItemCarrinhoDto> itens, 
            string? codigoCupom = null, 
            decimal? valorFrete = null)
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

            // Validacao previa de cupom se informado
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

                // Registro do uso unico do cupom se informado (gerando o id_CupomUso)
                int? idCupomUso = null;
                if (!string.IsNullOrWhiteSpace(codigoCupom))
                {
                    var uso = await _cupomService.RegistrarUsoCupomAsync(codCliente, codigoCupom, venda.IdVenda);
                    idCupomUso = uso?.IdCupomUso;
                }

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
                        ValorTotal = totalLinha,
                        IdCupomUso = idCupomUso
                    };

                    // A baixa definitiva e atomica de saldo e executada pelo trigger relacional 'trg_AtualizarEstoque_ItemVenda'
                    _context.ItemVendas.Add(itemVenda);
                }

                // Calculo do desconto por cupom
                decimal valorDesconto = 0m;
                if (!string.IsNullOrWhiteSpace(codigoCupom))
                {
                    valorDesconto = await CalcularDescontoCupomAsync(codigoCupom, subtotalTotal, codCliente);
                }

                // Regra de Frete: Grátis acima de R$ 800,00; caso contrário R$ 9,99 (ou valor informado)
                decimal freteCalculado = 0m;
                if (subtotalTotal >= 800m)
                {
                    freteCalculado = 0m;
                }
                else if (valorFrete.HasValue)
                {
                    freteCalculado = valorFrete.Value;
                }
                else
                {
                    freteCalculado = 9.99m;
                }

                decimal totalFinal = Math.Max(0m, subtotalTotal - valorDesconto + freteCalculado);

                venda.ValorFrete = freteCalculado;
                venda.ValorDesconto = valorDesconto;
                venda.ValorTotal = totalFinal;
                _context.Vendas.Update(venda);

                // Atualizacao cadastral do cliente
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

            var vendaIds = vendas.Select(v => v.IdVenda).ToList();
            var cuponsUsados = await _context.CupomUsos
                .Where(u => u.IdVenda != null && vendaIds.Contains(u.IdVenda.Value))
                .AsNoTracking()
                .ToListAsync();

            return vendas.Select(v =>
            {
                var subtotal = v.Itens.Sum(i => i.ValorTotal);
                var cupomUso = cuponsUsados.FirstOrDefault(u => u.IdVenda == v.IdVenda);
                decimal frete = v.ValorFrete;
                decimal desconto = v.ValorDesconto;
                decimal totalPago = v.ValorTotal ?? Math.Max(0m, subtotal - desconto + frete);

                return new VendaHistoricoDto
                {
                    IdVenda = v.IdVenda,
                    DataVenda = v.DataVenda,
                    Subtotal = subtotal,
                    ValorFrete = frete,
                    ValorDesconto = desconto,
                    TotalVenda = totalPago,
                    CodigoCupom = cupomUso?.CodigoCupom,
                    Itens = v.Itens.Select(i => new ItemVendaDetalheDto
                    {
                        IdItemVenda = i.IdItemVenda,
                        CodProduto = i.CodProduto,
                        NomeProduto = i.Produto?.TipoInstrumento ?? $"Instrumento #{i.CodProduto}",
                        Quantidade = i.Quantidade,
                        ValorUnitario = i.ValorUnitario,
                        ValorTotal = i.ValorTotal,
                        IdCupomUso = i.IdCupomUso
                    }).ToList()
                };
            }).ToList();
        }
    }
}
