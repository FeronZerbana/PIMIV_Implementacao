using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PIM_WebAPI.Models
{
    [Table("ItemVendas")]
    public class ItemVenda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int VendaID { get; set; }

        [Required]
        public int ProdutoID { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [Required]
        [Column(TypeName = "decimal(8,2)")]
        public decimal ValorUnitario { get; set; }

        [NotMapped]
        public decimal ValorTotal => Quantidade * ValorUnitario;

        // Navegacoes
        [ForeignKey(nameof(VendaID))]
        [JsonIgnore]
        public Venda? Venda { get; set; }

        [ForeignKey(nameof(ProdutoID))]
        public Produto? Produto { get; set; }
    }
}
