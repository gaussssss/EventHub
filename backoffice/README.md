# EventHub — Back Office

Interface d'administration d'EventHub (Angular 20). Consomme l'[API Web](../api)
(routes `/api/admin/*`). Réservée aux rôles `organizer` / `moderator` / `admin`.

## Stack

- **Angular 20** standalone (aucun NgModule)
- **État par `signal` + `inject`** (pas de store externe) — Clean Architecture par feature
- **Tailwind v4 + daisyUI 5** — thème maison **Azure/Fluent** (clair + sombre), UI **plate** (bordures, pas d'ombres)
- **Icônes Fluent** via Iconify (`@iconify-json/fluent` + `@iconify/tailwind4`) : `icon-[fluent--<nom>]`
- **Auth Microsoft Entra** (SPA) via `@azure/msal-browser` (Option B : access token Bearer)

Conventions d'architecture détaillées : [ARCHITECTURE.md](ARCHITECTURE.md).

## Fonctionnalités (manifeste §4)

- **Tableau de bord** — KPIs, export CSV des inscriptions, (dev) bouton *Seed*
- **Activités** — CRUD (créer/éditer/archiver), publier / annuler / mettre à la une,
  association d'un organisateur, **inscrits + présence** (crédite les cœurs),
  **statistiques par activité** (remplissage, présence, no-show)
- **Utilisateurs & rôles** — recherche, changement de rôle / statut, ajustement de cœurs
- **Classement** — leaderboard global paginé
- **Modération** — file de signalements avec **prévisualisation du contenu**
  (post/commentaire) et confirmation avant masquage
- **Catégories / Organisateurs** — CRUD des référentiels
- **Notifications** — diffusion d'une notification (titre / message)
- **Paramètres** — seuils de gamification (Bronze/Argent/Or) + récompense de présence

## Démarrer en développement

Prérequis : Node 22+, l'[API](../api) lancée sur `http://localhost:5199`.

```bash
npm install
npm start          # ng serve → http://localhost:4200
```

### Configuration

`src/environments/environment.development.ts` :

- `apiUrl` — base de l'API (défaut `http://localhost:5199/api`)
- `entra.tenantId` / `entra.clientId` — **app registration SPA** du back-office
  (plateforme *Single-page application*, redirect URI `http://localhost:4200`,
  permission déléguée sur la scope de l'API). Tant que `clientId` est vide, le
  bouton de connexion indique « Entra non configuré ».
- `entra.apiScope` — `api://<clientId-API>/access_as_user`

> **Autorisation admin** : l'API lit les rôles depuis la base ; le premier admin
> est amorcé via `Authentication:AdminEmails` (voir l'API). Une fois connecté
> avec un email listé, tu accèdes aux routes `/admin/*`.

## Thème

Bouton de bascule **clair / sombre** dans la barre supérieure. Le choix est
persisté (`localStorage`) et retombe sur la préférence système au premier
lancement (thèmes daisyUI `azure` / `azure-dark`).

## Données de démonstration (dev uniquement)

Le **Tableau de bord** affiche, hors production, un bouton **« Seed dev data »**
qui réinitialise puis régénère un jeu réaliste (faux users, activités,
inscriptions, cœurs, posts/commentaires/likes, signalements). Les faux
utilisateurs n'ont **aucune identité Microsoft** (`EntraObjectId = null`) : ils
ne peuvent pas se connecter, ils n'existent que comme données. Un reseed
n'efface **que** les données taguées (`@seed.local`, slug `seed-`) — jamais les
vrais utilisateurs du tenant.

## Build & tests

```bash
npm run build      # build de production
npm test           # tests unitaires (Karma + Jasmine)
```

Pour lancer les tests en headless :

```bash
npm test -- --watch=false --browsers=ChromeHeadless
```
