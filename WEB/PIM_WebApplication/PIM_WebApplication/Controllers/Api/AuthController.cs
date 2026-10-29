using Microsoft.AspNetCore.Mvc;
using PIM_WebApplication.Models;
using PIM_WebApplication.Services;

namespace PIM_WebApplication.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public AuthController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var cliente = await _usuarioService.AutenticarAsync(request.Email, request.Senha);
            if (cliente == null)
            {
                return Unauthorized(new { mensagem = "E-mail ou senha invalidos." });
            }

            return Ok(new LoginResponseDto
            {
                Id = cliente.IdCliente,
                Nome = cliente.Nome,
                Email = cliente.Email,
                IsAdmin = cliente.IsAdmin,
                Mensagem = "Autenticacao realizada com sucesso."
            });
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] Cliente novoCliente)
        {
            if (string.IsNullOrWhiteSpace(novoCliente.Nome) || 
                string.IsNullOrWhiteSpace(novoCliente.Email) || 
                string.IsNullOrWhiteSpace(novoCliente.Senha))
            {
                return BadRequest(new { mensagem = "Nome, e-mail e senha sao campos obrigatorios." });
            }

            var (sucesso, mensagem, clienteCriado) = await _usuarioService.RegistrarAsync(
                novoCliente.Nome,
                novoCliente.Email,
                novoCliente.Senha,
                novoCliente.Telefone);

            if (!sucesso)
            {
                return BadRequest(new { mensagem });
            }

            return Ok(new
            {
                mensagem = "Usuario cadastrado com sucesso.",
                id = clienteCriado?.IdCliente
            });
        }
    }
}
