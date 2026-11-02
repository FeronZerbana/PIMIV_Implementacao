# Regras de Governanca do Projeto (Implementacao)

> O repositorio de documentacao oficial e fonte unica da verdade (SSOT) esta localizado em `Vault/`.

---

## 1. Regras Mandatorias de Desenvolvimento

1. **Proibido Codigo Sem Spec**: Antes de criar ou modificar qualquer codigo nos diretorios `WEB/`, `DESKTOP/` ou `MOBILE/`, consulte e valide a respectiva especificacao no Vault (`04-Specs-Backlog/Active/` ou `03-Systems/`).
2. **Proibicao Estrita de Emojis**: E terminantemente proibido o uso de emojis em arquivos de documentacao, especificacoes, ADRs, mensagens de commit e comentarios de codigo. Toda a comunicacao tecnica deve ser formal, sobria e profissional.
3. **Branching Model**:
   - `feat/web`: Alteracoes e desenvolvimento do sistema Web.
   - `feat/desktop`: (Deprecada / Obsoleta) O cliente Desktop foi unificado na aplicacao Flutter em `MOBILE/` via branch `feat/mobile`.
   - `feat/mobile`: Alteracoes e desenvolvimento do sistema Mobile e Desktop (Flutter multiplataforma).
   - `develop`: Branch de integracao e testes conjuntos.
   - `production`: Branch estavel de entrega final consolidada.
4. **Core Domain Unificado**: Todas as regras de negocio de precificacao, estoque, checkout, cupons e pagamento devem seguir estritamente o definido em `Vault/01-Core-Domain/Business-Rules.md`.
5. **Sincronizacao Continua**: Toda alteracao, refatoracao ou commit deve ser documentada no Vault utilizando a skill `vault-sync`.
6. **Diagnostico Legado First**: Mapeie debitos e regras ocultas em `Vault/02-Legacy-Analysis/Debt-And-Bottlenecks.md` antes de reescrever modulos para ASP.NET Core MVC.
7. **Guia Pratico Git**: Para instrucoes detalhadas de criacao de sub-branches (`feat/web/...`), prevencao de `detached HEAD` e regras de push, consulte `GIT-INSTRUCTIONS.md` na raiz do repositorio agregador.
8. **Tratamento de Funcionalidades Incompletas**: Ao identificar funcionalidades ou fluxos incompletos no codigo, nao realize alteracoes arbitrarias; registre no Vault (`02-Legacy-Analysis/Debt-And-Bottlenecks.md` ou na respectiva spec) e notifique a equipe para planejamento formal.
9. **Identidade Mandatoria e Redacao de Commits**: Todos os commits devem obrigatoriamente ser efetuados com o usuario configurado na maquina (`user.name` e `user.email` validos). Caso nao haja usuario configurado, solicite a configuracao formal antes de commitar. E proibido commitar com usuarios temporarios ou de improviso. Alem disso, evite verbos no infinitivo no texto dos commits; priorize o uso de substantivos e formato nominal (ex: "inclusao de...", "implementacao de...", "ajuste em...", "correcao de...").
10. **Verificacao e Realizacao de Push Seguro**: Os pushs devem ser efetuados apos os commits. Antes de realizar o push, verifique o estado do repositorio remoto para assegurar que a operacao e segura. Se for seguro, efetue o push; caso contrario, notifique o usuario com referencia a especificacao para tomada de decisao.
11. **Isolamento de Branches no Vault**: Toda e qualquer documentacao ou sincronizacao com o Vault deve ser realizada exclusivamente na branch correspondente ao fluxo de trabalho atual (`docs/api`, `docs/web`, `docs/desktop`, `docs/mobile`), sendo proibido registrar alteracoes de outras plataformas de forma cruzada. A branch `master` do Vault atua como SSOT consolidada e recebe atualizacoes exclusivamente via Pull Request homologado.
12. **Mapa de Referencias Cruzadas e Impacto**: Antes de alterar modelos de dados, endpoints ou regras centrais, consulte obrigatoriamente `Vault/00-Meta/Cross-Reference-Map.md` para identificar dependencias de entrada (`depends_on`) e sistemas impactados (`impacts`). Ao concluir alteracoes com impacto em outros subsistemas, registre os documentos dependentes em `docs_to_sync` e atualize a matriz de impacto do mapa antes do commit.
