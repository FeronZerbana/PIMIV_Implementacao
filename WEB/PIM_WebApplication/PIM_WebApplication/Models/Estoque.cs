using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PIM_WebApplication.Models
{
    [Table("Estoque")]
    public class Estoque
    {
        [Key]
        [Column("id_Estoque")]
        public int IdEstoque { get; set; }

        [Column("Cod_do_Produto")]
        public int CodProduto { get; set; }

        [Column("Quantidade_Disponivel")]
        public int QuantidadeDisponivel { get; set; }

        [JsonIgnore]
        [ForeignKey("CodProduto")]
        public Produto? Produto { get; set; }
    }
}
