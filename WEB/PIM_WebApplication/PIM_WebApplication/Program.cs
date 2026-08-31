using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Data;
using PIM_WebApplication.Services;

namespace PIM_WebApplication
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

            // 2. Registro de Servicos de Dominio e Negocio
            builder.Services.AddScoped<IProdutoService, ProdutoService>();
            builder.Services.AddScoped<IEstoqueService, EstoqueService>();
            builder.Services.AddScoped<IVendaService, VendaService>();
            builder.Services.AddScoped<IUsuarioService, UsuarioService>();
            builder.Services.AddHttpContextAccessor();

            // 3. Suporte a Sessao (para Carrinho de Compras do MVC)
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(2);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 4. Autenticacao baseada em Cookies com Claims
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Conta/Login";
                    options.LogoutPath = "/Conta/Logout";
                    options.AccessDeniedPath = "/Conta/AcessoNegado";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                });

            // 5. Suporte conjunto a Views Razor (MVC) e Endpoints REST (API)
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();

            // 6. Swagger / OpenAPI para documentacao e teste dos endpoints da API
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // 7. CORS (compatibilidade para clientes externos Mobile e Desktop)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("PermitirTudo", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // 8. Inicializacao e Criacao Automatica do Banco de Dados ShopNowDB no OMEGA
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                var context = services.GetRequiredService<AppDbContext>();

                await DbInitializer.InitializeAsync(context, logger);
            }

            // 9. Pipeline de Requisicoes HTTP
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("PermitirTudo");

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            // Rotas de API e MVC
            app.MapControllers();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}
