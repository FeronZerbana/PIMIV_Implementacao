using PIM_WebApplication.Models;

namespace PIM_WebApplication.Services
{
    public interface ICupomService
    {
        Task<List<Cupom>> ObterTodosAsync();
        Task<Cupom?> ObterPorCodigoAsync(string codigo);
        Task<(bool Valido, string Mensagem, int Desconto)> ValidarCupomClienteAsync(string codigo, int? codCliente);
        Task<(bool Sucesso, string Mensagem)> CriarCupomAsync(string valor, int desconto);
        Task<bool> ExcluirCupomAsync(int id);
        Task<CupomUso?> RegistrarUsoCupomAsync(int codCliente, string codigoCupom, int? idVenda = null);
        Task<bool> ClienteJaUsouCupomAsync(int codCliente, string codigoCupom);
    }
}
