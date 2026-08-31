using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public interface IProdutoService
    {
        Task<List<Produto>> ObterTodosAsync();
        Task<Produto?> ObterPorIdAsync(int id);
        Task<Produto> CriarProdutoAsync(Produto produto, int estoqueInicial = 0);
        Task<bool> ExcluirProdutoAsync(int id);
        Task<List<Produto>> ObterDestaquesAsync(int quantidade = 6);
        Task<List<Produto>> ObterOfertasAsync();
    }
}
