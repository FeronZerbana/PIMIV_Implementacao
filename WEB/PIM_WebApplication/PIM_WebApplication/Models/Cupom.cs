using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebApplication.Models
{
    [Table("CUPONS")]
    public class Cupom
    {
        [Key]
        [Column("id_Cupom")]
        public int IdCupom { get; set; }

        [Required(ErrorMessage = "O código do cupom é obrigatório.")]
        [Column("VALOR")]
        [StringLength(50)]
        public string Valor { get; set; } = string.Empty;

        [Required(ErrorMessage = "O percentual de desconto é obrigatório.")]
        [Column("DESCONTO")]
        [Range(1, 99, ErrorMessage = "O desconto deve ser entre 1% e 99%.")]
        public int Desconto { get; set; }

        [Column("Ativo")]
        public bool Ativo { get; set; } = true;

        [Column("DataCriacao")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;
    }
}
