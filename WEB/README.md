# Subsistema Web: Branch de Controle (controle/web)

> **Branch de Origem**: `controle/web`  
> **Finalidade**: Baseline estatico de comparacao do sistema web legado (`Site-API`)  
> **Dependencias Externas**: Nenhuma (execucao 100% estatica no navegador)

---

## 1. Objetivo da Branch de Controle

Esta branch atua como grupo de controle visual e funcional para a equipe de desenvolvimento e avaliadores do projeto PIM IV.

Ao alternar para esta branch (`git checkout controle/web`), e possivel inspecionar, executar e comparar a interface web original diretamente no navegador, avaliando paridade de telas, textos, estilizacoes e regras de negocio em relacao a nova implementacao em ASP.NET Core MVC na branch `feat/web`.

---

## 2. Como Executar

Por ser uma versao puramente estatica baseada em HTML5, CSS3 e JavaScript puro (ES6), nao ha necessidade de compilar solucoes .NET nem de subir servicos de banco de dados SQL Server ou Web API:

### Opcao 1: Abertura Direta no Navegador
Abra o arquivo `WEB/index.html` com duplo clique ou arraste-o para o navegador de sua preferencia (Google Chrome, Microsoft Edge, Mozilla Firefox).

### Opcao 2: Servidor Estatico Local (Live Server / Node / Python)
Caso deseje executar com um servidor HTTP local:

* **Extensao VS Code**: Abra a pasta `WEB/` e clique em *Go Live* (Live Server).
* **Python**:
  ```bash
  cd WEB
  python -m http.server 8080
  ```
* **Node (npx serve)**:
  ```bash
  cd WEB
  npx serve .
  ```

---

## 3. Funcionalidades Emuladas Localmente (Sem Backend)

1. **Catalogo de Produtos**: Carregamento imediato de catalogo com saldos de estoque pre-configurados em memoria e persistidos no `localStorage`.
2. **Navegacao e Telas**:
   * Vitrine principal (`page-home`)
   * Ofertas especiais (`page-ofertas`)
   * Apresentacao institucional e equipe (`page-sobre`)
   * Politica de privacidade e LGPD (`page-privacidade`)
   * Acessibilidade digital e Libras (`page-libras`)
   * Carrinho de compras (`page-cart`)
   * Painel administrativo (`page-admin`)
3. **Fluxos de Compra e Cupons**:
   * Adicao ao carrinho com controle de estoque maximo.
   * Cupons validos: `FRETE22` (frete gratis), `DEV10` (10% off), `SHOPNOW15` (15% off) e `PROMO50` (50% off).
   * Fechamento de pedido com geracao de numero de comprovante e deducao automatica de saldo de estoque.
4. **Painel de Administracao**:
   * Para acessar o painel de administracao, utilize o e-mail `admin@shopnow.com` na tela de login.
   * Permite cadastrar novos produtos e reabastecer o estoque disponivel com atualizacao em tempo real da vitrine.
