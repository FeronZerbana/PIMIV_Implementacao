using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public interface IProdutoService
    {
        Task<List<Produto>> ObterTodosAsync();
        Task<Produto?> ObterPorIdAsync(int id);
        Task<List<Produto>> ObterPorCategoriaAsync(string categoria);
        Task<Produto> CriarProdutoAsync(Produto produto, int estoqueInicial = 0);
        Task<Produto?> AtualizarProdutoAsync(int id, Produto produtoAtualizado);
        Task<bool> AtualizarImagemAsync(int id, string imagemUrl);
        Task<bool> ExcluirProdutoAsync(int id);
        Task<List<Produto>> ObterDestaquesAsync(int quantidade = 6);
        Task<List<Produto>> ObterOfertasAsync();
    }
}
