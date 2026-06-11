# Rapport d'exécution des tests — Tournoi d'escrime fantastique

## 1. Identification
- Projet : Système de notation pour un tournoi d'escrime fantastique
- Référence : plan de test v1.1 (docs/TEST_PLAN.md)
- Auteurs : Yanis Mechta, Eddin Adou
- Date d'exécution : 2026-06-11
- Environnement : .NET SDK 9.0.310 (Linux), xUnit 2.9.2,
  FluentAssertions 8.3.0, Moq 4.20.72, coverlet.collector 6.0.2,
  EF Core 9.0.5 (SQLite), Microsoft.AspNetCore.Mvc.Testing 9.0.5

## 2. Synthèse

| Campagne | Tests | Verts | Rouges | Durée |
|---|---|---|---|---|
| Unitaires (Escrime.UnitTests) | 28 | 28 | 0 | ~0,5 s |
| Intégration API (Escrime.IntegrationTests) | 12 | 12 | 0 | ~3 s |
| **Total** | **40** | **40** | **0** | |

- 100 % des exigences REQ-E-001 à REQ-E-017 couvertes et vérifiées
  (matrice de traçabilité : TEST_PLAN §9).
- Critère du sujet « minimum 15 tests » largement dépassé (28 tests
  unitaires, dont 2 Theory paramétrées InlineData + MemberData).
- Méthodologie TDD respectée : chaque exigence a son couple de commits
  `test(REQ-E-xxx): [ROUGE]` / `feat(REQ-E-xxx): [VERT]` dans
  l'historique git (preuve rouge → vert).

## 3. Couverture de code (unitaires, format Cobertura)

| Classe | Lignes | Branches |
|---|---|---|
| ScoreCalculator | **100 %** | **100 %** |
| TournamentRanking | 100 % | 100 % |
| TournamentService | 100 % | 100 % |
| Player | 100 % | n/a |
| MatchResult | 83 % | 100 % |

- Exigence du sujet : >= 95 % sur ScoreCalculator → **atteinte (100 %)**.
- MatchResult à 83 % : seul le constructeur par défaut (fourni par le
  squelette du sujet pour la désérialisation) n'est pas exercé par les
  tests unitaires — sans impact sur la logique métier.

## 4. Résultats par exigence

| Exigence | Cas | Verdict | Remarque |
|---|---|---|---|
| REQ-E-001 barème | TC-001, TC-002 | ✅ | |
| REQ-E-002 bonus de série | TC-003..007 | ✅ | H1 appliquée : WWWLWWWW → 31 |
| REQ-E-003 disqualification | TC-008, TC-009 | ✅ | |
| REQ-E-004 pénalités | TC-010 | ✅ | |
| REQ-E-005 plancher zéro | TC-011, TC-012 | ✅ | |
| REQ-E-006 liste vide | TC-013 | ✅ | |
| REQ-E-007 null | TC-014 | ✅ | ArgumentNullException, param `matches` |
| REQ-E-008 pénalités négatives | TC-015 | ✅ | ArgumentException |
| REQ-E-009 100 combats | TC-016 | ✅ | valeur épinglée 305 (cf. TEST_PLAN) |
| REQ-E-010 tri décroissant | TC-017 | ✅ | |
| REQ-E-011 égalités | TC-018 | ✅ | H2 : tri stable |
| REQ-E-012 champion | TC-019 | ✅ | |
| REQ-E-013 tous disqualifiés / vide | TC-020, TC-021 | ✅ | H3 |
| REQ-E-014 notification (Moq) | TC-022, TC-023 | ✅ | Times.Once / Times.Never |
| REQ-E-015 API CRUD joueurs | TC-101..105 | ✅ | |
| REQ-E-016 API combats/pénalités/DQ | TC-110..113 | ✅ | TC-110 : 4 Wins via HTTP → 17 |
| REQ-E-017 API classement | TC-120..122 | ✅ | |

## 5. Vérification de bout en bout (manuelle)

API lancée en réel (SQLite sur fichier, migration automatique) puis
scénario complet déroulé via curl :
création joueur → 201 ; 4 victoires → score **17** (bonus de série à
travers HTTP + EF) ; pénalité 3 → 14 ; classement trié avec rangs ;
champion correct ; disqualification → 0 ; résultat invalide → 400.
Swagger UI accessible sur `/swagger` (HTTP 200) et CORS actif
(`Access-Control-Allow-Origin: *`) pour le développement du front.

## 6. Hypothèses appliquées (ambiguïtés du sujet)

- **H1** : WWWLWWWW → 31 (un bonus par série), le « 26 » du sujet est
  une coquille. À valider avec le formateur.
- **H2** : ex æquo dans l'ordre d'entrée (tri stable).
- **H3** : champion null si aucun joueur ; premier joueur si tous à 0.
- **H4** : validation des arguments avant le court-circuit de
  disqualification.
- **H5** : WWLW → 9 (le « 6 » du sujet contredit son propre barème).

## 7. CI

Workflow GitHub Actions (`.github/workflows/ci.yml`) : build Release,
exécution des 40 tests, couverture Cobertura agrégée par ReportGenerator,
résumé Markdown publié dans le job summary et rapport HTML en artifact.
S'exécute à chaque push et pull request dès que le repo est poussé
sur GitHub.
