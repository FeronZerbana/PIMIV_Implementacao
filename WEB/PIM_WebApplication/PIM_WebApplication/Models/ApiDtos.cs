namespace PIM_WebApplication.Models
{
    public class CarrinhoRequest
    {
        public int CodCliente { get; set; }
        public List<ItemCarrinhoDto> Itens { get; set; } = new();
        public string? Cupom { get; set; }
    }

    public class ItemCarrinhoDto
    {
        public int CodProduto { get; set; }
        public int Quantidade { get; set; }
        public string? Nome { get; set; }
        public decimal PrecoUnitario { get; set; }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}
