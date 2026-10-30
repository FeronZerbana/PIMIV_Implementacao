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
                .OrderByDescending(p => p.IdProduto)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Produto?> ObterPorIdAsync(int id)
        {
            return await _context.Produtos
                .Include(p => p.Estoque)
                .FirstOrDefaultAsync(p => p.IdProduto == id);
        }

        public async Task<List<Produto>> ObterPorCategoriaAsync(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
            {
                return await ObterTodosAsync();
            }

            var cat = categoria.Trim().ToLowerInvariant();
            var query = _context.Produtos
                .Include(p => p.Estoque)
                .AsNoTracking()
                .AsQueryable();

            if (cat.Contains("guitar"))
            {
                query = query.Where(p => p.TipoInstrumento.ToLower().Contains("guitar"));
            }
            else if (cat.Contains("viol") || cat.Contains("violoes"))
            {
                query = query.Where(p => p.TipoInstrumento.ToLower().Contains("viol"));
            }
            else if (cat.Contains("tecl") || cat.Contains("pian") || cat.Contains("sintet"))
            {
                query = query.Where(p => p.TipoInstrumento.ToLower().Contains("tecl") 
                                      || p.TipoInstrumento.ToLower().Contains("pian") 
                                      || p.TipoInstrumento.ToLower().Contains("sintet")
                                      || p.TipoInstrumento.ToLower().Contains("midi"));
            }
            else if (cat.Contains("bater") || cat.Contains("percuss") || cat.Contains("cajon"))
            {
                query = query.Where(p => p.TipoInstrumento.ToLower().Contains("bater") 
                                      || p.TipoInstrumento.ToLower().Contains("percuss")
                                      || p.TipoInstrumento.ToLower().Contains("cajon"));
            }
            else if (cat.Contains("acessor") || cat.Contains("cabo") || cat.Contains("palhet") || cat.Contains("afinad") || cat.Contains("correia") || cat.Contains("suporte") || cat.Contains("corda") || cat.Contains("bag") || cat.Contains("case"))
            {
                query = query.Where(p => p.TipoInstrumento.ToLower().Contains("acessor") 
                                      || p.TipoInstrumento.ToLower().Contains("cabo") 
                                      || p.TipoInstrumento.ToLower().Contains("palhet") 
                                      || p.TipoInstrumento.ToLower().Contains("afinad") 
                                      || p.TipoInstrumento.ToLower().Contains("correia") 
                                      || p.TipoInstrumento.ToLower().Contains("suporte") 
                                      || p.TipoInstrumento.ToLower().Contains("corda") 
                                      || p.TipoInstrumento.ToLower().Contains("bag") 
                                      || p.TipoInstrumento.ToLower().Contains("case"));
            }
            else // Outros instrumentos (sopros, orquestrais, etc.)
            {
                query = query.Where(p => !p.TipoInstrumento.ToLower().Contains("guitar") 
                                      && !p.TipoInstrumento.ToLower().Contains("viol")
                                      && !p.TipoInstrumento.ToLower().Contains("tecl")
                                      && !p.TipoInstrumento.ToLower().Contains("pian")
                                      && !p.TipoInstrumento.ToLower().Contains("bater")
                                      && !p.TipoInstrumento.ToLower().Contains("cabo")
                                      && !p.TipoInstrumento.ToLower().Contains("palhet"));
            }

            return await query.OrderByDescending(p => p.IdProduto).ToListAsync();
        }

        public async Task<Produto> CriarProdutoAsync(Produto produto, int estoqueInicial = 0)
        {
            try
            {
                // Insercao atomica via Stored Procedure sp_CadastrarProdutoComEstoque
                var idParam = new Microsoft.Data.SqlClient.SqlParameter
                {
                    ParameterName = "@IdProdutoGerado",
                    SqlDbType = System.Data.SqlDbType.Int,
                    Direction = System.Data.ParameterDirection.Output
                };

                var tipoParam = new Microsoft.Data.SqlClient.SqlParameter("@TipoInstrumento", produto.TipoInstrumento ?? "Outros");
                var precoParam = new Microsoft.Data.SqlClient.SqlParameter("@Preco", produto.Preco);
                var modeloParam = new Microsoft.Data.SqlClient.SqlParameter("@Modelo", produto.Modelo ?? string.Empty);
                var imgParam = new Microsoft.Data.SqlClient.SqlParameter("@ImagemUrl", (object?)produto.ImagemUrl ?? DBNull.Value);
                var qtdParam = new Microsoft.Data.SqlClient.SqlParameter("@QuantidadeEstoque", estoqueInicial >= 0 ? estoqueInicial : 0);

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_CadastrarProdutoComEstoque @TipoInstrumento, @Preco, @Modelo, @ImagemUrl, @QuantidadeEstoque, @IdProdutoGerado OUTPUT",
                    tipoParam, precoParam, modeloParam, imgParam, qtdParam, idParam);

                if (idParam.Value != null && idParam.Value != DBNull.Value)
                {
                    produto.IdProduto = (int)idParam.Value;
                }

                var estoque = new Estoque
                {
                    CodProduto = produto.IdProduto,
                    QuantidadeDisponivel = estoqueInicial >= 0 ? estoqueInicial : 0
                };

                produto.Estoque = estoque;
                return produto;
            }
            catch
            {
                // Fallback resiliente via EF Core tradicional
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
        }

        public async Task<Produto?> AtualizarProdutoAsync(int id, Produto produtoAtualizado)
        {
            var produtoExistente = await _context.Produtos
                .Include(p => p.Estoque)
                .FirstOrDefaultAsync(p => p.IdProduto == id);

            if (produtoExistente == null)
            {
                return null;
            }

            produtoExistente.TipoInstrumento = produtoAtualizado.TipoInstrumento;
            produtoExistente.Preco = produtoAtualizado.Preco;
            produtoExistente.Modelo = produtoAtualizado.Modelo;

            if (!string.IsNullOrWhiteSpace(produtoAtualizado.ImagemUrl))
            {
                produtoExistente.ImagemUrl = produtoAtualizado.ImagemUrl;
            }

            await _context.SaveChangesAsync();
            return produtoExistente;
        }

        public async Task<bool> AtualizarImagemAsync(int id, string imagemUrl)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto == null)
            {
                return false;
            }

            produto.ImagemUrl = imagemUrl;
            await _context.SaveChangesAsync();
            return true;
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

            // Remocao de imagem física em disco se for upload
            if (!string.IsNullOrWhiteSpace(produto.ImagemUrl) && produto.ImagemUrl.StartsWith("/uploads/"))
            {
                try
                {
                    var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var cleanPath = produto.ImagemUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
                    var fullPath = Path.Combine(basePath, cleanPath);
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
                catch
                {
                    // Nao impede a remocao do registro caso haja lock no arquivo
                }
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
