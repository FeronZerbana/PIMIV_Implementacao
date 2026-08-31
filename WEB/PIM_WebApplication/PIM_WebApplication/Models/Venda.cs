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

        public List<ItemVenda> Itens { get; set; } = new();
    }
}
