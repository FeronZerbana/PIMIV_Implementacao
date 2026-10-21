namespace PIM_WebApplication.ViewModels
{
    public class CarrinhoViewModel
    {
        public List<ItemCarrinhoViewModel> Itens { get; set; } = new();
        public decimal Subtotal => Itens.Sum(i => i.Subtotal);
        public int TotalQuantidade => Itens.Sum(i => i.Quantidade);
        public decimal Desconto { get; set; } = 0m;
        public decimal Frete => (Subtotal >= 99m || Subtotal == 0m) ? 0m : 15m;
        public decimal Total => Math.Max(0m, Subtotal - Desconto + Frete);
        public string? CupomAplicado { get; set; }
        public string? MensagemCupom { get; set; }
        public bool CupomValido { get; set; }
        public string ValorParcela => Total > 0 ? (Total / 4).ToString("F2") : "0,00";
    }

    public class ItemCarrinhoViewModel
    {
        public int IdProduto { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public string Tamanho { get; set; } = string.Empty;
        public string Modelo { get => Tamanho; set => Tamanho = value; }
        public string Emoji { get; set; } = string.Empty;
        public int Quantidade { get; set; } = 1;
        public int EstoqueDisponivel { get; set; } = 0;
        public decimal Subtotal => Preco * Quantidade;
    }
}
