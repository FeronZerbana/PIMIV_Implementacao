using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.ViewModels
{
    public class AdminViewModel
    {
        public List<Produto> ProdutosCadastrados { get; set; } = new();
        public NovoProdutoViewModel NovoProduto { get; set; } = new();
        public AdicionarEstoqueViewModel AdicionarEstoque { get; set; } = new();
        public List<Cupom> CuponsCadastrados { get; set; } = new();
        public NovoCupomViewModel NovoCupom { get; set; } = new();
    }

    public class NovoProdutoViewModel
    {
        [Required(ErrorMessage = "A categoria do instrumento e obrigatoria.")]
        [Display(Name = "CATEGORIA DO INSTRUMENTO")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preco e obrigatorio.")]
        [Range(0.01, 99999.99, ErrorMessage = "Informe um preco valido maior que zero.")]
        [Display(Name = "PRECO (R$)")]
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "O modelo / descricao e obrigatorio.")]
        [Display(Name = "MODELO / ESPECIFICACAO")]
        public string Modelo { get; set; } = string.Empty;

        [Display(Name = "TAMANHO")]
        public string? Tamanho { get; set; }

        [Display(Name = "FOTO DO INSTRUMENTO (ARQUIVO)")]
        public IFormFile? ImagemArquivo { get; set; }

        [Display(Name = "URL DA IMAGEM")]
        public string? ImagemUrl { get; set; }

        [Display(Name = "ESTOQUE INICIAL")]
        [Range(0, 10000, ErrorMessage = "A quantidade deve ser positiva.")]
        public int EstoqueInicial { get; set; } = 0;
    }

    public class AdicionarEstoqueViewModel
    {
        [Required(ErrorMessage = "Selecione um produto.")]
        [Display(Name = "SELECIONE UM INSTRUMENTO")]
        public int IdProduto { get; set; }

        [Required(ErrorMessage = "Informe a quantidade.")]
        [Range(1, 10000, ErrorMessage = "A quantidade deve ser no minimo 1.")]
        [Display(Name = "QUANTIDADE A ADICIONAR")]
        public int Quantidade { get; set; }
    }

    public class NovoCupomViewModel
    {
        [Required(ErrorMessage = "O codigo do cupom e obrigatorio.")]
        [StringLength(50, ErrorMessage = "O codigo do cupom deve ter no maximo 50 caracteres.")]
        [Display(Name = "CODIGO DO CUPOM")]
        public string Valor { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a porcentagem de desconto.")]
        [Range(1, 99, ErrorMessage = "O desconto deve ser entre 1% e 99%.")]
        [Display(Name = "DESCONTO (%)")]
        public int Desconto { get; set; }
    }
}
