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

                // 5. Inicializacao de Cupons Padrao
                await InicializarCuponsPadraoAsync(context, logger);
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
                // Script idempotente de migracao de tabelas e colunas
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

                    -- 5. Adiciona coluna FirstTime na tabela CLIENTES
                    IF NOT EXISTS (
                        SELECT 1 FROM sys.columns 
                        WHERE object_id = OBJECT_ID('CLIENTES') AND name = 'FirstTime'
                    )
                    BEGIN
                        ALTER TABLE CLIENTES ADD FirstTime BIT NOT NULL CONSTRAINT DF_CLIENTES_FirstTime DEFAULT 1;
                    END;

                    -- 6. Cria tabela CUPONS caso ainda nao exista
                    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CUPONS')
                    BEGIN
                        CREATE TABLE CUPONS (
                            id_Cupom INT IDENTITY(1,1) PRIMARY KEY,
                            VALOR VARCHAR(50) NOT NULL UNIQUE,
                            DESCONTO INT NOT NULL CHECK (DESCONTO >= 1 AND DESCONTO <= 99),
                            Ativo BIT NOT NULL DEFAULT 1,
                            DataCriacao DATETIME NOT NULL DEFAULT GETDATE()
                        );
                    END;

                    -- 7. Cria tabela CUPOM_USOS para controle de uso unico por usuario
                    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CUPOM_USOS')
                    BEGIN
                        CREATE TABLE CUPOM_USOS (
                            id_CupomUso INT IDENTITY(1,1) PRIMARY KEY,
                            Cod_do_Cliente INT NOT NULL,
                            Codigo_Cupom VARCHAR(50) NOT NULL,
                            id_Venda INT NULL,
                            Data_Uso DATETIME NOT NULL DEFAULT GETDATE(),
                            CONSTRAINT UQ_CupomUso UNIQUE (Cod_do_Cliente, Codigo_Cupom)
                        );
                    END;
                ";

                await context.Database.ExecuteSqlRawAsync(sqlAtualizacao);
                logger.LogInformation("Estrutura do banco de dados (PRODUTOS, CLIENTES, CUPONS, CUPOM_USOS) verificada e sincronizada.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Aviso ao executar atualizacoes estruturais em lote.");
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
                // 1. Seed de Usuarios (Admin e Cliente de Teste com FirstTime = true)
                if (!await context.Clientes.AnyAsync())
                {
                    logger.LogInformation("Inicializando contas de usuario padrao...");

                    var admin = new Cliente
                    {
                        Nome = "Administrador da Loja",
                        Email = "admin@lojamusical.com",
                        Senha = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        Telefone = "(11) 99999-0001",
                        IsAdmin = true,
                        FirstTime = false
                    };

                    var cliente = new Cliente
                    {
                        Nome = "Cliente Musical",
                        Email = "cliente@lojamusical.com",
                        Senha = BCrypt.Net.BCrypt.HashPassword("Cliente@123"),
                        Telefone = "(11) 98888-0002",
                        IsAdmin = false,
                        FirstTime = true
                    };

                    context.Clientes.AddRange(admin, cliente);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Contas padrao criadas com sucesso.");
                }

                // 2. Seed de Catalogo alinhado estritamente as 6 categorias do site
                if (!await context.Produtos.AnyAsync())
                {
                    logger.LogInformation("Populando catalogo tematico inicial de Instrumentos Musicais...");

                    var catalogo = new List<(string Categoria, string Modelo, decimal Preco, string Imagem, int Estoque)>
                    {
                        // 1. Guitarras
                        ("Guitarras", "Fender Player Stratocaster Sunburst", 6899.00m, "/img/guitarra.jpeg", 15),
                        ("Guitarras", "Gibson Les Paul Standard 60s Vintage", 8499.00m, "/img/guitarra.jpeg", 10),
                        ("Guitarras", "Ibanez GRG121DX Superstrato Black", 2199.00m, "/img/guitarra.jpeg", 20),
                        ("Guitarras", "Tagima TG-530 Woodstock White", 949.00m, "/img/guitarra.jpeg", 25),
                        ("Guitarras", "PRS SE Custom 24 Vintage Sunburst", 7299.00m, "/img/guitarra.jpeg", 8),

                        // 2. Violoes
                        ("Violões", "Folk Eletroacustico Cutaway Aco Natural", 1499.00m, "/img/violao.png", 20),
                        ("Violões", "Classico Nylon Cordas Estudante", 799.00m, "/img/violao.png", 30),
                        ("Violões", "Jumbo Eletroacustico Sunburst Tobacco", 2299.00m, "/img/violao.png", 12),
                        ("Violões", "Parlor Vintage Folk Acustico", 1690.00m, "/img/violao.png", 14),

                        // 3. Teclados
                        ("Teclados", "Sintetizador Workstation 61 Teclas Touch", 3490.00m, "/img/teclado.png", 10),
                        ("Teclados", "Piano Digital 88 Teclas Acao Pesada", 4999.00m, "/img/teclado.png", 8),
                        ("Teclados", "Controlador MIDI USB 49 Teclas Semi-Pesadas", 1290.00m, "/img/teclado.png", 18),
                        ("Teclados", "Teclado Arranjador 61 Teclas com Ritmos", 1890.00m, "/img/teclado.png", 15),

                        // 4. Baterias
                        ("Baterias", "Bateria Acustica 5 Pecas Bumbo 22 Sparkle Red", 4290.00m, "/img/bateria.jpeg", 6),
                        ("Baterias", "Bateria Eletronica Mesh Head com Modulo USB", 3690.00m, "/img/bateria.jpeg", 12),
                        ("Baterias", "Cajon Eletroacustico Inclinado Jacaranda", 480.00m, "/img/bateria.jpeg", 25),
                        ("Baterias", "Kit Pratos Liga B20 (Hi-Hat, Crash, Ride)", 2190.00m, "/img/bateria.jpeg", 14),

                        // 5. Outros
                        ("Outros", "Saxofone Alto Mib Laqueado Dourado", 3790.00m, "/img/outros.png", 8),
                        ("Outros", "Violino 4/4 Artesanal Spruce Maple Envernizado", 1490.00m, "/img/outros.png", 15),
                        ("Outros", "Flauta Transversal C Chaves Abertas Prateada", 980.00m, "/img/outros.png", 12),
                        ("Outros", "Trompete Sib Custom Dourado com Estojo", 2190.00m, "/img/outros.png", 9),
                        ("Outros", "Acordeon 80 Baixos 37 Teclas Perolado", 5490.00m, "/img/outros.png", 5),

                        // 6. Acessorios
                        ("Acessórios", "Cabo P10 Profissional 5m Blindado Conector Ouro", 79.90m, "/img/acessorios.png", 80),
                        ("Acessórios", "Afinador Digital Clip-on Recarregavel Display LED", 59.90m, "/img/acessorios.png", 90),
                        ("Acessórios", "Kit Palhetas Sortidas (12 un) 0.71mm / 0.96mm", 29.90m, "/img/acessorios.png", 150),
                        ("Acessórios", "Correia Acolchoada de Couro para Instrumentos", 119.00m, "/img/acessorios.png", 40),
                        ("Acessórios", "Suporte de Chao para Instrumentos com Trava", 95.00m, "/img/acessorios.png", 50),
                        ("Acessórios", "Encordoamento Violao Aco (.010) Bronze Fosforoso", 45.00m, "/img/acessorios.png", 100)
                    };

                    foreach (var item in catalogo)
                    {
                        var produto = new Produto
                        {
                            TipoInstrumento = item.Categoria,
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

                    logger.LogInformation("Catalogo com {Qtd} produtos distribuidos nas 6 categorias foi cadastrado.", catalogo.Count);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Aviso ao realizar seed de produtos e contas.");
            }
        }

        private static async Task InicializarCuponsPadraoAsync(AppDbContext context, ILogger logger)
        {
            try
            {
                if (!await context.Cupons.AnyAsync())
                {
                    logger.LogInformation("Inicializando cupons padrao no banco de dados...");

                    var cupons = new List<Cupom>
                    {
                        new Cupom { Valor = "ROCK15", Desconto = 15, Ativo = true },
                        new Cupom { Valor = "DEV10", Desconto = 10, Ativo = true },
                        new Cupom { Valor = "PROMO20", Desconto = 20, Ativo = true },
                        new Cupom { Valor = "MEGASHOW30", Desconto = 30, Ativo = true }
                    };

                    context.Cupons.AddRange(cupons);
                    await context.SaveChangesAsync();
                    logger.LogInformation("Cupons padrao cadastrados com sucesso: ROCK15 (15%), DEV10 (10%), PROMO20 (20%), MEGASHOW30 (30%).");
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Aviso ao realizar seed de cupons.");
            }
        }
    }
}
