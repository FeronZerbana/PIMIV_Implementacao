using System.ComponentModel.DataAnnotations;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "O e-mail e obrigatorio.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
        [Display(Name = "E-MAIL")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha e obrigatoria.")]
        [DataType(DataType.Password)]
        [Display(Name = "SENHA")]
        public string Senha { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }

    public class RegistroViewModel
    {
        [Required(ErrorMessage = "O nome completo e obrigatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        [Display(Name = "NOME COMPLETO")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail e obrigatorio.")]
        [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
        [Display(Name = "E-MAIL")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "TELEFONE")]
        public string? Telefone { get; set; }

        [Required(ErrorMessage = "A senha e obrigatoria.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter no minimo 6 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "SENHA")]
        public string Senha { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirme sua senha.")]
        [DataType(DataType.Password)]
        [Compare("Senha", ErrorMessage = "As senhas nao coincidem.")]
        [Display(Name = "CONFIRMAR SENHA")]
        public string ConfirmarSenha { get; set; } = string.Empty;
    }

    public class PerfilUsuarioViewModel
    {
        public int IdCliente { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public bool IsAdmin { get; set; }
        public List<VendaHistoricoDto> Pedidos { get; set; } = new();
    }
}
