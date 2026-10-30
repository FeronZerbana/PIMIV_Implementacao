-- ==============================================================================
-- PROJETO INTEGRADO MULTIDISCIPLINAR (PIM IV) - DISCIPLINA: BANCO DE DADOS
-- SISTEMA: ShopNow - Comercio Eletronico e Gestao de Instrumentos Musicais
-- SGBD: Microsoft SQL Server (Transact-SQL)
-- ARQUIVO: StoredProcedures_Triggers_ShopNow.sql
-- DESCRICAO: Definicao de Stored Procedures, Triggers, Tabelas de Auditoria e
--            Cenarios de Teste Corporativos para Homologacao Academica.
-- ==============================================================================

USE ShopNowDB;
GO

-- ==============================================================================
-- 1. TABELA DE AUDITORIA DE TRANSACOES
-- ==============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AUDITORIA_VENDAS')
BEGIN
    CREATE TABLE AUDITORIA_VENDAS (
        id_Auditoria INT IDENTITY(1,1) PRIMARY KEY,
        id_Venda INT NOT NULL,
        Cod_do_Cliente INT NOT NULL,
        Data_Acao DATETIME NOT NULL DEFAULT GETDATE(),
        Tipo_Acao VARCHAR(20) NOT NULL, -- 'INSERCAO' ou 'EXCLUSAO'
        Usuario_SO VARCHAR(100) NOT NULL DEFAULT SUSER_SNAME(),
        Host_Name VARCHAR(100) NOT NULL DEFAULT HOST_NAME()
    );
    PRINT 'Tabela AUDITORIA_VENDAS criada com sucesso.';
END
ELSE
BEGIN
    PRINT 'Tabela AUDITORIA_VENDAS ja existe.';
END;
GO

-- ==============================================================================
-- 2. TRIGGERS (INTEGRIDADE, BAIXA DE ESTOQUE E AUDITORIA)
-- ==============================================================================

