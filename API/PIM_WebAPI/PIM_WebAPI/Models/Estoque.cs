using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PIM_WebAPI.Models
{
    [Table("Estoques")]
    [Index(nameof(ProdutoID), IsUnique = true)]
    public class Estoque
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        [Required]
        public int ProdutoID { get; set; }

        [Required]
        public int Quantidade { get; set; }

        [ForeignKey(nameof(ProdutoID))]
        [JsonIgnore]
        public Produto? Produto { get; set; }
    }
}
