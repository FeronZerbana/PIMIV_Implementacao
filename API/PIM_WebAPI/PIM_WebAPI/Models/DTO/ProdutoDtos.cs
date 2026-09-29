using System.ComponentModel.DataAnnotations;

namespace PIM_WebAPI.Models.DTO
{
    public class ProdutoDto
    {
        public int ID { get; set; }
        public ProductType Tipo { get; set; }
        public decimal Preco { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public string? ImgURL { get; set; }
        public int QuantidadeEstoque { get; set; }
    }

    public class CriarProdutoDto
    {
        [Required]
        public ProductType Tipo { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "O preco deve ser maior que zero.")]
        public decimal Preco { get; set; }

        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade de estoque inicial nao pode ser negativa.")]
        public int EstoqueInicial { get; set; } = 0;
    }

    public class AtualizarProdutoDto
    {
        [Required]
        public ProductType Tipo { get; set; }

        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "O preco deve ser maior que zero.")]
        public decimal Preco { get; set; }

        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;
    }

    public class RespostaUploadImagemDto
    {
        public string Mensagem { get; set; } = string.Empty;
        public string ImgURL { get; set; } = string.Empty;
    }
}