-- 2.1. Trigger: trg_AtualizarEstoque_ItemVenda
-- Finalidade: Decrementar automaticamente o estoque disponivel a cada insercao em
-- ITEM_VENDAS e impedir que o saldo se torne negativo (Atomicidade e Integridade).
CREATE OR ALTER TRIGGER trg_AtualizarEstoque_ItemVenda
ON ITEM_VENDAS
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- Decrementa o saldo fisico na tabela ESTOQUE para cada item comercializado
    UPDATE e
    SET e.Quantidade_Disponivel = e.Quantidade_Disponivel - i.Quantidade
    FROM ESTOQUE e
    INNER JOIN inserted i ON e.Cod_do_Produto = i.Cod_do_Produto;

    -- Validacao de Integridade: Se qualquer item provocar saldo negativo, aborta a transacao
    IF EXISTS (
        SELECT 1 
        FROM ESTOQUE e
        INNER JOIN inserted i ON e.Cod_do_Produto = i.Cod_do_Produto
        WHERE e.Quantidade_Disponivel < 0
    )
    BEGIN
        RAISERROR('Operacao abortada: Saldo de estoque insuficiente para um ou mais itens da venda.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

-- 2.2. Trigger: trg_ValidarEstoqueMinimo
-- Finalidade: Garantir que nenhuma alteracao direta na tabela ESTOQUE permita valores negativos.
CREATE OR ALTER TRIGGER trg_ValidarEstoqueMinimo
ON ESTOQUE
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE Quantidade_Disponivel < 0)
    BEGIN
        RAISERROR('Violacao de regra de negocio: A quantidade disponivel em estoque nao pode ser inferior a zero.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END;
END;
GO

-- 2.3. Trigger: trg_AuditoriaVendas
-- Finalidade: Registrar eventos de auditoria na tabela AUDITORIA_VENDAS a cada emissao ou remocao de venda.
CREATE OR ALTER TRIGGER trg_AuditoriaVendas
ON VENDAS
AFTER INSERT, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    -- Registro de criacao de venda
    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO AUDITORIA_VENDAS (id_Venda, Cod_do_Cliente, Data_Acao, Tipo_Acao)
        SELECT id_Vendas, Cod_do_Cliente, GETDATE(), 'INSERCAO'
        FROM inserted;
    END;

    -- Registro de estorno/cancelamento de venda
    IF EXISTS (SELECT 1 FROM deleted) AND NOT EXISTS (SELECT 1 FROM inserted)
    BEGIN
        INSERT INTO AUDITORIA_VENDAS (id_Venda, Cod_do_Cliente, Data_Acao, Tipo_Acao)
        SELECT id_Vendas, Cod_do_Cliente, GETDATE(), 'EXCLUSAO'
        FROM deleted;
    END;
END;
GO

-- ==============================================================================
-- 3. STORED PROCEDURES DE NEGOCIO E OPERACAO
-- ==============================================================================

-- 3.1. Procedure: sp_CadastrarProdutoComEstoque
-- Finalidade: Realizar o cadastro atomico de um novo instrumento/acessorio em PRODUTOS
-- e criar imediatamente a sua respectiva conta de saldo na tabela ESTOQUE.
CREATE OR ALTER PROCEDURE sp_CadastrarProdutoComEstoque
    @TipoInstrumento VARCHAR(150),
    @Preco DECIMAL(8,2),
    @Modelo VARCHAR(100),
    @ImagemUrl VARCHAR(255) = NULL,
    @QuantidadeEstoque INT = 0,
    @IdProdutoGerado INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Preco <= 0)
    BEGIN
        RAISERROR('O preco do produto deve ser estritamente maior que zero.', 16, 1);
        RETURN;
    END;

    BEGIN TRANSACTION;
    BEGIN TRY
        INSERT INTO PRODUTOS (Tipo_Instrumento, Preco, Modelo, Imagem_Url)
        VALUES (@TipoInstrumento, @Preco, @Modelo, @ImagemUrl);

        SET @IdProdutoGerado = SCOPE_IDENTITY();

        INSERT INTO ESTOQUE (Cod_do_Produto, Quantidade_Disponivel)
        VALUES (@IdProdutoGerado, CASE WHEN @QuantidadeEstoque >= 0 THEN @QuantidadeEstoque ELSE 0 END);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF (XACT_STATE() <> 0)
            ROLLBACK TRANSACTION;

        DECLARE @MsgErro NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@MsgErro, 16, 1);
        RETURN;
    END CATCH;
END;
GO

