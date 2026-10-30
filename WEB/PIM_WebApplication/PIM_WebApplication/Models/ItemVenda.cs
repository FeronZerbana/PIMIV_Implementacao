using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebApplication.Models
{
    [Table("Item_Vendas")]
    public class ItemVenda
    {
        [Key]
        [Column("id_Item_Venda")]
        public int IdItemVenda { get; set; }

        [Column("Cod_da_Venda")]
        public int CodVenda { get; set; }

        [Column("Cod_do_Produto")]
        public int CodProduto { get; set; }

        public int Quantidade { get; set; }

        [Column("Valor_Unitario", TypeName = "decimal(8,2)")]
        public decimal ValorUnitario { get; set; }

        [Column("Valor_Total", TypeName = "decimal(8,2)")]
        public decimal ValorTotal { get; set; }

        [Column("id_CupomUso")]
        public int? IdCupomUso { get; set; }

        [ForeignKey("CodVenda")]
        public Venda? Venda { get; set; }

        [ForeignKey("CodProduto")]
        public Produto? Produto { get; set; }

        [ForeignKey("IdCupomUso")]
        public CupomUso? CupomUso { get; set; }
    }
}
