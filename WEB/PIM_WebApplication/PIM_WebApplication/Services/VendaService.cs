using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Data;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public interface IVendaService
    {
        Task<(bool Sucesso, string Mensagem, int VendaId)> FinalizarVendaAsync(int codCliente, List<ItemCarrinhoDto> itens, string? codigoCupom = null);
        Task<decimal> CalcularDescontoCupomAsync(string codigoCupom, decimal subtotal);
    }

    public class VendaService : IVendaService
    {
        private readonly AppDbContext _context;

        public VendaService(AppDbContext context)
        {
            _context = context;
        }

        public Task<decimal> CalcularDescontoCupomAsync(string codigoCupom, decimal subtotal)
        {
            if (string.IsNullOrWhiteSpace(codigoCupom))
            {
                return Task.FromResult(0m);
            }

            var cupomNormalizado = codigoCupom.Trim().ToUpperInvariant();
            decimal desconto = cupomNormalizado switch
            {
                "FRETE22" => 20m,
                "DEV10" => 10m,
                "SHOPNOW15" => 15m,
                _ => 0m
            };

            // Regra RN-PREC-04: Desconto maximo permitido de 50% do subtotal
            decimal maxDesconto = subtotal * 0.50m;
            if (desconto > maxDesconto)
            {
                desconto = maxDesconto;
            }

            return Task.FromResult(desconto);
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

                foreach (var item in itens)
                {
                    var produto = await _context.Produtos.FindAsync(item.CodProduto);
                    if (produto == null)
                    {
                        produto = new Produto
                        {
                            TipoRoupa = !string.IsNullOrWhiteSpace(item.Nome) ? item.Nome : $"Instrumento #{item.CodProduto}",
                            Preco = item.PrecoUnitario > 0 ? item.PrecoUnitario : 999.00m,
                            Tamanho = "Padrao"
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
                        throw new InvalidOperationException($"Estoque insuficiente para o produto '{produto.TipoRoupa}'. Disponivel: {estoque.QuantidadeDisponivel}.");
                    }

                    var itemVenda = new ItemVenda
                    {
                        CodVenda = venda.IdVenda,
                        CodProduto = item.CodProduto,
                        Quantidade = item.Quantidade,
                        ValorUnitario = produto.Preco,
                        ValorTotal = Math.Round(produto.Preco * item.Quantidade, 2, MidpointRounding.AwayFromZero)
                    };

                    estoque.QuantidadeDisponivel -= item.Quantidade;
                    _context.ItemVendas.Add(itemVenda);
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
    }
}
