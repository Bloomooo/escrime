# Plan de test — Système de notation du tournoi d'escrime fantastique

## 1. Identification
- Projet : Système de notation pour un tournoi d'escrime fantastique
- Version : 1.1 (v1.0 : exigences du domaine REQ-E-001 à REQ-E-014 ;
  v1.1 : exécution complète, ajout de H5 — coquille du sujet sur WWLW —,
  correction de la valeur épinglée de TC-016 (350 → 305, séries
  contiguës entre blocs), exigences API REQ-E-015..017 exécutées)
- Auteurs : Yanis Mechta, Eddin Adou
- Date : 2026-06-11
- Statut : Exécuté — 40/40 tests verts

## 2. Périmètre

### In scope
- Classe `ScoreCalculator` : méthode `CalculateScore(List<MatchResult> matches,
  bool isDisqualified = false, int penaltyPoints = 0)`
- Classes `MatchResult` (enum `Result { Win, Draw, Loss }`) et `Player`
- Classe `TournamentRanking` : `GetRanking`, `GetChampion`
- Classe `TournamentService` (annonce du champion avec notification),
  testée en isolation via Moq (`IPlayerRepository`, `INotificationService`)
- Phase d'évolution (v1.1) : API REST (joueurs, combats, pénalités,
  disqualification, classement) persistée en SQLite via EF Core

