using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Data;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public interface IUsuarioService
    {
        Task<Cliente?> AutenticarAsync(string email, string senha);
        Task<(bool Sucesso, string Mensagem, Cliente? Cliente)> RegistrarAsync(string nome, string email, string senha, string? telefone);
        Task<Cliente?> ObterPorIdAsync(int idCliente);
    }

    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;

        public UsuarioService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> AutenticarAsync(string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            {
                return null;
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Email == email.Trim().ToLowerInvariant());

            if (cliente == null)
            {
                return null;
            }

            bool senhaValida = BCrypt.Net.BCrypt.Verify(senha, cliente.Senha);
            return senhaValida ? cliente : null;
        }

        public async Task<(bool Sucesso, string Mensagem, Cliente? Cliente)> RegistrarAsync(string nome, string email, string senha, string? telefone)
        {
            var emailNormalizado = email.Trim().ToLowerInvariant();

            bool jaExiste = await _context.Clientes.AnyAsync(c => c.Email == emailNormalizado);
            if (jaExiste)
            {
                return (false, "Este e-mail ja esta cadastrado no sistema.", null);
            }

            var novoCliente = new Cliente
            {
                Nome = nome.Trim(),
                Email = emailNormalizado,
                Senha = BCrypt.Net.BCrypt.HashPassword(senha),
                Telefone = telefone?.Trim(),
                IsAdmin = false
            };

            _context.Clientes.Add(novoCliente);
            await _context.SaveChangesAsync();

            return (true, "Conta criada com sucesso!", novoCliente);
        }

        public async Task<Cliente?> ObterPorIdAsync(int idCliente)
        {
            return await _context.Clientes.FindAsync(idCliente);
        }
    }
}