-- 3.2. Procedure: sp_ValidarEAplicarCupom
-- Finalidade: Validar regras de cupom de desconto (ativo, teto de 50%, e uso unico por cliente).
CREATE OR ALTER PROCEDURE sp_ValidarEAplicarCupom
    @CodCliente INT,
    @CodigoCupom VARCHAR(50),
    @Subtotal DECIMAL(10,2),
    @CupomValido BIT OUTPUT,
    @PercentualDesconto INT OUTPUT,
    @ValorDesconto DECIMAL(10,2) OUTPUT,
    @MensagemRetorno VARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @CupomValido = 0;
    SET @PercentualDesconto = 0;
    SET @ValorDesconto = 0.00;
    SET @MensagemRetorno = '';

    IF (@Subtotal <= 0)
    BEGIN
        SET @MensagemRetorno = 'Subtotal invalido para aplicacao de cupom.';
        RETURN;
    END;

    IF (@CodigoCupom IS NULL OR RTRIM(LTRIM(@CodigoCupom)) = '')
    BEGIN
        SET @MensagemRetorno = 'Nenhum codigo de cupom informado.';
        RETURN;
    END;

    DECLARE @CodigoNormalizado VARCHAR(50) = UPPER(RTRIM(LTRIM(@CodigoCupom)));
    DECLARE @DescontoBase INT;
    DECLARE @Ativo BIT;

    SELECT @DescontoBase = DESCONTO, @Ativo = Ativo
    FROM CUPONS
    WHERE VALOR = @CodigoNormalizado;

    IF (@DescontoBase IS NULL)
    BEGIN
        SET @MensagemRetorno = 'Cupom de desconto inexistente.';
        RETURN;
    END;

    IF (@Ativo = 0)
    BEGIN
        SET @MensagemRetorno = 'Este cupom encontra-se inativo.';
        RETURN;
    END;

    -- Validacao de unicidade de uso por cliente (Regra RN-CUP-01)
    IF EXISTS (
        SELECT 1 
        FROM CUPOM_USOS 
        WHERE Cod_do_Cliente = @CodCliente AND Codigo_Cupom = @CodigoNormalizado
    )
    BEGIN
        SET @MensagemRetorno = 'Este cupom ja foi utilizado anteriormente por sua conta.';
        RETURN;
    END;

    -- Normalizacao de escala de desconto (1 a 99)
    IF (@DescontoBase >= 100 AND @DescontoBase <= 990)
        SET @DescontoBase = @DescontoBase / 10;

    SET @PercentualDesconto = @DescontoBase;
    DECLARE @CalculoBruto DECIMAL(10,2) = ROUND(@Subtotal * (@DescontoBase / 100.0), 2);

    -- Teto maximo permitido de 50% de abatimento (Regra RN-PREC-04)
    DECLARE @TetoMaximo DECIMAL(10,2) = ROUND(@Subtotal * 0.50, 2);
    IF (@CalculoBruto > @TetoMaximo)
        SET @ValorDesconto = @TetoMaximo;
    ELSE
        SET @ValorDesconto = @CalculoBruto;

    SET @CupomValido = 1;
    SET @MensagemRetorno = 'Cupom aplicado com sucesso.';
END;
GO

-- 3.3. Procedure: sp_RegistrarVendaCompleta
-- Finalidade: Executar transacionalmente a emissao de uma venda, atualizando FirstTime e
-- gravando o vinculo com cupom em CUPOM_USOS e propagando id_CupomUso para ITEM_VENDAS.
CREATE OR ALTER PROCEDURE sp_RegistrarVendaCompleta
    @CodCliente INT,
    @CodigoCupom VARCHAR(50) = NULL,
    @IdVendaGerada INT OUTPUT,
    @IdCupomUsoGerado INT = NULL OUTPUT,
    @MensagemRetorno VARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @IdVendaGerada = 0;
    SET @IdCupomUsoGerado = NULL;
    SET @MensagemRetorno = '';

    -- Validacao do cliente
    IF NOT EXISTS (SELECT 1 FROM CLIENTES WHERE id_cliente = @CodCliente)
    BEGIN
        SET @MensagemRetorno = 'Cliente nao encontrado no sistema.';
        RAISERROR(@MensagemRetorno, 16, 1);
        RETURN;
    END;

    BEGIN TRANSACTION;
    BEGIN TRY
        -- 1. Insercao do cabecalho da Venda (ativa trigger de auditoria)
        INSERT INTO VENDAS (Cod_do_Cliente, Data_Venda)
        VALUES (@CodCliente, GETDATE());

        SET @IdVendaGerada = SCOPE_IDENTITY();

        -- 2. Registro do uso de cupom caso informado
        IF (@CodigoCupom IS NOT NULL AND RTRIM(LTRIM(@CodigoCupom)) <> '')
        BEGIN
            DECLARE @CodCupomNormalizado VARCHAR(50) = UPPER(RTRIM(LTRIM(@CodigoCupom)));

            INSERT INTO CUPOM_USOS (Cod_do_Cliente, Codigo_Cupom, id_Venda, Data_Uso)
            VALUES (@CodCliente, @CodCupomNormalizado, @IdVendaGerada, GETDATE());

            SET @IdCupomUsoGerado = SCOPE_IDENTITY();
        END;
        ELSE
        BEGIN
            SET @IdCupomUsoGerado = NULL;
        END;

        -- 3. Atualizacao da flag de primeira compra (Regra RN-PREC-05)
        UPDATE CLIENTES
        SET FirstTime = 0
        WHERE id_cliente = @CodCliente AND FirstTime = 1;

        COMMIT TRANSACTION;
        SET @MensagemRetorno = 'Venda registrada com sucesso.';
    END TRY
    BEGIN CATCH
        IF (XACT_STATE() <> 0)
            ROLLBACK TRANSACTION;

        DECLARE @MsgErro NVARCHAR(4000) = ERROR_MESSAGE();
        SET @MensagemRetorno = @MsgErro;
        RAISERROR(@MsgErro, 16, 1);
        RETURN;
    END CATCH;
