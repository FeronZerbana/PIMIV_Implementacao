# Regras de Governança do Projeto (Implementacao)

> O repositório de documentação oficial e fonte única da verdade (SSOT) está localizado em `Vault/`.

---

## 1. Regras Mandatórias de Desenvolvimento

1. **Proibido Código Sem Spec**: Antes de criar ou modificar qualquer código nos diretórios `WEB/`, `DESKTOP/` ou `MOBILE/`, consulte e valide a respectiva especificação no Vault (`04-Specs-Backlog/Active/` ou `03-Systems/`).
2. **Proibição Estrita de Emojis**: É terminantemente proibido o uso de emojis em arquivos de documentação, especificações, ADRs, mensagens de commit e comentários de código. Toda a comunicação técnica deve ser formal, sóbria e profissional.
3. **Branching Model**:
   - `feat/web`: Alterações e desenvolvimento do sistema Web.
   - `feat/desktop`: Alterações e desenvolvimento do sistema Desktop.
   - `feat/mobile`: Alterações e desenvolvimento do sistema Mobile.
   - `develop`: Branch de integração e testes conjuntos.
   - `production`: Branch estável de entrega final consolidada.
4. **Core Domain Unificado**: Todas as regras de negócio de precificação, estoque, checkout, cupons e pagamento devem seguir estritamente o definido em `Vault/01-Core-Domain/Business-Rules.md`.
5. **Sincronização Contínua**: Toda alteração, refatoração ou commit deve ser documentada no Vault utilizando a skill `vault-sync`.
6. **Diagnóstico Legado First**: Mapeie débitos e regras ocultas em `Vault/02-Legacy-Analysis/Debt-And-Bottlenecks.md` antes de reescrever módulos para ASP.NET Core MVC.
7. **Guia Prático Git**: Para instruções detalhadas de criação de sub-branches (`feat/web/...`), prevenção de `detached HEAD` e regras de push, consulte `GIT-INSTRUCTIONS.md` na raiz do repositório agregador.
8. **Tratamento de Funcionalidades Incompletas**: Ao identificar funcionalidades ou fluxos incompletos no código, não realize alterações arbitrárias; registre no Vault (`02-Legacy-Analysis/Debt-And-Bottlenecks.md` ou na respectiva spec) e notifique a equipe para planejamento formal.
9. **Identidade Mandatória de Commits**: Todos os commits devem obrigatoriamente ser efetuados com o usuário configurado na máquina (`user.name` e `user.email` válidos). Caso não haja usuário configurado, solicite a configuração formal antes de commitar. É proibido commitar com usuários temporários ou de improviso.
10. **Verificação e Realização de Push Seguro**: Os pushs devem ser efetuados após os commits. Antes de realizar o push, verifique o estado do repositório remoto para assegurar que a operação é segura. Se for seguro, efetue o push; caso contrário, notifique o usuário com referência à especificação para tomada de decisão.


