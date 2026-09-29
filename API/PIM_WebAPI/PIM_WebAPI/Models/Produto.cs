using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PIM_WebAPI.Models
{
    public enum ProductType
    {
        Camiseta = 1,
        Moletom = 2,
        Programacao = 3,
        Linux = 4,
        Tech = 5,
        Games = 6,
        Outros = 99
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
        [Column(TypeName = "decimal(8,2)")]
        public decimal Preco { get; set; }

        [StringLength(2000)]
        public string Descricao { get; set; } = string.Empty;

        [Column("ImgURL", TypeName = "nvarchar(255)")]
        [StringLength(255)]
        public string? ImgURL { get; set; }

        // Navegacoes
        [JsonIgnore]
        public Estoque? Estoque { get; set; }

        [JsonIgnore]
        public ICollection<ItemVenda> ItemVendas { get; set; } = new List<ItemVenda>();
    }
}
