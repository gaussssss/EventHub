# Manifeste technique, Back-office (Angular 20)

> Guide de maintenance de l'administration d'**UQTR en santé**. Décrit
> l'architecture et les écrans **tels qu'implémentés**.
> Commandes de lancement/build : [RUN_AND_BUILD.md](RUN_AND_BUILD.md).
> Conventions de code : [../backoffice/ARCHITECTURE.md](../backoffice/ARCHITECTURE.md).

- **Rôle** : interface d'administration, consomme l'API (`/api/admin/*`).
- **Accès** : réservé aux rôles `organizer` / `moderator` / `admin`.

---

## 1. Stack & architecture

| Élément | Choix |
|---|---|
| Framework | **Angular 20** standalone (aucun NgModule) |
| État | **`signal` + `inject`** (pas de store externe), Clean Architecture par feature |
| UI | **Tailwind v4 + daisyUI 5**, thème maison **Azure/Fluent** (clair + sombre), UI plate |
| Icônes | **Fluent** via Iconify (`icon-[fluent--<nom>]`) |
| QR | **qrcode** (génération du QR d'émargement) |
| Auth | **Microsoft Entra** (SPA) via `@azure/msal-browser`, **connexion par redirection** (pas de popup), access token Bearer |

**Organisation par feature** :
`features/<feature>/{components, models, services/{application, infrastructure/{repository, states}}}`.
Les composants lisent des `states` (signaux) et déclenchent des cas d'usage
(`application/*`) qui appellent les `repository` (HTTP).

---

## 2. Écrans & fonctionnalités

| Section | Contenu |
|---|---|
| **Tableau de bord** | KPIs, export CSV des inscriptions, (dev) bouton **« Seed dev data »** |
| **Activités** | CRUD (créer/éditer/archiver), publier / annuler / mettre à la une, organisateur, **coût de participation** (informatif), **lien d'inscription obligatoire** (validation d'URL), **image par URL ou téléversement depuis le poste**, **contraintes d'édition** (champs requis, fin après début, échéance avant début) |
| **Inscrits & présence** | Liste + marquage de présence (crédite les cœurs), **KPI cliquables qui filtrent la liste** + recherche, **QR d'émargement** (modale à projeter sur place) |
| **Utilisateurs & rôles** | Recherche, changement de rôle/statut, ajustement manuel de cœurs |
| **Classement** | Leaderboard global paginé + **recherche par nom** |
| **Modération** | File de signalements avec **prévisualisation du contenu** + confirmation avant masquage |
| **Catégories** | CRUD ; **slug généré automatiquement** depuis le libellé (champ masqué) |
| **Organisateurs** | CRUD |
| **Paramètres** | Seuils de gamification (Argent/Or) + **contributeurs de la page « À propos »** (tableau éditable : ordre, nom, rôle, avatar) |

> La section **Notifications** est **masquée du menu** (la route et l'API
> existent toujours ; ré-ajouter l'entrée dans `shared/components/layout/layout.ts`
> pour la réactiver).

---

## 3. Points techniques notables

- **Fuseau horaire** : les champs `datetime-local` sont saisis/affichés en heure
  **locale** et convertis en **UTC** aux frontières du formulaire d'activité
  (envoi `new Date(local).toISOString()`, édition UTC → local). L'API stocke et
  sert de l'UTC.
- **Images** : les uploads renvoient un **chemin relatif** (`/uploads/…`) ; un
  résolveur (`core/utils/media-url.ts`) les ré-ancre sur l'origine API pour
  l'affichage (et « répare » d'anciennes URLs absolues à hôte périmé).
- **QR d'émargement** : généré côté client à partir du `checkInToken` de
  l'activité, payload `uqtrsante://checkin?a=<id>&k=<token>`.
- **Logo officiel** sur la page de connexion et la barre supérieure.

---

## 4. Configuration

`src/environments/environment.development.ts` (gitignoré) : `apiUrl` +
`entra.{tenantId, clientId, apiScope, redirectUri}`. L'app SPA doit être
enregistrée dans Entra en plateforme **Single-page application** (redirect
`http://localhost:4200`) avec permission déléguée sur la scope de l'API. Voir
[RUN_AND_BUILD.md](RUN_AND_BUILD.md).

L'autorisation admin repose sur les **rôles en base** ; le premier admin est
amorcé côté API via `Authentication:AdminEmails`.
