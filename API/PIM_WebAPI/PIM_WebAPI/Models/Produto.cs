using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebAPI.Models
{

    public enum ProductType
    {

    }

    [Table("Produtos")]
    public class Produto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        public ProductType Tipo { get; set; }
        [Required]
        public decimal Preco { get; set; }
        [StringLength(2000)]
        public string Descricao { get; set; } = "";
        [Column(TypeName = "varbinary(max)")]
        [StringLength(255)]
        public string? ImgURL { get; set; }

    }
}
