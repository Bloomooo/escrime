# Arène d'Acier & d'Arcane — frontend

Front SvelteKit (SPA statique) du tournoi d'escrime fantastique. Les données
viennent de l'API .NET du dépôt ; aucune règle de score n'est calculée ici.

## Démarrer (front + back reliés)

```bash
# terminal 1 : l'API .NET (SQLite cree automatiquement, CORS ouvert)
dotnet run --project ../src/Escrime.Api        # http://localhost:5000

# terminal 2 : le front
cp .env.example .env                           # PUBLIC_API_BASE=http://localhost:5000
bun install
bun run dev                                    # http://localhost:5173
```

Build de production : `bun run build` (sortie statique dans `build/`,
servable par n'importe quel hébergeur statique ; l'API reste à lancer à part).

## Scripts

| Script                              | Rôle                                                         |
| ----------------------------------- | ------------------------------------------------------------ |
| `bun run dev`                       | serveur de développement                                     |
| `bun run build` / `bun run preview` | build statique + aperçu                                      |
| `bun run check`                     | typecheck svelte-check                                       |
| `bun run lint` / `bun run format`   | Prettier + ESLint                                            |
| `bun run test:unit -- --run`        | tests Vitest                                                 |
| `bun run test:coverage`             | tests + couverture (seuils bloquants)                        |
| `bun run test:e2e`                  | smoke Playwright (la CI lance l'API .NET puis teste la pile) |

## Écrans

Arène (hero), Combattants (forge, recherche, pénalités, disqualification),
Duel (animation squelettique, le destin tranche), Reconstitution (rejoue le
breakdown du score servi par `GET /api/players/{id}/score-breakdown`),
Classement (podium, rangs).