END;
GO

-- ==============================================================================
-- 4. STORED PROCEDURES ANALITICAS E DE CONSULTA (RELATORIOS)
-- ==============================================================================

-- 4.1. Procedure: sp_RelatorioVendasPorPeriodo
-- Finalidade: Gerar consolidado analitico de vendas, clientes, itens comercializados e faturamento bruto.
CREATE OR ALTER PROCEDURE sp_RelatorioVendasPorPeriodo
    @DataInicio DATETIME,
    @DataFim DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        v.id_Vendas AS id_Venda,
        v.Data_Venda,
        c.Nome AS Nome_Cliente,
        c.Email AS Email_Cliente,
        COUNT(iv.id_Item_Venda) AS Quantidade_Itens_Distintos,
        ISNULL(SUM(iv.Quantidade), 0) AS Total_Pecas_Vendidas,
        ISNULL(SUM(iv.Valor_Total), 0.00) AS Valor_Bruto_Venda,
        cu.Codigo_Cupom AS Cupom_Utilizado
    FROM VENDAS v
    INNER JOIN CLIENTES c ON v.Cod_do_Cliente = c.id_cliente
    LEFT JOIN ITEM_VENDAS iv ON v.id_Vendas = iv.Cod_da_Venda
    LEFT JOIN CUPOM_USOS cu ON v.id_Vendas = cu.id_Venda
    WHERE v.Data_Venda >= @DataInicio AND v.Data_Venda <= @DataFim
    GROUP BY 
        v.id_Vendas, 
        v.Data_Venda, 
        c.Nome, 
        c.Email, 
        cu.Codigo_Cupom
    ORDER BY v.Data_Venda DESC;
END;
GO

-- 4.2. Procedure: sp_RelatorioInstrumentosMaisVendidos
-- Finalidade: Apresentar o ranking analitico de giro de estoque, total vendido e estoque remanescente.
CREATE OR ALTER PROCEDURE sp_RelatorioInstrumentosMaisVendidos
    @Top INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Top)
        p.id_Produto,
        p.Tipo_Instrumento AS Categoria,
        p.Modelo,
        p.Preco AS Preco_Unitario_Atual,
        ISNULL(SUM(iv.Quantidade), 0) AS Total_Unidades_Vendidas,
        ISNULL(SUM(iv.Valor_Total), 0.00) AS Faturamento_Total_Gerado,
        ISNULL(e.Quantidade_Disponivel, 0) AS Estoque_Remanescente_Atual
    FROM PRODUTOS p
    LEFT JOIN ITEM_VENDAS iv ON p.id_Produto = iv.Cod_do_Produto
    LEFT JOIN ESTOQUE e ON p.id_Produto = e.Cod_do_Produto
    GROUP BY 
        p.id_Produto, 
        p.Tipo_Instrumento, 
        p.Modelo, 
        p.Preco, 
        e.Quantidade_Disponivel
    ORDER BY Total_Unidades_Vendidas DESC, Faturamento_Total_Gerado DESC;
END;
GO

PRINT 'Stored Procedures e Triggers corporativos do ShopNow compilados com exito.';
GO
