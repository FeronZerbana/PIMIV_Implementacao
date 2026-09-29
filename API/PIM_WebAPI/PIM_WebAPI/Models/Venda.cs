using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebAPI.Models
{
    [Table("Vendas")]
    public class Venda
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int UsuarioID { get; set; }

        [Required]
        public DateTime Data_Venda { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UsuarioID))]
        public Usuario? Usuario { get; set; }

        public ICollection<ItemVenda> Itens { get; set; } = new List<ItemVenda>();
    }
}
