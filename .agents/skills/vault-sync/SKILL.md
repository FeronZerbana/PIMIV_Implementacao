---
name: vault-sync
description: >-
  Sincronizacao obrigatoria de documentacao, rastreabilidade de commits, refatoracoes, 
  decisoes arquiteturais, mapa de referencias cruzadas e especificacoes tecnicas com o repositorio Vault (Single Source of Truth). 
  Ative sempre que planejar, codificar, refatorar ou commitar alteracoes nos projetos Web, Desktop ou Mobile.
---

# Skill: Sincronizacao e Governanca com o Vault (SSOT)

Esta skill estabelece o protocolo operacional para manter o **Vault** 100% sincronizado com toda e qualquer atividade no codigo-fonte dos sistemas (**Web**, **Desktop** e **Mobile**), garantindo o rastreamento continuo de dependencias e impactos cruzados.

---

## 1. Diretrizes de Estilo e Padronizacao Documental

* **Proibicao de Emojis**: Nao utilize emojis em nenhum documento, cabecalho, tabela, diagrama, mensagem de commit ou comentario de codigo.
* **Tom Tecnico e Profissional**: A documentacao deve manter escrita formal, impessoal, clara e concisa.
* **Formatos Padronizados com Frontmatter YAML**: Toda especificacao deve derivar de `00-Meta/Templates/SPEC-TEMPLATE.md`, toda decisao arquitetural de `00-Meta/Templates/ADR-TEMPLATE.md`, contendo obrigatoriamente metadados de dependencias (`depends_on`, `impacts`, `criticality`, `docs_to_sync`).

---

## 2. Quando Acionar Esta Skill

Esta skill deve ser acionada em qualquer um dos seguintes cenarios:
1. **Nova Demanda ou Feature**: Antes de escrever qualquer linha de codigo, verificar se ha uma especificacao ativa no Vault. Se nao houver, redigir a especificacao tecnica seguindo `00-Meta/Templates/SPEC-TEMPLATE.md` e salvar em `04-Specs-Backlog/Draft/` -> `Active/`.
2. **Alteracao ou Refatoracao de Codigo**: Sempre que arquivos forem alterados, refatorados ou adicionados em `WEB/`, `DESKTOP/` ou `MOBILE/`, registrar as mudancas tecnicas no Vault.
3. **Mapeamento e Avaliacao de Impacto Cruzado**: Consultar `00-Meta/Cross-Reference-Map.md` antes de modificar codigo para mapear dependencias de entrada e sistemas afetados.
4. **Criacao / Atualizacao de Contratos de API**: Atualizar os schemas em `01-Core-Domain/API-Contracts/` e sinalizar em `docs_to_sync` as plataformas consumidoras.
5. **Decisoes Arquiteturais e Pesquisas**: Criar um documento de pesquisa em `05-Architecture-Knowledge/Research/` ou um ADR em `05-Architecture-Knowledge/ADRs/`.
6. **Conclusao de Tarefas (Commits / Merges)**: Atualizar o status da spec para `04-Specs-Backlog/Completed/`, preencher `impacts` / `docs_to_sync`, atualizar o mapa de impacto e registrar o changelog.

---

## 3. Procedimento Passo a Passo (Protocolo do Duplo Check)

### Passo 1: Validacao Pre-Codigo e Analise de Impacto (Check 1)
1. Navegue ate o Vault e posicione-se na branch correspondente a sua frente de trabalho (`docs/api`, `docs/web`, `docs/desktop`, `docs/mobile`).
2. **Consulte obrigatoriamente `00-Meta/Cross-Reference-Map.md`**:
   - Localize o modulo que sera alterado na matriz hierarquica.
   - Identifique quais subsistemas dependem dele (`depended_by` / `impacts`).
   - Avalie a criticidade (`P0` a `P3`): alteracoes P0 (contratos e banco) ou P1 (regras de negocio) exigem planejamento de compatibilidade retroativa.
3. Verifique se existe uma especificacao para a tarefa em:
   - `04-Specs-Backlog/Active/` ou `03-Systems/<Web|Desktop|Mobile>/`
4. Caso a especificacao nao exista:
   - **PARE** a geracao de codigo.
   - Crie a especificacao a partir do template `00-Meta/Templates/SPEC-TEMPLATE.md` com frontmatter completo.
   - Salve em `04-Specs-Backlog/Draft/` e obtenha validacao antes de mover para `Active/`.

### Passo 2: Verificacao de Regras de Dominio Unificadas
1. Consulte `01-Core-Domain/Business-Rules.md` para garantir que o calculo, validacao ou fluxo que esta sendo implementado segue as regras universais.
2. Se houver regras especificas de plataforma (ex: views Razor no Web, push notifications no Mobile, portas seriais de impressora no Desktop), certifique-se de que estao documentadas em `03-Systems/<Sistema>/`.

### Passo 3: Implementacao e Testes na Branch Correta
1. Execute a implementacao na branch correspondente (`feat/web`, `feat/desktop`, `feat/mobile`, `feat/api`).
2. Garanta a criacao e execucao dos testes unitarios e de integracao pertinentes.

### Passo 4: Registro Pos-Implementacao e Sincronizacao do Grafo (Check 2)
1. No repositorio do Vault, certifique-se de estar posicionado na branch correspondente (`docs/api`, `docs/web`, `docs/desktop`, `docs/mobile`).
2. Atualize o frontmatter da nota alterada:
   - Registre os subsistemas impactados no campo `impacts`.
   - Se houver pendencia de atualizacao em outras plataformas (ex: Mobile precisa consumir novo campo da API), registre os links das notas pendentes no campo `docs_to_sync`.
   - Atualize `last_updated` e `updated_by`.
3. Se a feature foi concluida com sucesso:
   - Mova o arquivo de especificacao de `04-Specs-Backlog/Active/` para `04-Specs-Backlog/Completed/`.
4. Atualize a matriz estatica em `00-Meta/Cross-Reference-Map.md`.
5. Registre as alteracoes realizadas usando o template `00-Meta/Templates/CHANGELOG-TEMPLATE.md` do subsistema correspondente.
6. Realize o commit e o push seguro exclusivamente para a respectiva branch `docs/*` no Vault. Abra Pull Request para integracao na branch `master` ao homologar a entrega.