### Out of scope
- Interface utilisateur / application web (le front sera développé
  séparément à partir de la documentation Swagger de l'API)
- Authentification / autorisation
- Envoi réel de notifications (seule une implémentation de log est fournie ;
  le contrat `INotificationService` est testé via mock)

## 3. Stratégie
- Méthodologie : Test-Driven Development (Red - Green - Refactor)
- Un cycle complet par exigence : commit du test rouge
  (`test(REQ-E-xxx): [ROUGE] NomDuTest (TC-xxx)`), puis commit du code
  minimal qui le fait passer (`feat(REQ-E-xxx): [VERT] ...`), puis
  refactor éventuel (tests toujours verts).
- Framework : xUnit + FluentAssertions ; Moq pour l'isolation des
  dépendances de `TournamentService` ; tests paramétrés via
  `[Theory]` + `[InlineData]` (cas simples) et `[MemberData]`
  (listes de `MatchResult`, non exprimables en InlineData).
- Chaque exigence est tracée dans les tests via
  `[Trait("Requirement", "REQ-E-xxx")]` et chaque cas par un
  commentaire `// TC-xxx`.
- Nommage des tests : `MethodName_Scenario_ExpectedResult`
- Couverture cible : 100 % sur `ScoreCalculator` (exigence du sujet : >= 95 %)

## 4. Critères d'entrée
- Spécifications validées (règles du tournoi, exemples 1 à 5 du sujet)
- Squelettes `MatchResult` et `ScoreCalculator` fournis par le sujet

## 5. Critères de sortie
- 100 % des exigences couvertes par au moins un cas de test
- Tous les tests verts (>= 15 tests unitaires exigés par le sujet)
- Couverture de lignes >= 95 % sur `ScoreCalculator`

## 6. Environnement
- .NET 9 (SDK 9.0.310)
- xUnit 2.9.2, FluentAssertions 8.3.0, Moq 4.20.72
- coverlet.collector 6.0.2 (couverture de code, format Cobertura)
- CI : GitHub Actions (ubuntu-latest), exécution des tests à chaque push
  et publication de la couverture dans le résumé du run

## 7. Exigences

Rappel des règles métier (sujet) :
- Victoire = +3 points ; Match nul = +1 point ; Défaite = 0 point
- Bonus de série : +5 points par série de 3 victoires consécutives ou plus
  (accordé une seule fois par série ; une série est cassée par un nul ou
  une défaite)
- Disqualification : score final = 0, quelles que soient les performances
- Pénalités : soustraites du score ; le score final n'est jamais négatif
- `matches` null → `ArgumentNullException` ; pénalités négatives →
  `ArgumentException` ; liste vide → 0 point

| Exigence  | Règle |
|-----------|-------|
| REQ-E-001 | Barème de base : Win = 3, Draw = 1, Loss = 0, score = somme |
| REQ-E-002 | Bonus de +5 par série de >= 3 victoires consécutives, une fois par série, cassée par Draw ou Loss |
| REQ-E-003 | Joueur disqualifié → score final = 0 |
| REQ-E-004 | Les pénalités sont soustraites du score |
| REQ-E-005 | Le score final n'est jamais négatif (plancher à 0) |
| REQ-E-006 | Aucun combat (liste vide) → 0 point |
| REQ-E-007 | `matches` null → `ArgumentNullException` (paramètre `matches`) |
| REQ-E-008 | Pénalités négatives → `ArgumentException` |
| REQ-E-009 | Robustesse : tournoi long (100 combats, pattern complexe) calculé correctement |
| REQ-E-010 | `GetRanking` trie les joueurs par score décroissant |
| REQ-E-011 | Égalité de score : ordre d'entrée préservé (tri stable, cf. H2) |
| REQ-E-012 | `GetChampion` retourne le joueur au meilleur score |
| REQ-E-013 | Tous les joueurs disqualifiés : classement à 0, champion = premier joueur (cf. H3) ; liste vide → null |
| REQ-E-014 | `TournamentService.AnnounceChampion` notifie le champion via `INotificationService` (exactement une fois) ; aucune notification s'il n'y a aucun joueur |
| REQ-E-015 | API : CRUD joueurs (création, lecture, liste, suppression) — v1.1 |
| REQ-E-016 | API : enregistrement des combats, pénalités, disqualification ; score calculé à la volée — v1.1 |
| REQ-E-017 | API : classement décroissant et champion — v1.1 |
| REQ-E-018 | Déroulé du score (`CalculateBreakdown` + GET score-breakdown) : événements match / streakBonus / penalty / clampToZero / disqualification avec score courant, rejouable par le front sans recalcul — v1.2 |
| REQ-E-019 | API : un joueur disqualifié ne peut plus enregistrer de combat (409) — v1.2 |

## 8. Cas de test prévus

### ScoreCalculator — barème de base (REQ-E-001)

TC-001 — Calcul simple sans bonus
  Entrée  : [Win, Draw, Loss]
  Attendu : 4 (3 + 1 + 0)
  Test    : CalculateScore_WinDrawLoss_Returns4Points

TC-002 — Combinaisons de base (paramétré, InlineData)
  Entrée  : [Win, Win] / [Draw, Draw, Draw] / [Loss, Loss] / [Win, Draw, Loss, Win]
  Attendu : 6 / 3 / 0 / 7 (exemple 1 du sujet)
  Test    : CalculateScore_BasicCombinations_ReturnsExpectedScore

### ScoreCalculator — bonus de série (REQ-E-002)

TC-003 — Trois victoires consécutives
  Entrée  : [Win, Win, Win]
  Attendu : 14 (9 + 5)
  Test    : CalculateScore_ThreeConsecutiveWins_Adds5PointsBonus

TC-004 — Quatre victoires consécutives : bonus accordé une seule fois
  Entrée  : [Win, Win, Win, Win]
  Attendu : 17 (12 + 5)
  Test    : CalculateScore_FourConsecutiveWins_AddsBonusOnlyOnce

TC-005 — Série interrompue par une défaite : pas de bonus
  Entrée  : [Win, Win, Loss, Win]
  Attendu : 9 (3+3+0+3, aucune série de 3 victoires — cf. H5 : le sujet
            annonce 6, incompatible avec son propre barème)
  Test    : CalculateScore_StreakBrokenByLoss_NoBonus

TC-006 — Série interrompue par un nul : pas de bonus
  Entrée  : [Win, Draw, Win, Win]
  Attendu : 10 (cas 9 du sujet : « Bonus NON accordé »)
  Test    : CalculateScore_StreakBrokenByDraw_NoBonus

TC-007 — Séries multiples (paramétré, MemberData)
  Entrée  : [W,W,W,L,W,W,W,W] → 31 (21 + 5 + 5, exemple 3 du sujet, cf. H1)
            [W,L,W,W,W] → 17 (12 + 5, bonus pour les 3 dernières victoires)
            [W,W,W,L,W,W,W] → 28 (18 + 5 + 5)
  Test    : CalculateScore_MultipleWinStreaks_AddsBonusPerStreak

### ScoreCalculator — disqualification (REQ-E-003)

TC-008 — Disqualifié avec score positif
  Entrée  : [Win, Win, Win], isDisqualified = true
  Attendu : 0
  Test    : CalculateScore_DisqualifiedWithPositiveScore_ReturnsZero

TC-009 — Disqualifié sans combats
  Entrée  : [], isDisqualified = true
  Attendu : 0
  Test    : CalculateScore_DisqualifiedWithoutMatches_ReturnsZero

### ScoreCalculator — pénalités (REQ-E-004, REQ-E-005)

TC-010 — Pénalités normales
  Entrée  : [Win, Win, Draw, Win] (10 pts), penaltyPoints = 3
  Attendu : 7
  Test    : CalculateScore_PenaltiesSubtracted_ReturnsReducedScore

TC-011 — Pénalités supérieures au score
  Entrée  : [Win, Draw, Draw] (5 pts), penaltyPoints = 8
  Attendu : 0 (jamais négatif)
  Test    : CalculateScore_PenaltiesExceedScore_ReturnsZero

TC-012 — Pénalités égales au score
  Entrée  : [Win, Win, Draw] (7 pts), penaltyPoints = 7
  Attendu : 0
  Test    : CalculateScore_PenaltiesEqualScore_ReturnsZero

### ScoreCalculator — cas limites (REQ-E-006 à REQ-E-009)

TC-013 — Liste vide
  Entrée  : []
  Attendu : 0
  Test    : CalculateScore_EmptyMatches_ReturnsZero

TC-014 — Liste null
  Entrée  : null
  Attendu : ArgumentNullException, paramètre « matches », message « *cannot be null* »
  Test    : CalculateScore_NullMatches_ThrowsArgumentNullException

TC-015 — Pénalités négatives
  Entrée  : [Win], penaltyPoints = -5
  Attendu : ArgumentException
  Test    : CalculateScore_NegativePenalties_ThrowsArgumentException

TC-016 — Très long tournoi (100 combats, pattern complexe)
  Entrée  : 10 répétitions du bloc [W,W,W,L,W,D,W,W,W,W] (100 combats)
  Attendu : 305 — base 8×3+1 = 25 par bloc (250) ; bonus : la série finale
            d'un bloc (WWWW) se poursuit dans le bloc suivant (WWW), donc
            bloc 1 = 2 séries (+10) et blocs 2..10 = 1 nouvelle série
            chacun (+45). Vérifié à la main : 250 + 55 = 305.
  Test    : CalculateScore_HundredMatchesComplexPattern_ReturnsPinnedScore

### TournamentRanking (REQ-E-010 à REQ-E-013)

TC-017 — Classement par score décroissant
  Entrée  : 3 joueurs avec scores distincts (14, 4, 0)
  Attendu : ordre décroissant des scores
  Test    : GetRanking_PlayersWithDifferentScores_SortsByScoreDescending

TC-018 — Égalité de score : ordre d'entrée préservé (H2)
  Entrée  : 2 joueurs ex æquo + 1 joueur derrière
  Attendu : les ex æquo restent dans leur ordre d'entrée
  Test    : GetRanking_TiedScores_PreservesInsertionOrder

TC-019 — Champion = meilleur score
  Entrée  : 3 joueurs, scores 4 / 14 / 7
  Attendu : le joueur à 14
  Test    : GetChampion_MultiplePlayers_ReturnsHighestScorer

TC-020 — Tous les joueurs disqualifiés (H3)
  Entrée  : 3 joueurs tous disqualifiés (scores 0)
  Attendu : classement complet à 0, champion = premier joueur de la liste
  Test    : GetChampion_AllPlayersDisqualified_ReturnsFirstPlayerWithZeroScore

TC-021 — Aucun joueur (H3)
  Entrée  : []
  Attendu : GetChampion → null ; GetRanking → liste vide
  Test    : GetChampion_EmptyPlayers_ReturnsNull

### TournamentService avec Moq (REQ-E-014)

TC-022 — Le champion est notifié exactement une fois
  Entrée  : IPlayerRepository mocké retournant 2 joueurs (champion « Aria », 14 pts)
  Attendu : INotificationService.NotifyChampion("Aria", 14) vérifié Times.Once ;
            la méthode retourne le champion
  Test    : AnnounceChampion_WithPlayers_NotifiesChampionExactlyOnce

TC-023 — Aucun joueur : aucune notification
  Entrée  : IPlayerRepository mocké retournant une liste vide
  Attendu : NotifyChampion jamais appelé (Times.Never), retour null
  Test    : AnnounceChampion_NoPlayers_DoesNotNotify

### API REST (REQ-E-015 à REQ-E-017) — v1.1, tests d'intégration

TC-101 — POST /api/players → 201 + Location + corps correct
TC-102 — POST /api/players nom vide → 400
TC-103 — GET /api/players/{id} inconnu → 404
TC-104 — DELETE /api/players/{id} → 204, puis GET → 404
TC-105 — GET /api/players → liste des joueurs créés
TC-110 — 4 POST matches Win puis GET joueur → score 17 (le bonus de série
          traverse la pile HTTP + EF : prouve la préservation de l'ordre)
TC-111 — POST match avec result invalide ("Banana") → 400
TC-112 — POST penalties : score réduit ; points négatifs → 400
TC-113 — POST disqualification → score 0
TC-120 — GET /api/ranking → trié par score décroissant
TC-121 — GET /api/ranking/champion → meilleur joueur
TC-122 — GET /api/ranking/champion sans joueur → 404

### Déroulé du score (REQ-E-018, REQ-E-019) — v1.2

TC-030 — CalculateBreakdown [W,W,W,D] → match(3), match(6), match(9),
          streakBonus(14), match(15) ; score final 15 (scénario spec front §5)
TC-031 — pénalité sous zéro → penalty (score courant négatif) puis clampToZero(0)
TC-032 — disqualifié → le déroulé raconte les combats puis disqualification(0)
TC-033 — cohérence : FinalScore = CalculateScore et dernier événement = score final
          (théorie, 6 scénarios dont série cassée et liste vide)
TC-034 — garde-fous identiques à CalculateScore (null → ArgumentNullException,
          pénalité négative → ArgumentException)
TC-130 — GET score-breakdown (série + nul + pénalité) → séquence exacte des types
          et scores courants à travers HTTP + EF
TC-131 — GET score-breakdown joueur inconnu → 404
TC-132 — GET score-breakdown joueur disqualifié → finit par disqualification, score 0
TC-140 — POST match sur un disqualifié → 409, aucune trace dans les annales

## 9. Matrice de traçabilité

| Exigence  | Cas de test            | Statut       |
|-----------|------------------------|--------------|
| REQ-E-001 | TC-001, TC-002         | OK (vert)    |
| REQ-E-002 | TC-003 à TC-007        | OK (vert)    |
| REQ-E-003 | TC-008, TC-009         | OK (vert)    |
| REQ-E-004 | TC-010                 | OK (vert)    |
| REQ-E-005 | TC-011, TC-012         | OK (vert)    |
| REQ-E-006 | TC-013                 | OK (vert)    |
| REQ-E-007 | TC-014                 | OK (vert)    |
| REQ-E-008 | TC-015                 | OK (vert)    |
| REQ-E-009 | TC-016                 | OK (vert)    |
| REQ-E-010 | TC-017                 | OK (vert)    |
| REQ-E-011 | TC-018                 | OK (vert)    |
| REQ-E-012 | TC-019                 | OK (vert)    |
| REQ-E-013 | TC-020, TC-021         | OK (vert)    |
| REQ-E-014 | TC-022, TC-023         | OK (vert)    |
| REQ-E-015 | TC-101 à TC-105 (v1.1) | OK (vert)    |
| REQ-E-016 | TC-110 à TC-113 (v1.1) | OK (vert)    |
| REQ-E-017 | TC-120 à TC-122 (v1.1) | OK (vert)    |
| REQ-E-018 | TC-030 à TC-034, TC-130 à TC-132 (v1.2) | OK (vert) |
| REQ-E-019 | TC-140 (v1.2)          | OK (vert)    |

## 10. Hypothèses et risques

Hypothèses (ambiguïtés du sujet, figées ici) :

- H1 — Le sujet est incohérent sur le pattern [W,W,W,L,W,W,W,W] :
  l'exemple détaillé n° 3 donne 31 (21 + 5 + 5, « un bonus par série »),
  mais le cas de test n° 8 de la liste annonce 26 (21 + 5). Les deux sont
  incompatibles. On retient la règle énoncée et l'exemple détaillé :
  un bonus de +5 **par série** de >= 3 victoires consécutives, soit 31.
  Le « 26 » est supposé être une coquille. À valider avec le formateur.
- H2 — En cas d'égalité de score, le sujet ne définit pas d'ordre : on
  retient un tri stable (ordre d'entrée préservé), comportement garanti
  par `OrderByDescending` de LINQ.
- H3 — `GetChampion` sur une liste vide retourne null ; si tous les
  joueurs sont à 0 (ex. tous disqualifiés), le premier joueur de la
  liste est champion (conséquence du tri stable H2).
- H4 — Les gardes de validation (`matches` null, pénalités négatives)
  s'appliquent **avant** le court-circuit de disqualification : un appel
  invalide lève toujours une exception, même pour un joueur disqualifié.
- H5 — Le cas n° 7 de la liste de tests du sujet annonce
  « Win, Win, Loss, Win → 6 points », mais le barème du même sujet donne
  3+3+0+3 = 9 (6 correspondrait à [Win, Win, Loss] sans le dernier
  combat). On retient 9, conforme au barème. Coquille supposée, à
  valider avec le formateur.

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|------------|
| H1 : 26 vs 31 — interprétation contraire du correcteur | Moyenne | Moyen | Hypothèse documentée, logique du bonus isolée, un seul test (TC-007) à ajuster |
| Bonus déclenché à chaque victoire au-delà de 3 (>= au lieu de ==) | Moyenne | Élevé | TC-004 (WWWW → 17, pas 22) verrouille le comportement |
| Le nul ne casse pas la série dans une implémentation naïve | Moyenne | Élevé | TC-006 ([W,D,W,W] → 10) verrouille le comportement |
| Ordre des combats non garanti une fois persistés en base (v1.1) | Haute | Élevé | Lecture systématiquement ordonnée (OrderBy sur l'Id auto-incrémenté) ; TC-110 vérifie le bonus à travers la pile HTTP + EF |
| Score stocké en base et désynchronisé | Moyenne | Élevé | Le score n'est jamais stocké : toujours recalculé par ScoreCalculator à la projection |
| Sur-conception en phase Green | Moyenne | Moyen | Discipline : code minimal, chaque comportement exige son test rouge préalable |
