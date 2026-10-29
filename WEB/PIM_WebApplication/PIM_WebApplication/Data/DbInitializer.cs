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

                // 1. Criacao do banco e tabelas caso ainda nao existam no SQL Server
                bool created = await context.Database.EnsureCreatedAsync();
                if (created)
                {
                    logger.LogInformation("Banco de dados ShopNowDB criado com sucesso com catalogo vazio.");
                }
                else
                {
                    logger.LogInformation("Banco de dados ShopNowDB ja existe.");
                }

                // 2. Atualizacao estrutural resiliente para bancos pre-existentes (Migracao de colunas e novas tabelas)
                await AtualizarEstruturaTabelasAsync(context, logger);

                // 3. Provisionamento dos diretorios fisicos para uploads de midia
                ProvisionarDiretoriosUpload(logger);

                logger.LogInformation("Inicializacao do banco de dados concluida. Catalogo pronto e vazio para operacao.");
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
                var basePathUploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "produtos");
                if (!Directory.Exists(basePathUploads))
                {
                    Directory.CreateDirectory(basePathUploads);
                    logger.LogInformation("Diretorio de upload provisionado em: {Caminho}", basePathUploads);
                }

                var imgUploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "img", "uploads");
                if (!Directory.Exists(imgUploadsPath))
                {
                    Directory.CreateDirectory(imgUploadsPath);
                    logger.LogInformation("Diretorio de imagens provisionado em: {Caminho}", imgUploadsPath);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Falha ao provisionar diretorios fisicos de uploads.");
            }
        }
    }
}
