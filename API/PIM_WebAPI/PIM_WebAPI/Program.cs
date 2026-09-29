using Microsoft.EntityFrameworkCore;
using PIM_WebAPI.Data;

namespace PIM_WebAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Configuracao da Conexao com SQL Server (ShopNowDB no servidor OMEGA)
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? "Server=OMEGA;Database=ShopNowDB;Trusted_Connection=True;TrustServerCertificate=True;";

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2. Registro de servicos de Controllers e OpenAPI
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // 3. Garantia do diretorio fisico de uploads de produtos
            var pastaUploads = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "produtos");
            if (!Directory.Exists(pastaUploads))
            {
                Directory.CreateDirectory(pastaUploads);
            }

            // 4. Inicializacao e Auto-Criacao do Banco de Dados via EF Core Migrations
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<AppDbContext>();

                await DbInitializer.InitializeAsync(context, logger);
            }

            // 5. Configuracao do Pipeline de Requisicoes HTTP
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // Middleware para servir imagens e midias estaticas de wwwroot/uploads
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync();
        }
    }
}
