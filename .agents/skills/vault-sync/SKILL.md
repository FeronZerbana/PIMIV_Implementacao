---
name: vault-sync
description: >-
  Sincronizacao obrigatoria de documentacao, rastreabilidade de commits, refatoracoes, 
  decisoes arquiteturais e especificacoes tecnicas com o repositorio Vault (Single Source of Truth). 
  Ative sempre que planejar, codificar, refatorar ou commitar alteracoes nos projetos Web, Desktop ou Mobile.
---

# Skill: Sincronização e Governança com o Vault (SSOT)

Esta skill estabelece o protocolo operacional para manter o **Vault** 100% sincronizado com toda e qualquer atividade no código-fonte dos sistemas (**Web**, **Desktop** e **Mobile**).

---

## 1. Diretrizes de Estilo e Padronização Documental

* **Proibição de Emojis**: Não utilize emojis em nenhum documento, cabeçalho, tabela, diagrama, mensagem de commit ou comentário de código.
* **Tom Técnico e Profissional**: A documentação deve manter escrita formal, impessoal, clara e concisa.
* **Formatos Padronizados**: Toda especificação deve derivar de `00-Meta/Templates/SPEC-TEMPLATE.md` e toda decisão arquitetural de `00-Meta/Templates/ADR-TEMPLATE.md`.

---

## 2. Quando Acionar Esta Skill

Esta skill deve ser acionada em qualquer um dos seguintes cenários:
1. **Nova Demanda ou Feature**: Antes de escrever qualquer linha de código, verificar se há uma especificação ativa no Vault. Se não houver, redigir a especificação técnica seguindo `00-Meta/Templates/SPEC-TEMPLATE.md` e salvar em `04-Specs-Backlog/Draft/` -> `Active/`.
2. **Alteração ou Refatoração de Código**: Sempre que arquivos forem alterados, refatorados ou adicionados em `WEB/`, `DESKTOP/` ou `MOBILE/`, registrar as mudanças técnicas no Vault.
3. **Criação / Atualização de Contratos de API**: Atualizar os schemas em `01-Core-Domain/API-Contracts/`.
4. **Decisões Arquiteturais e Pesquisas**: Criar um documento de pesquisa em `05-Architecture-Knowledge/Research/` ou um ADR em `05-Architecture-Knowledge/ADRs/`.
5. **Conclusão de Tarefas (Commits / Merges)**: Atualizar o status da spec para `04-Specs-Backlog/Completed/` e registrar o changelog.

---

## 3. Procedimento Passo a Passo

### Passo 1: Validação Pré-Código (Proibido Código Sem Spec)
1. Navegue até o Vault e posicione-se na branch correspondente à sua frente de trabalho (`docs/api`, `docs/web`, `docs/desktop`, `docs/mobile`).
2. Verifique se existe uma especificação para a tarefa em:
   - `04-Specs-Backlog/Active/` ou `03-Systems/<Web|Desktop|Mobile>/`
3. Caso a especificação não exista:
   - **PARE** a geração de código.
   - Crie a especificação a partir do template `00-Meta/Templates/SPEC-TEMPLATE.md` (sem emojis).
   - Salve em `04-Specs-Backlog/Draft/` e obtenha validação antes de mover para `Active/`.

### Passo 2: Verificação de Regras de Domínio Unificadas
1. Consulte `01-Core-Domain/Business-Rules.md` para garantir que o cálculo, validação ou fluxo que está sendo implementado segue as regras universais.
2. Se houver regras específicas de plataforma (ex: views Razor no Web, push notifications no Mobile, portas seriais de impressora no Desktop), certifique-se de que estão documentadas em `03-Systems/<Sistema>/`.

### Passo 3: Implementação e Testes na Branch Correta
1. Execute a implementação na branch correspondente (`feat/web`, `feat/desktop`, `feat/mobile`, `feat/api`).
2. Garanta a criação e execução dos testes unitários e de integração pertinentes.

### Passo 4: Registro Pós-Implementação no Vault
1. No repositório do Vault, certifique-se de estar posicionado na branch correspondente (`docs/api`, `docs/web`, `docs/desktop`, `docs/mobile`).
2. Se a feature foi concluída com sucesso:
   - Mova o arquivo de especificação de `04-Specs-Backlog/Active/` para `04-Specs-Backlog/Completed/`.
3. Registre as alterações realizadas usando o template `00-Meta/Templates/CHANGELOG-TEMPLATE.md`.
4. Se foram identificados novos débitos ou regras implícitas no legado durante o processo, registre imediatamente referenciando o subsistema correspondente.
5. Realize o commit e o push seguro exclusivamente para a respectiva branch `docs/*` no Vault. Abra Pull Request para integração na branch `master` ao homologar a entrega.
