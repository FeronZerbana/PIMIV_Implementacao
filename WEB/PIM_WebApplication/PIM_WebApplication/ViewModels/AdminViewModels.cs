using System.ComponentModel.DataAnnotations;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.ViewModels
{
    public class AdminViewModel
    {
        public List<Produto> ProdutosCadastrados { get; set; } = new();
        public NovoProdutoViewModel NovoProduto { get; set; } = new();
        public AdicionarEstoqueViewModel AdicionarEstoque { get; set; } = new();
    }

    public class NovoProdutoViewModel
    {
        [Required(ErrorMessage = "O nome do produto e obrigatorio.")]
        [Display(Name = "NOME DO PRODUTO")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preco e obrigatorio.")]
        [Range(0.01, 99999.99, ErrorMessage = "Informe um preco valido maior que zero.")]
        [Display(Name = "PRECO (R$)")]
        public decimal Preco { get; set; }

        [Display(Name = "TAMANHO")]
        public string? Tamanho { get; set; }

        [Display(Name = "MODELO")]
        public string Modelo { get; set; } = string.Empty;

        [Display(Name = "ESTOQUE INICIAL")]
        [Range(0, 10000, ErrorMessage = "A quantidade deve ser positiva.")]
        public int EstoqueInicial { get; set; } = 0;
    }

    public class AdicionarEstoqueViewModel
    {
        [Required(ErrorMessage = "Selecione um produto.")]
        [Display(Name = "SELECIONE UM PRODUTO")]
        public int IdProduto { get; set; }

        [Required(ErrorMessage = "Informe a quantidade.")]
        [Range(1, 10000, ErrorMessage = "A quantidade deve ser no minimo 1.")]
        [Display(Name = "QUANTIDADE A ADICIONAR")]
        public int Quantidade { get; set; }
    }
}
