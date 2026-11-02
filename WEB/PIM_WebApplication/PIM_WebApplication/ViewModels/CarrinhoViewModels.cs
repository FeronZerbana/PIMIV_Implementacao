namespace PIM_WebApplication.ViewModels
{
    public class CarrinhoViewModel
    {
        public List<ItemCarrinhoViewModel> Itens { get; set; } = new();
        public decimal Subtotal => Itens.Sum(i => i.Subtotal);
        public int TotalQuantidade => Itens.Sum(i => i.Quantidade);
        
        public decimal DescontoCupom { get; set; } = 0m;

        // Regra RN-PREC-04: Desconto maximo permitido de 50% do subtotal
        public decimal Desconto => Math.Min(Math.Round(Subtotal * 0.50m, 2), DescontoCupom);

        // Regra de Frete: Grátis para compras acima de R$ 800,00; taxa fixa de R$ 9,99 para valores inferiores
        public string? CepDestino { get; set; }
        public bool FreteCalculado { get; set; }
        public bool FreteGratisElegivel => Subtotal >= 800m && Subtotal > 0m;
        public decimal FaltaParaFreteGratis => Subtotal < 800m ? 800m - Subtotal : 0m;

        public decimal Frete
        {
            get
            {
                if (Subtotal == 0m) return 0m;
                if (Subtotal >= 800m) return 0m;
                if (FreteCalculado || !string.IsNullOrWhiteSpace(CepDestino)) return 9.99m;
                return 0m;
            }
        }

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
        public string ImagemUrl { get; set; } = string.Empty;
        public int Quantidade { get; set; } = 1;
        public int EstoqueDisponivel { get; set; } = 0;
        public decimal Subtotal => Preco * Quantidade;
    }
}
