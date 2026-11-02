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
        [Column("Tipo_Instrumento")]
        [StringLength(150)]
        public string TipoInstrumento { get; set; } = string.Empty;

        // Alias para compatibilidade legada
        [NotMapped]
        public string TipoRoupa
        {
            get => TipoInstrumento;
            set => TipoInstrumento = value;
        }

        [NotMapped]
        public string Nome
        {
            get => TipoInstrumento;
            set => TipoInstrumento = value;
        }

        [Required]
        [Column("Preco", TypeName = "decimal(8,2)")]
        public decimal Preco { get; set; }

        [Column("Modelo")]
        [StringLength(100)]
        public string Modelo { get; set; } = string.Empty;

        // Alias para compatibilidade legada
        [NotMapped]
        public string Tamanho
        {
            get => Modelo;
            set => Modelo = value;
        }

        [NotMapped]
        public string TipoModelo => TipoInstrumento;

        [Column("Descricao")]
        [StringLength(1000)]
        public string? Descricao { get; set; }

        [Column("Ativo")]
        public bool Ativo { get; set; } = true;

        [Column("Imagem_Url")]
        [StringLength(255)]
        public string? ImagemUrl { get; set; }

        [NotMapped]
        public string? ImgURL
        {
            get => ImagemUrl;
            set => ImagemUrl = value;
        }

        public Estoque? Estoque { get; set; }
    }
}
