# Tournoi d'escrime fantastique — Système de notation

[![CI](https://github.com/Bloomooo/escrime/actions/workflows/ci.yml/badge.svg)](https://github.com/Bloomooo/escrime/actions/workflows/ci.yml)
[![Couverture lignes](https://bloomooo.github.io/escrime/badge_linecoverage.svg)](https://bloomooo.github.io/escrime/)
[![Couverture branches](https://bloomooo.github.io/escrime/badge_branchcoverage.svg)](https://bloomooo.github.io/escrime/)

TP : système de notation pour un tournoi d'escrime fantastique.
Domaine métier (calcul de score, classement) développé en **TDD**
(xUnit + FluentAssertions + Moq), exposé via une **API REST**
(ASP.NET Core, **SQLite + EF Core**, **Swagger**), avec CI GitHub Actions.

## Structure

- `src/Escrime.Domain` — règles métier : `ScoreCalculator`, `TournamentRanking`, `TournamentService`
- `src/Escrime.Api` — API REST + persistance SQLite (EF Core) + Swagger
- `tests/Escrime.UnitTests` — 28 tests unitaires (xUnit, FluentAssertions, Moq)
- `tests/Escrime.IntegrationTests` — 12 tests d'intégration (WebApplicationFactory + SQLite in-memory)
- `docs/TEST_PLAN.md` — plan de test (rédigé avant le code) ; `docs/TEST_REPORT.md` — rapport d'exécution
- `.github/workflows/ci.yml` — CI : tests + couverture publiée

## Règles du tournoi

Victoire +3, nul +1, défaite 0 ; **bonus +5 par série de ≥ 3 victoires
consécutives** (une fois par série, cassée par un nul ou une défaite) ;
disqualification → score 0 ; pénalités soustraites, score plancher à 0.
Les ambiguïtés du sujet sont documentées en hypothèses H1–H5 (TEST_PLAN §10).

## Lancer les tests

```bash
dotnet test                                        # 40 tests (28 unitaires + 12 intégration)
dotnet test --collect:"XPlat Code Coverage"        # + couverture Cobertura
```

Couverture mesurée : **ScoreCalculator 100 % lignes / 100 % branches**
(exigence du sujet : ≥ 95 %).

## Lancer l'API

```bash
dotnet run --project src/Escrime.Api
```

- **Swagger UI : http://localhost:5000/swagger** (port selon `dotnet run`,
  affiché au démarrage) — contrat complet pour développer le front.
- Base SQLite `escrime.db` créée/migrée automatiquement au démarrage.
- CORS ouvert (`AllowAnyOrigin`) : un front local (Vite, React, etc.)
  peut appeler l'API directement. À restreindre en production.
- Les enums transitent en chaîne (`"Win"`, `"Draw"`, `"Loss"`).

## Endpoints

| Verbe | Route | Description | Réponses |
|---|---|---|---|
| POST | `/api/players` `{"name": "Sir Galahad"}` | Inscrire un joueur | 201 + Location, 400 |
| GET | `/api/players` | Liste avec scores calculés | 200 |
| GET | `/api/players/{id}` | Détail + combats | 200, 404 |
| DELETE | `/api/players/{id}` | Désinscrire | 204, 404 |
| POST | `/api/players/{id}/matches` `{"result": "Win"}` | Enregistrer un combat (refusé si disqualifié) | 201, 400, 404, 409 |
| GET | `/api/players/{id}/score-breakdown` | Déroulé du score pour la reconstitution front | 200, 404 |
| POST | `/api/players/{id}/penalties` `{"points": 3}` | Infliger une pénalité | 200, 400, 404 |
| POST | `/api/players/{id}/disqualification` | Disqualifier | 200, 404 |
| GET | `/api/ranking` | Classement décroissant avec rangs | 200 |
| GET | `/api/ranking/champion` | Champion | 200, 404 |

Exemple de scénario :

```bash
curl -X POST localhost:5000/api/players -H "Content-Type: application/json" -d '{"name":"Sir Galahad"}'
for i in 1 2 3 4; do curl -X POST localhost:5000/api/players/1/matches -H "Content-Type: application/json" -d '{"result":"Win"}'; done
curl localhost:5000/api/players/1          # → "score": 17 (12 + 5 de bonus de série)
curl localhost:5000/api/ranking/champion
```

Le score n'est **jamais stocké** : il est recalculé par `ScoreCalculator`
à chaque lecture (les combats sont relus dans l'ordre chronologique,
le bonus de série en dépend).

## CI et couverture en ligne

À chaque push : build Release, 40 tests, couverture agrégée
(ReportGenerator) affichée dans le résumé du run + rapport HTML en
artifact. Sur `main`, le rapport complet et les badges de couverture
sont publiés sur GitHub Pages :
**https://bloomooo.github.io/escrime/**

(Prérequis une seule fois : Settings → Pages → Source = « GitHub Actions ».)

## Démarche TDD

L'historique git matérialise le cycle rouge/vert : chaque exigence
`REQ-E-xxx` a un commit `test(REQ-E-xxx): [ROUGE] NomDuTest (TC-xxx)`
(échec montré) puis `feat(REQ-E-xxx): [VERT] ...` (code minimal).
Traçabilité : `[Trait("Requirement", "REQ-E-xxx")]` sur chaque test,
matrice exigences ↔ cas de test dans `docs/TEST_PLAN.md`.
