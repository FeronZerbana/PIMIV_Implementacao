using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Data;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public interface IEstoqueService
    {
        Task<Estoque?> ObterPorProdutoAsync(int idProduto);
        Task<int> AdicionarEstoqueAsync(int idProduto, int quantidade);
    }

    public class EstoqueService : IEstoqueService
    {
        private readonly AppDbContext _context;

        public EstoqueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Estoque?> ObterPorProdutoAsync(int idProduto)
        {
            return await _context.Estoques
                .FirstOrDefaultAsync(e => e.CodProduto == idProduto);
        }

        public async Task<int> AdicionarEstoqueAsync(int idProduto, int quantidade)
        {
            if (quantidade <= 0)
            {
                throw new ArgumentException("A quantidade adicionada deve ser maior que zero.", nameof(quantidade));
            }

            var estoque = await _context.Estoques
                .FirstOrDefaultAsync(e => e.CodProduto == idProduto);

            if (estoque == null)
            {
                // Se nao existir registro de estoque, cria um novo
                estoque = new Estoque
                {
                    CodProduto = idProduto,
                    QuantidadeDisponivel = quantidade
                };
                _context.Estoques.Add(estoque);
            }
            else
            {
                estoque.QuantidadeDisponivel += quantidade;
            }

            await _context.SaveChangesAsync();
            return estoque.QuantidadeDisponivel;
        }
    }
}
