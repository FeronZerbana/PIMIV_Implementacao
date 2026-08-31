using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Data;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public class ProdutoService : IProdutoService
    {
        private readonly AppDbContext _context;

        public ProdutoService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Produto>> ObterTodosAsync()
        {
            return await _context.Produtos
                .Include(p => p.Estoque)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Produto?> ObterPorIdAsync(int id)
        {
            return await _context.Produtos
                .Include(p => p.Estoque)
                .FirstOrDefaultAsync(p => p.IdProduto == id);
        }

        public async Task<Produto> CriarProdutoAsync(Produto produto, int estoqueInicial = 0)
        {
            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            var estoque = new Estoque
            {
                CodProduto = produto.IdProduto,
                QuantidadeDisponivel = estoqueInicial >= 0 ? estoqueInicial : 0
            };

            _context.Estoques.Add(estoque);
            await _context.SaveChangesAsync();

            produto.Estoque = estoque;
            return produto;
        }

        public async Task<bool> ExcluirProdutoAsync(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Estoque)
                .FirstOrDefaultAsync(p => p.IdProduto == id);

            if (produto == null)
            {
                return false;
            }

            // Verifica se o produto tem historico de vendas associado
            bool possuiVendas = await _context.ItemVendas.AnyAsync(i => i.CodProduto == id);
            if (possuiVendas)
            {
                throw new InvalidOperationException("Nao e possivel excluir um produto que possui historico de vendas.");
            }

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Produto>> ObterDestaquesAsync(int quantidade = 6)
        {
            return await _context.Produtos
                .Include(p => p.Estoque)
                .AsNoTracking()
                .Take(quantidade)
                .ToListAsync();
        }

        public async Task<List<Produto>> ObterOfertasAsync()
        {
            return await _context.Produtos
                .Include(p => p.Estoque)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
