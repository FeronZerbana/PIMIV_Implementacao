using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Data;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public class CupomService : ICupomService
    {
        private readonly AppDbContext _context;

        public CupomService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Cupom>> ObterTodosAsync()
        {
            return await _context.Cupons
                .OrderByDescending(c => c.DataCriacao)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Cupom?> ObterPorCodigoAsync(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return null;
            }

            var normalizado = codigo.Trim().ToUpperInvariant();
            return await _context.Cupons
                .FirstOrDefaultAsync(c => c.Valor == normalizado && c.Ativo);
        }

        public async Task<bool> ClienteJaUsouCupomAsync(int codCliente, string codigoCupom)
        {
            if (codCliente <= 0 || string.IsNullOrWhiteSpace(codigoCupom))
            {
                return false;
            }

            var normalizado = codigoCupom.Trim().ToUpperInvariant();
            return await _context.CupomUsos
                .AnyAsync(u => u.CodCliente == codCliente && u.CodigoCupom == normalizado);
        }

        public async Task<(bool Valido, string Mensagem, int Desconto)> ValidarCupomClienteAsync(string codigo, int? codCliente)
        {
            if (string.IsNullOrWhiteSpace(codigo))
            {
                return (false, "Informe o codigo do cupom.", 0);
            }

            var normalizado = codigo.Trim().ToUpperInvariant();
            var cupom = await _context.Cupons
                .FirstOrDefaultAsync(c => c.Valor == normalizado && c.Ativo);

            if (cupom == null)
            {
                return (false, "Cupom invalido ou inexistente.", 0);
            }

            if (codCliente.HasValue && codCliente.Value > 0)
            {
                bool jaUsou = await ClienteJaUsouCupomAsync(codCliente.Value, normalizado);
                if (jaUsou)
                {
                    return (false, $"O cupom '{normalizado}' ja foi utilizado por voce nesta conta. Cada cupom e de uso unico.", 0);
                }
            }

            return (true, $"Cupom '{cupom.Valor}' aplicado com sucesso! ({cupom.Desconto}% de desconto)", cupom.Desconto);
        }

        public async Task<(bool Sucesso, string Mensagem)> CriarCupomAsync(string valor, int desconto)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return (false, "O codigo do cupom e obrigatorio.");
            }

            // Normaliza escala x10 caso seja informado valor em escala multiplicada (ex: 500 para 50%)
            if (desconto >= 100 && desconto <= 990)
            {
                desconto = (int)Math.Round(desconto / 10.0);
            }

            if (desconto < 1 || desconto > 99)
            {
                return (false, "O percentual de desconto deve estar entre 1% e 99%.");
            }

            var normalizado = valor.Trim().ToUpperInvariant();

            var cupomExistente = await _context.Cupons.FirstOrDefaultAsync(c => c.Valor == normalizado);
            if (cupomExistente != null)
            {
                if (!cupomExistente.Ativo)
                {
                    cupomExistente.Ativo = true;
                    cupomExistente.Desconto = desconto;
                    await _context.SaveChangesAsync();
                    return (true, $"Cupom '{normalizado}' reativado com sucesso com {desconto}% de desconto!");
                }

                return (false, $"O cupom '{normalizado}' ja esta cadastrado no sistema.");
            }

            var novoCupom = new Cupom
            {
                Valor = normalizado,
                Desconto = desconto,
                Ativo = true,
                DataCriacao = DateTime.Now
            };

            _context.Cupons.Add(novoCupom);
            await _context.SaveChangesAsync();

            return (true, $"Cupom '{normalizado}' cadastrado com sucesso com {desconto}% de desconto!");
        }

        public async Task<bool> AlternarStatusCupomAsync(int id)
        {
            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom == null)
            {
                return false;
            }

            cupom.Ativo = !cupom.Ativo;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExcluirCupomAsync(int id)
        {
            var cupom = await _context.Cupons.FindAsync(id);
            if (cupom == null)
            {
                return false;
            }

            // Soft delete para preservar integridade de historico e auditoria
            cupom.Ativo = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<CupomUso?> RegistrarUsoCupomAsync(int codCliente, string codigoCupom, int? idVenda = null)
        {
            if (codCliente <= 0 || string.IsNullOrWhiteSpace(codigoCupom))
            {
                return null;
            }

            var normalizado = codigoCupom.Trim().ToUpperInvariant();
            var usoExistente = await _context.CupomUsos
                .FirstOrDefaultAsync(u => u.CodCliente == codCliente && u.CodigoCupom == normalizado);

            if (usoExistente != null)
            {
                return usoExistente;
            }

            var uso = new CupomUso
            {
                CodCliente = codCliente,
                CodigoCupom = normalizado,
                IdVenda = idVenda,
                DataUso = DateTime.Now
            };

            _context.CupomUsos.Add(uso);
            await _context.SaveChangesAsync();
            return uso;
        }
    }
}
