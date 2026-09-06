# collective-api

API REST: .NET Web API + PostgreSQL. Monolito modular; en V1 solo el módulo de contacto.

## Estado

Repositorio recién creado. Todavía no hay solución: la fase 0 está descrita en el plan de este repositorio.

## Stack

- .NET Web API — minimal APIs, cortes verticales por módulo
- PostgreSQL con EF Core (sin Dapper)
- Docker Compose para desarrollo local
- Despliegue: contenedor

## Documentación

Vive fuera del repositorio, en la carpeta de documentación del proyecto:

| Documento | Qué responde |
|---|---|
| `PLAN_BACKEND.md` | Cómo se construye este repositorio, y sus reglas obligatorias |
| `PLAN_GENERAL.md` | Alcance, roadmap y decisiones de producto |

Los ADR propios de este repositorio van en `docs/decisions/`.
El runbook de operación irá en `docs/runbook.md` y la política de datos en `docs/data-protection.md`.

## Alcance de la V1

Un módulo (`Contact`), un endpoint de escritura y una tabla:

```
POST /api/v1/contact   → 202 | 400 | 429
GET  /health/live      → 200
GET  /health/ready     → 200 | 503
```

Cada endpoint que se añada sin una pantalla que lo use es deuda.

## Contrato con el frontend

Este repositorio publica `openapi.json` como artefacto en cada build. `collective-web`
genera sus tipos a partir de él. Todo cambio de contrato se etiqueta `contract` en el PR.

## Cómo se trabaja

- `main` protegida y siempre desplegable.
- Ramas cortas: `feat/`, `fix/`, `chore/`, `docs/`. Vida máxima: 3 días.
- PR obligatorio, una aprobación, checks en verde, squash merge.
- Conventional Commits.
- Código, ramas, commits y ADR en inglés.

---

Software propietario. Todos los derechos reservados.
