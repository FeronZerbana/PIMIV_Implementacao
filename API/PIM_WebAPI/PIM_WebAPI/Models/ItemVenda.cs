using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebAPI.Models
{
    [Table("ItemVendas")]
    public class ItemVenda
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int VendaID { get; set; }
        public int ProdutoID { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal()
        {
            return Quantidade * ValorUnitario;
        }

    }
}
