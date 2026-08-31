using Microsoft.EntityFrameworkCore;

namespace PIM_WebApplication.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Verificando se o banco de dados ShopNowDB existe...");
                
                // Cria o banco de dados e todas as tabelas se ainda nao existirem, sem popular dados
                bool created = await context.Database.EnsureCreatedAsync();
                if (created)
                {
                    logger.LogInformation("Banco de dados ShopNowDB e esquemas relacionais criados com sucesso (sem dados iniciais).");
                }
                else
                {
                    logger.LogInformation("Banco de dados ShopNowDB ja existe.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao inicializar e verificar existencia do banco de dados ShopNowDB.");
                // Nao interrompe a subida da aplicacao caso o servidor SQL esteja temporariamente inacessivel
            }
        }
    }
}
