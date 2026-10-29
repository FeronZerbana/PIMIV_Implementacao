using Microsoft.EntityFrameworkCore;
using PIM_WebApplication.Models;

namespace PIM_WebApplication.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                logger.LogInformation("Verificando existencia e estrutura do banco de dados ShopNowDB...");

                // 1. Criacao do banco e tabelas caso ainda nao existam
                bool created = await context.Database.EnsureCreatedAsync();
                if (created)
                {
                    logger.LogInformation("Banco de dados ShopNowDB criado com sucesso.");
                }
                else
                {
                    logger.LogInformation("Banco de dados ShopNowDB ja existe.");
                }

                // 2. Atualizacao estrutural resiliente para bancos pre-existentes (Migracao automatica)
                await AtualizarEstruturaTabelasAsync(context, logger);

                // 3. Provisionamento do diretorio fisico para uploads de midia
                ProvisionarDiretoriosUpload(logger);

                // 4. Inicializacao de dados tematicos de Instrumentos Musicais
                await InicializarDadosMusicaisAsync(context, logger);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro durante a inicializacao e sincronizacao do banco de dados ShopNowDB.");
            }
        }

        private static async Task AtualizarEstruturaTabelasAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                // Script idempotente de migracao de colunas legadas para a tematica de instrumentos musicais
                var sqlAtualizacao = @"
                    -- 1. Renomeia Tipo_Roupa para Tipo_Instrumento caso a coluna antiga exista
                    IF EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('PRODUTOS') AND name = 'Tipo_Roupa'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('PRODUTOS') AND name = 'Tipo_Instrumento'
                    )
                    BEGIN
                        EXEC sp_rename 'PRODUTOS.Tipo_Roupa', 'Tipo_Instrumento', 'COLUMN';
                    END;

                    -- 2. Renomeia Tamanho para Modelo caso a coluna antiga exista
                    IF EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('PRODUTOS') AND name = 'Tamanho'
                    ) AND NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('PRODUTOS') AND name = 'Modelo'
                    )
                    BEGIN
                        EXEC sp_rename 'PRODUTOS.Tamanho', 'Modelo', 'COLUMN';
                    END;

                    -- 3. Garante que a coluna Modelo suporte nomes maiores (VARCHAR(100))
                    IF EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('PRODUTOS') AND name = 'Modelo' AND max_length < 100
                    )
                    BEGIN
                        ALTER TABLE PRODUTOS ALTER COLUMN Modelo VARCHAR(100) NULL;
                    END;

                    -- 4. Adiciona a coluna Imagem_Url se ainda nao existir
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('PRODUTOS') AND name = 'Imagem_Url'
                    )
                    BEGIN
                        ALTER TABLE PRODUTOS ADD Imagem_Url VARCHAR(255) NULL;
                    END;
                ";

                await context.Database.ExecuteSqlRawAsync(sqlAtualizacao);
                logger.LogInformation("Estrutura da tabela PRODUTOS verificada e harmonizada com a tematica de Instrumentos.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Nao foi possivel executar a atualizacao estrutural direta (pode ser banco em memoria ou sem permissoes DDL).");
            }
        }

        private static void ProvisionarDiretoriosUpload(ILogger logger)
        {
            try
            {
                var basePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "produtos");
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                    logger.LogInformation("Diretorio de upload provisionado em: {Caminho}", basePath);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao provisionar diretorio fisico de uploads.");
            }
        }

        private static async Task InicializarDadosMusicaisAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                // 1. Seed de Usuarios (Admin e Cliente de Teste)
                if (!await context.Clientes.AnyAsync())
                {
                    logger.LogInformation("Inicializando contas de usuario padrao...");

                    var admin = new Cliente
                    {
                        Nome = "Administrador da Loja",
                        Email = "admin@lojamusical.com",
                        Senha = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        Telefone = "(11) 99999-0001",
                        IsAdmin = true
                    };

                    var cliente = new Cliente
                    {
                        Nome = "Cliente Musical",
                        Email = "cliente@lojamusical.com",
                        Senha = BCrypt.Net.BCrypt.HashPassword("Cliente@123"),
                        Telefone = "(11) 98888-0002",
                        IsAdmin = false
                    };

                    context.Clientes.AddRange(admin, cliente);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Contas padrao criadas: admin@lojamusical.com e cliente@lojamusical.com.");
                }

                // 2. Seed de Catalogo de Instrumentos Musicais
                if (!await context.Produtos.AnyAsync())
                {
                    logger.LogInformation("Populando catalogo tematico inicial de Instrumentos Musicais...");

                    var catalogo = new List<(string Instrumento, string Modelo, decimal Preco, string Imagem, int Estoque)>
                    {
                        // Guitarras
                        ("Guitarra Eletrica Stratocaster", "Sunburst Maple", 2499.00m, "/img/guitarra.png", 20),
                        ("Guitarra Eletrica Les Paul", "Vintage Blackwood", 3899.90m, "/img/guitarra.png", 15),
                        ("Guitarra Superstrato HSH", "Neon Purple", 2999.00m, "/img/guitarra.png", 18),

                        // Violoes
                        ("Violao Eletroacustico Folk", "Aco Natural", 1399.00m, "/img/violao.png", 25),
                        ("Violao Classico Nylon", "Ebano Fosco", 899.00m, "/img/violao.png", 30),
                        ("Violao Jumbo Cutaway", "Sunburst Tobacco", 2150.00m, "/img/violao.png", 12),

                        // Teclados e Pianos
                        ("Teclado Sintetizador Workstation", "61 Teclas Touch", 3200.00m, "/img/teclado.png", 10),
                        ("Piano Digital 88 Teclas", "Movel Mogno Acao Pesada", 4799.00m, "/img/teclado.png", 8),
                        ("Controlador MIDI USB", "49 Teclas Semi-Pesadas", 1150.00m, "/img/teclado.png", 22),

                        // Baterias e Percussao
                        ("Bateria Acustica 5 Pecas", "Bumbo 22 Sparkle Red", 4290.00m, "/img/bateria.png", 7),
                        ("Bateria Eletronica Mesh Head", "Modulo DSP USB", 3590.00m, "/img/bateria.png", 14),
                        ("Cajon Eletroacustico", "Inclinado Jacaranda", 450.00m, "/img/bateria.png", 35),

                        // Outros Instrumentos
                        ("Saxofone Alto Mib", "Laqueado Dourado", 3790.00m, "/img/outros.png", 6),
                        ("Violino 4/4 Artesanal", "Spruce & Maple Envelhecido", 1490.00m, "/img/outros.png", 12),
                        ("Flauta Transversal C", "Chaves Abertas Prateada", 980.00m, "/img/outros.png", 15),
                        ("Trompete Sib Custom", "Dourado com Estojo Luxo", 2190.00m, "/img/outros.png", 8),

                        // Acessorios
                        ("Cabo P10 Profissional 5m", "Blindado Conectores Ouro", 79.90m, "/img/acessorios.png", 100),
                        ("Afinador Digital Clip-on", "Display LED Recarregavel", 59.90m, "/img/acessorios.png", 80),
                        ("Kit Palhetas Sortidas (12 un)", "0.71mm / 0.96mm Celuloide", 29.90m, "/img/acessorios.png", 150),
                        ("Correia Acolchoada de Couro", "Reforcada Ajustavel", 119.00m, "/img/acessorios.png", 50),
                        ("Suporte de Chao para Instrumentos", "Aco com Trava Automatica", 95.00m, "/img/acessorios.png", 60),
                        ("Encordoamento Violao Aco (.010)", "Bronze Fosforoso Alta Durabilidade", 45.00m, "/img/acessorios.png", 120)
                    };

                    foreach (var item in catalogo)
                    {
                        var produto = new Produto
                        {
                            TipoInstrumento = item.Instrumento,
                            Modelo = item.Modelo,
                            Preco = item.Preco,
                            ImagemUrl = item.Imagem
                        };

                        context.Produtos.Add(produto);
                        await context.SaveChangesAsync();

                        var estoque = new Estoque
                        {
                            CodProduto = produto.IdProduto,
                            QuantidadeDisponivel = item.Estoque
                        };

                        context.Estoques.Add(estoque);
                        await context.SaveChangesAsync();
                    }

                    logger.LogInformation("Catalogo tematico inicial com {Qtd} instrumentos cadastrado com sucesso.", catalogo.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Aviso ao realizar seed de dados tematicos iniciais.");
            }
        }
    }
}
