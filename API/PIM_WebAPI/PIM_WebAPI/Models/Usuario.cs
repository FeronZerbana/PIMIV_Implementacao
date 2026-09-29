using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PIM_WebAPI.Models
{

    [Table("Usuarios")]
    [Index(nameof(Email), IsUnique = true)]
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = string.Empty;
        [Required]
        [StringLength(250)]
        public string Email { get; set; } = string.Empty;
        [Required]
        [StringLength(300)]
        public string Password_Hash { get; set; } = string.Empty;
        [Required]
        public DateTime Data_Criacao { get; set; }
        [Required]
        public bool IsAdmin { get; set; } = false;



    }
}
