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
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var cliente = await _usuarioService.AutenticarAsync(request.Email, request.Senha);
            if (cliente == null)
            {
                return Unauthorized(new { mensagem = "Email ou senha inválidos!" });
            }

            return Ok(new
            {
                id = cliente.IdCliente,
                nome = cliente.Nome,
                isAdmin = cliente.IsAdmin
            });
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> Registrar([FromBody] Cliente novoCliente)
        {
            var (sucesso, mensagem, clienteCriado) = await _usuarioService.RegistrarAsync(
                novoCliente.Nome,
                novoCliente.Email,
                novoCliente.Senha,
                novoCliente.Telefone);

            if (!sucesso)
            {
                return BadRequest(new { mensagem });
            }

            return Ok(new { mensagem = "Usuário criado com sucesso!" });
        }
    }
}
