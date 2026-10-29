using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebApplication.Models
{
    [Table("CLIENTES")]
    public class Cliente
    {
        [Key]
        [Column("id_cliente")]
        public int IdCliente { get; set; }

        [Required]
        [Column("Nome")]
        [StringLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [Column("Email")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Column("Senha")]
        [StringLength(255)]
        public string Senha { get; set; } = string.Empty;

        [Column("Telefone")]
        [StringLength(30)]
        public string? Telefone { get; set; }

        [Column("isAdmin")]
        public bool IsAdmin { get; set; } = false;

        [Column("FirstTime")]
        public bool FirstTime { get; set; } = true;
    }
}
