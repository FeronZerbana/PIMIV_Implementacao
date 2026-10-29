using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebApplication.Models
{
    [Table("CUPOM_USOS")]
    public class CupomUso
    {
        [Key]
        [Column("id_CupomUso")]
        public int IdCupomUso { get; set; }

        [Required]
        [Column("Cod_do_Cliente")]
        public int CodCliente { get; set; }

        [Required]
        [Column("Codigo_Cupom")]
        [StringLength(50)]
        public string CodigoCupom { get; set; } = string.Empty;

        [Column("id_Venda")]
        public int? IdVenda { get; set; }

        [Column("Data_Uso")]
        public DateTime DataUso { get; set; } = DateTime.Now;

        public Cliente? Cliente { get; set; }
    }
}
