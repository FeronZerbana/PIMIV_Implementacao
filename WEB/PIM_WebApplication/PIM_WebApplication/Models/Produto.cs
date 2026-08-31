using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebApplication.Models
{
    [Table("PRODUTOS")]
    public class Produto
    {
        [Key]
        [Column("id_Produto")]
        public int IdProduto { get; set; }

        [Required]
        [Column("Tipo_Roupa")]
        [StringLength(150)]
        public string TipoRoupa { get; set; } = string.Empty;

        [Required]
        [Column("Preco", TypeName = "decimal(8,2)")]
        public decimal Preco { get; set; }

        [Column("Tamanho")]
        [StringLength(10)]
        public string Tamanho { get; set; } = string.Empty;

        public Estoque? Estoque { get; set; }
    }
}
