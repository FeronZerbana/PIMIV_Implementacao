using Microsoft.EntityFrameworkCore;

namespace PIM_WebAPI.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Iniciando verificacao e auto-criacao do banco de dados ShopNowDB via EF Core Migrations...");

                // Cria o banco de dados caso nao exista no SQL Server e aplica todas as migrations pendentes
                await context.Database.MigrateAsync();

                logger.LogInformation("Banco de dados ShopNowDB provisionado e sincronizado com sucesso com as migrations.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Falha ao provisionar ou conectar ao banco de dados ShopNowDB no SQL Server.");
            }
        }
    }
}
