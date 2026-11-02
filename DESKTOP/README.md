# Modulo Desktop (Deprecado)

> **Status**: Deprecado / Obsoleto  
> **Substituicao**: O cliente Desktop (Windows, macOS, Linux) foi unificado na aplicacao Flutter multiplataforma localizada no diretorio `MOBILE/`.  
> **Branch de Origem**: `feat/mobile`

---

## 1. Diretriz de Governanca

A aplicacao cliente para desktop nao mais reside de forma isolada neste diretorio. Toda a logica de interface, comunicacao com a API RESTful e suporte multiplataforma foi consolidada na aplicacao Flutter em `MOBILE/`, a qual possui configuracao nativa para o sistema operacional Windows sob `MOBILE/windows/`.

## 2. Documentacao de Referencia no Vault

- `Vault/03-Systems/Desktop/README.md`
- `Vault/03-Systems/Mobile/README.md`
- `Vault/00-Meta/Cross-Reference-Map.md`
