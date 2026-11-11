# ShopNow - Repositório de Implementação (PIM IV)

Instruções técnicas para configuração e execução das aplicações do ecossistema corporativo ShopNow em estações de trabalho e ambientes locais.

---

## 1. Pré-Requisitos de Ambiente

Para executar as soluções do projeto em um computador diferente da estação original de desenvolvimento, instale:

- **Git** (versão 2.35 ou superior);
- **Microsoft .NET SDK 10.0** (ou superior) com suporte a ASP.NET Core;
- **Microsoft SQL Server** (edições Express, Developer ou Standard instaladas localmente);
- **Flutter SDK** (versão 3.x no canal estável) com suporte ao sistema alvo (Desktop Windows via `flutter config --enable-windows-desktop`, ou Android SDK / emulador), caso pretenda executar o aplicativo Mobile.

---

## 2. Banco de Dados e Configuração do SQL Server

A solução Web (`PIM_WebApplication`) executa uma rotina automática e idempotente de auto-provisionamento na inicialização (`DbInitializer`). Caso o banco `ShopNowDB` não exista na instância do SQL Server, o sistema cria automaticamente a estrutura das 8 tabelas e popula os dados semente essenciais (categorias de instrumentos musicais, produtos iniciais do catálogo com estoque e conta administrativa de testes).

Para que o auto-provisionamento e as operações transacionais ocorram corretamente em seu computador:
- Verifique a cadeia de conexão no arquivo `WEB/PIM_WebApplication/PIM_WebApplication/appsettings.json` (ou `appsettings.Development.json`):
  ```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ShopNowDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
  ```
- Altere o parâmetro `Server=` para coincidir com a sua instância local do Microsoft SQL Server (por exemplo: `localhost`, `.`, `localhost\SQLEXPRESS` ou o nome do seu computador). Não é necessário criar previamente tabelas ou rodar scripts manuais para iniciar a aplicação Web.

---

## 3. Relação de Dependência entre as Soluções (Web e Mobile)

O aplicativo móvel em Flutter (`MOBILE/`) foi projetado de forma desacoplada e consome dados estritamente através dos serviços RESTful expostos pelo monólito modular Web (`/api/*`). Ele não opera de forma autônoma sem a API.

Portanto:
- **A aplicação Web DEVE estar em execução antes de inicializar o aplicativo Mobile**, caso deseje navegar no catálogo real, aplicar cupons, registrar usuários ou realizar compras.
- Por padrão, o servidor local da aplicação Web (Kestrel) escuta requisições nas portas:
  - HTTP: `http://localhost:5019`
  - HTTPS: `https://localhost:7297`
- Para viabilizar a comunicação do aplicativo Flutter com a API:
  - **Execução Desktop Windows**: O aplicativo Flutter se comunica diretamente através de `http://localhost:5019/api`.
  - **Execução em Emulador Android**: Utilize `http://10.0.2.2:5019/api` (alias padrão do roteador virtual do Android para a máquina host).
  - **Execução em Dispositivo Físico**: Utilize o endereço IP local da máquina que está rodando o servidor Web (ex.: `http://192.168.1.X:5019/api`), garantindo que ambos os dispositivos estejam conectados na mesma rede local.
  - O aplicativo móvel dispõe de um diálogo de parametrização acessível pelo botão de engrenagem na barra superior de todas as telas principais (`ServerSettingsDialog`), permitindo ajustar o endereço do servidor dinamicamente em tempo de execução sem alterar código-fonte.

---

## 4. Como Rodar o Projeto em Cada Branch

A governança do repositório adota o modelo GitFlow adaptado. O conteúdo disponível e o procedimento de execução variam conforme a branch selecionada:

### 4.1. Branch `production` (Versão Estável Consolidada)
Contém a entrega final homologada da plataforma, unificando tanto a aplicação Web (.NET 10) quanto o aplicativo móvel (Flutter).

1. **Posicionar-se na branch**:
   ```bash
   git checkout production
   git pull origin production
   ```
2. **Executar a Solução Web**:
   ```bash
   cd WEB/PIM_WebApplication/PIM_WebApplication
   dotnet restore
   dotnet run
   ```
   - Portal da Loja no navegador: `http://localhost:5019`
   - Documentação OpenAPI/Swagger: `http://localhost:5019/swagger`
3. **Executar a Solução Mobile** (com o terminal da Web ativo em segundo plano):
   Em um segundo terminal, a partir da raiz da pasta `Implementacao`:
   ```bash
   cd MOBILE
   flutter pub get
   flutter run
   ```
   Selecione o dispositivo desejado (ex.: Windows Desktop, emulador Android ou dispositivo conectado).

---

### 4.2. Branch `develop` (Tronco de Integração Contínua)
Tronco de integração onde residem as versões mais recentes das frentes Web e Mobile já combinadas para testes e homologação contínua.

1. **Posicionar-se na branch**:
   ```bash
   git checkout develop
   git pull origin develop
   ```
2. **Executar a Solução Web**:
   ```bash
   cd WEB/PIM_WebApplication/PIM_WebApplication
   dotnet restore
   dotnet run
   ```
3. **Executar a Solução Mobile** (em terminal paralelo com a Web ativa):
   ```bash
   cd MOBILE
   flutter pub get
   flutter run
   ```

---

### 4.3. Branch `feat/web` (Desenvolvimento da Solução Web)
Contém exclusivamente o código-fonte da aplicação Web ASP.NET Core MVC (Razor Views / SSR) e os controladores RESTful sob `/api/*`. O aplicativo móvel não está versionado nesta branch.

1. **Posicionar-se na branch**:
   ```bash
   git checkout feat/web
   git pull origin feat/web
   ```
2. **Executar a Solução Web**:
   ```bash
   cd WEB/PIM_WebApplication/PIM_WebApplication
   dotnet restore
   dotnet run
   ```
   - Acesso Web: `http://localhost:5019`
   - Credenciais administrativas iniciais: `admin@shopnow.com` / `AdminPassword123!` (geradas pelo inicializador caso a base esteja limpa).

---

### 4.4. Branch `feat/mobile` (Desenvolvimento da Solução Mobile)
Contém exclusivamente o código-fonte do aplicativo Flutter (`apppim`) e esteiras CI de compilação mobile. A versão recente do backend Web não reside nesta branch.

1. **Posicionar-se na branch**:
   ```bash
   git checkout feat/mobile
   git pull origin feat/mobile
   ```
2. **Executar a Solução Mobile**:
   ```bash
   cd MOBILE
   flutter pub get
   flutter run
   ```
3. **Requisito Operacional**: Para que o aplicativo mobile consuma os produtos e registre pedidos reais nesta branch, a solução Web precisa estar sendo executada em paralelo a partir de outra pasta/branch (`develop` ou `production`) ou hospedada em host acessível pela rede local. Ajuste a URL no ícone de engrenagem no topo do aplicativo.

---

### 4.5. Branch `feat/api` (Backend Desacoplado Legado)
Branch histórica utilizada na fase inicial do projeto que continha o serviço independente `PIM_WebAPI`, antes da unificação dos endpoints na solução modular `PIM_WebApplication` (decisão formalizada na ADR-004).

1. **Posicionar-se na branch**:
   ```bash
   git checkout feat/api
   ```
2. **Executar a API**:
   ```bash
   cd API/PIM_WebAPI/PIM_WebAPI
   dotnet restore
   dotnet run
   ```
   - Documentação Swagger: `http://localhost:5032/swagger`
