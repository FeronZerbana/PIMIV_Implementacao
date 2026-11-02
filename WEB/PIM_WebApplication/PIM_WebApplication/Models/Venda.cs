using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebApplication.Models
{
    [Table("VENDAS")]
    public class Venda
    {
        [Key]
        [Column("id_Vendas")]
        public int IdVenda { get; set; }

        [Column("Cod_do_Cliente")]
        public int CodCliente { get; set; }

        [Column("Data_Venda")]
        public DateTime DataVenda { get; set; } = DateTime.Now;

        [Column("Valor_Frete", TypeName = "decimal(8,2)")]
        public decimal ValorFrete { get; set; } = 0m;

        [Column("Valor_Desconto", TypeName = "decimal(8,2)")]
        public decimal ValorDesconto { get; set; } = 0m;

        [Column("Valor_Total", TypeName = "decimal(8,2)")]
        public decimal? ValorTotal { get; set; }

        public List<ItemVenda> Itens { get; set; } = new();
    }
}
