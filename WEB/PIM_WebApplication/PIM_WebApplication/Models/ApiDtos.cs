using System.ComponentModel.DataAnnotations;

namespace PIM_WebApplication.Models
{
    // --- Autenticacao e Contas ---
    public class LoginRequest
    {
        [Required(ErrorMessage = "O e-mail e obrigatorio.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail invalido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha e obrigatoria.")]
        public string Senha { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }

    // --- Produtos e Catalogo ---
    public class ProdutoDto
    {
        public int Id { get; set; }
        public string TipoInstrumento { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string Modelo { get; set; } = string.Empty;
        public string? ImagemUrl { get; set; }
        public int QuantidadeEstoque { get; set; }
    }

    public class CriarProdutoDto
    {
        [Required(ErrorMessage = "O nome ou tipo do instrumento e obrigatorio.")]
        [StringLength(150, ErrorMessage = "O tamanho maximo e de 150 caracteres.")]
        public string TipoInstrumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preco e obrigatorio.")]
        [Range(0.01, 999999.99, ErrorMessage = "O preco deve ser maior que zero.")]
        public decimal Preco { get; set; }

        [StringLength(100, ErrorMessage = "O modelo deve ter no maximo 100 caracteres.")]
        public string Modelo { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ImagemUrl { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "A quantidade de estoque inicial nao pode ser negativa.")]
        public int EstoqueInicial { get; set; } = 0;
    }

    public class AtualizarProdutoDto
    {
        [Required(ErrorMessage = "O nome ou tipo do instrumento e obrigatorio.")]
        [StringLength(150)]
        public string TipoInstrumento { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preco e obrigatorio.")]
        [Range(0.01, 999999.99, ErrorMessage = "O preco deve ser maior que zero.")]
        public decimal Preco { get; set; }

        [StringLength(100)]
        public string Modelo { get; set; } = string.Empty;

        [StringLength(255)]
        public string? ImagemUrl { get; set; }
    }

    public class RespostaUploadImagemDto
    {
        public string Mensagem { get; set; } = string.Empty;
        public string ImgURL { get; set; } = string.Empty;
    }

    // --- Carrinho e Vendas ---
    public class CarrinhoRequest
    {
        public int CodCliente { get; set; }
        public List<ItemCarrinhoDto> Itens { get; set; } = new();
        public string? Cupom { get; set; }
        public decimal? ValorFrete { get; set; }
    }

    public class ItemCarrinhoDto
    {
        public int CodProduto { get; set; }
        public int Quantidade { get; set; }
        public string? Nome { get; set; }
        public decimal PrecoUnitario { get; set; }
    }

    public class VendaHistoricoDto
    {
        public int IdVenda { get; set; }
        public DateTime DataVenda { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ValorFrete { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal TotalVenda { get; set; }
        public string? CodigoCupom { get; set; }
        public List<ItemVendaDetalheDto> Itens { get; set; } = new();
    }

    public class ItemVendaDetalheDto
    {
        public int IdItemVenda { get; set; }
        public int CodProduto { get; set; }
        public string NomeProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
        public int? IdCupomUso { get; set; }
    }
}
