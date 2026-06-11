# Tournoi d'escrime fantastique — Système de notation

TP : système de notation pour un tournoi d'escrime fantastique.
Domaine métier (calcul de score, classement) développé en TDD avec
xUnit + FluentAssertions + Moq, puis exposé via une API REST
(ASP.NET Core, SQLite + EF Core, Swagger).

## Structure

- `src/Escrime.Domain` — règles métier (ScoreCalculator, TournamentRanking, TournamentService)
- `src/Escrime.Api` — API REST + persistance SQLite (EF Core) + Swagger
- `tests/Escrime.UnitTests` — tests unitaires (xUnit, FluentAssertions, Moq)
- `tests/Escrime.IntegrationTests` — tests d'intégration de l'API
- `docs/TEST_PLAN.md` — plan de test (rédigé avant le code)

## Lancer les tests

```bash
dotnet test
```

(Sections lancement de l'API, Swagger et couverture complétées au fil du TP.)
