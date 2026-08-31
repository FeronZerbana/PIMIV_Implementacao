using PIM_WebApplication.Models;

namespace PIM_WebApplication.ViewModels
{
    public class HomeViewModel
    {
        public List<Produto> Destaques { get; set; } = new();
        public List<CategoriaItemViewModel> Categorias { get; set; } = new();
    }

    public class CategoriaItemViewModel
    {
        public string Nome { get; set; } = string.Empty;
        public string Icone { get; set; } = string.Empty;
    }

    public class OfertasViewModel
    {
        public List<Produto> Produtos { get; set; } = new();
        public decimal PercentualDesconto { get; set; } = 50m;
    }
}
