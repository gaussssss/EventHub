# EventHub, API Web (.NET 8, Clean Architecture)

Backend REST d'EventHub. **Spécification** : [../docs/BACKEND_MANIFEST.md](../docs/BACKEND_MANIFEST.md).

## Architecture (Clean Architecture)

```
api/
├── src/
│   ├── EventHub.Domain/          # Entités + règles métier (aucune dépendance)
│   ├── EventHub.Application/     # CQRS : commandes/requêtes + handlers, DTOs, interfaces (→ Domain)
│   ├── EventHub.Infrastructure/  # EF Core (SQLite), Identity, dépôts, médiateur (→ Application)
│   └── EventHub.Api/             # Contrôleurs, SignalR, DI (→ Application + Infra)
└── tests/
    ├── EventHub.UnitTests/       # Tests unitaires (domaine, TDD)
    └── EventHub.IntegrationTests/# Tests d'intégration (WebApplicationFactory + SQLite in-memory)
```

## CQRS (couche Application)

Chaque cas d'usage est un message **`ICommand<T>`** (écriture) ou **`IQuery<T>`**
(lecture) traité par un unique `ICommandHandler` / `IQueryHandler`. Les contrôleurs
ne dépendent que de **`ISender`** (médiateur maison, `Infrastructure/Messaging/Sender.cs`),
qui résout le handler par type et lui délègue l'exécution. Les handlers sont
**découverts et enregistrés automatiquement** par scan de l'assembly Application
(voir `AddApplicationHandlers` dans `DependencyInjection.cs`), aucun registre à tenir.
Pas de dépendance externe (ni MediatR) : dispatch par réflexion.

### Modèle de domaine (DDD tactique)

Les entités sont des **agrégats riches** : setters privés, construction via
**fabriques** `Entity.Create(...)` (gardes d'invariants → `DomainException`), et
mutations par **méthodes métier** (`Registration.Cancel/PromoteFromWaitlist/MarkAttended`,
`Activity.Update`, `Post.Hide`, `Report.Resolve`…). Impossible d'instancier un état
invalide.

**Le Domaine possède tous les ports** (hexagonal) et **ne dépend de rien** :
- `Domain/Repositories/`, dépôts d'écriture (agrégats) **et de lecture** (read-repos) ;
- `Domain/ReadModels/`, DTOs retournés par les read-repos (`ActivityDto`, `PostDto`…) ;
- `Domain/Services/`, ports techniques `IClock`, `ICurrentUser`, `IRealtimeNotifier`, `IUserAdminService`.

`Infrastructure` (EF, `SystemClock`, `UserAdminService`) et `Api` (`CurrentUser`,
`SignalRNotifier`) n'implémentent **que** des interfaces du Domaine. Le dispatcher CQRS
(`ISender`/`Sender`) est de la plomberie applicative → il vit dans `Application`. Restent
en Application les **résultats assemblés** (`RegistrationResult`, `HeartsSummaryDto`,
`ProfileDto`…), qui ne sont pas des read models de dépôt.

### Concurrence (dernière place)

L'invariant de capacité est protégé par **concurrence optimiste** : `Activity` porte
un jeton `Version` (`IsConcurrencyToken`). S'inscrire consomme une place via
`Activity.ClaimSpot(...)` (nouveau `Version`) ; deux demandes simultanées sur la
dernière place **entrent en conflit sur la même ligne**, la perdante reçoit une
`ConcurrencyConflictException` (traduite depuis EF dans `RegistrationRepository`),
est **rejouée** par le handler (jusqu'à 3 fois) et bascule en liste d'attente.
Jamais de sur-réservation. Couvert par `RegisterForActivityHandlerTests` (rejeu
déterministe : l'in-memory SQLite mono-connexion ne permet pas un vrai test parallèle).

### Organisation par fonctionnalité

Chaque fonctionnalité (`Registrations/`, `Social/`, `Moderation/`, `Users/`, …) est
découpée en sous-dossiers **un type = un fichier** ; le namespace reste au niveau de
la fonctionnalité (ex. `EventHub.Application.Social`) quel que soit le sous-dossier.

```
Application/<Feature>/
  Commands/     # records ICommand<T>
  Queries/      # records IQuery<T>
  Handlers/     # ICommandHandler / IQueryHandler
  Dtos/         # DTOs, résultats (Result) et enums de retour
  Abstractions/ # interfaces de dépôts/services de la fonctionnalité
Application/Common/
  Messaging/    # contrats CQRS (ICommand, IQuery, ISender…)
  Interfaces/   # abstractions transverses (IClock, ICurrentUser, IRealtimeNotifier)
```

## Stack

- **.NET 8**, ASP.NET Core (contrôleurs) + **SignalR** (`/hubs/notifications`).
- **EF Core 8 + SQLite** (`Microsoft.EntityFrameworkCore.Sqlite`), schéma géré par
  **migrations** (`InitialCreate`), appliquées au démarrage (`Database.Migrate()`).
- **ASP.NET Core Identity** (`IdentityCore` + rôles, clé `Guid`).
- **Auth Microsoft Entra ID** : JWT Bearer **actif dès que la config est fournie**
  (`Authentication:Authority` / `Audience`, via **user-secrets** en dev), avec
  provisioning JIT des utilisateurs et rôles en base. Sans config, repli sur le
  schéma dev (`X-User-Id`), utilisé par les tests.
- Tests : **xUnit**, **FluentAssertions**, **Moq**, `Microsoft.AspNetCore.Mvc.Testing`.

## Commandes

```bash
cd api
dotnet build            # compiler la solution
dotnet test             # tous les tests (unit + intégration)
dotnet run --project src/EventHub.Api   # lancer l'API (migrations appliquées au démarrage)

# Migrations EF (schéma dans src/EventHub.Infrastructure/Persistence/Migrations) :
dotnet ef migrations add <Nom> --project src/EventHub.Infrastructure --startup-project src/EventHub.Api --output-dir Persistence/Migrations
```

## Endpoints déjà implémentés

| Méthode | Route | Description |
|---|---|---|
| GET | `/api/activities` | Catalogue des activités publiées (+ `currentParticipants`) |
| GET | `/api/activities/featured` | Activités « à la une » |
| GET | `/api/activities/{id}` | Détail d'une activité (404 si absente) |
| POST | `/api/activities/{id}/register` | S'inscrire → `registered` / `waitlisted` (liste d'attente si complet), idempotent, `409` si échéance passée |
| POST | `/api/activities/{id}/cancel` | Annuler → promeut le 1ᵉʳ en liste d'attente, `404` si non inscrit |
| GET | `/api/me` | Profil : nom, courriel, avatar, total cœurs, niveau, inscriptions |
| GET | `/api/me/hearts` | Résumé cœurs de l'utilisateur : total, niveau (Bronze/Argent/Or), historique |
| POST · PUT | `/api/admin/activities` · `/{id}` | Back office : créer / mettre à jour une activité (statut, « à la une ») |
| POST | `/api/admin/activities/{id}/attendance` | Marque des présents → **crédite les cœurs** (idempotent) |
| GET | `/api/posts` · `/api/posts/{id}` | Fil communautaire / détail (auteur, likes, commentaires) |
| POST | `/api/posts` | Publier une photo (`imageUrl`, `caption`, `activityId?`) → `201` |
| POST · DELETE | `/api/posts/{id}/like` | Aimer / retirer (idempotent) → `{ likesCount }` |
| POST | `/api/posts/{id}/comments` | Commenter (`text`) |
| POST | `/api/reports` | Signaler un post/commentaire (`targetType`, `targetId`, `reason`) → `201`, `404` si absent |
| GET | `/api/admin/reports` | Back office : file des signalements ouverts (auteur, cible, motif) |
| POST | `/api/admin/posts/{id}/hide` | Masquer un post → disparaît du fil + clôt ses signalements |
| POST | `/api/admin/comments/{id}/hide` | Masquer un commentaire → disparaît du détail |
| GET | `/api/admin/users` | Back office : rechercher des utilisateurs (`?q=`), rôle, statut, cœurs |
| PATCH | `/api/admin/users/{id}` | Changer le rôle (`student`/`organizer`/`moderator`/`admin`) et/ou le statut |
| POST | `/api/admin/users/{id}/hearts` | Ajustement manuel de cœurs (`hearts`, `reason`) → `{ totalHearts }` |
| POST | `/api/activities/{id}/check-in` | **Auto-émargement par QR** : jeton secret + inscrit + fenêtre horaire → présence confirmée, cœurs crédités (idempotent) |
| GET | `/api/me/registrations` | Mes activités inscrites (avec `myStatus` : registered/attended/noshow/waitlisted) |
| POST | `/api/me/avatar` | Persiste la photo de profil (chemin renvoyé par l'upload) |
| POST | `/api/uploads/image` | Upload d'image (multipart) → chemin **relatif** `/uploads/…` (jamais de domaine en base) |
| GET | `/api/categories` · `/api/leaderboard` · `/api/stats/community` | Référentiels, classement et stats publics |
| GET | `/api/about/contributors` | Contributeurs de la page « À propos » (tri par ordre) |
| CRUD | `/api/admin/contributors` | Back office : gestion des contributeurs |
| CRUD | `/api/admin/categories` · `/api/admin/organizers` | Back office : référentiels |
| GET/PATCH | `/api/admin/settings/gamification` | Seuils de niveaux (Argent/Or) |
| POST | `/api/admin/notifications/broadcast` | Diffusion d'une notification |
| WS | `/hubs/notifications` | Hub SignalR, évènements `activityParticipantsChanged`, `registrationPromoted` |

> Réponses en **camelCase**, conformes au contrat figé par l'app mobile
> ([BACKEND_MANIFEST.md §3.0](../docs/BACKEND_MANIFEST.md)).
>
> **Identité** : `POST /register` lit l'utilisateur depuis le claim Entra `oid` ;
> tant que l'auth n'est pas active, repli sur l'en-tête `X-User-Id` (dev/tests).

## Transverse (production-readiness)

- **Santé** : `GET /health` → `{ status, version }` (sonde + connectivité base).
- **Erreurs** : gestionnaire global → **ProblemDetails** (RFC 7807), invariants de
  domaine → 400, conflit de concurrence → 409, reste → 500 (détail interne journalisé,
  non divulgué).
- **CORS** : politique `Default` pilotée par `Cors:AllowedOrigins` (tout autoriser si vide, dev).
- **Rate limiting** : fenêtre fixe par IP (`RateLimiting:PermitPerMinute`, défaut 600) → 429.
- **Observabilité** : journalisation des requêtes HTTP (`AddHttpLogging`).
- **Secrets** : `appsettings` < **user-secrets** (dev, `UserSecretsId`) < variables
  d'environnement ; aucune valeur sensible en dur (connexion, Authority, stockage).

## Authentification (Microsoft Entra ID, Option B)

Le contrôle d'accès est **en place et testé**, avec deux modes :

- **Mode dev (par défaut)** : schéma `Dev`, l'identité vient des en-têtes
  `X-User-Id` (id interne) et `X-User-Roles` (rôles, séparés par des virgules).
  Permet à `[Authorize]` de fonctionner en tests/dev. **Ne pas utiliser en prod.**
- **Mode Entra** : dès que `Authentication:Authority` est renseigné, l'API **valide
  les jetons Entra** (JWKS) et un **middleware de provisioning JIT** mappe le claim
  `oid` → un `ApplicationUser` interne (`EntraObjectId`), en le créant au 1ᵉʳ login.
  `CurrentUser` renvoie toujours le **Guid interne**.

Routes protégées : `[Authorize]` sur `/api/me/*` (utilisateur connecté) et
`[Authorize(Roles=…)]` sur `/api/admin/*` + `/attendance` (rôles
`organizer`/`moderator`/`admin` selon la route). Les lectures publiques
(catalogue, catégories, fil, leaderboard, stats, health) restent ouvertes.

**Activer l'auth Entra** (les identifiants Entra ne sont pas des secrets en Option B) :
```bash
cd src/EventHub.Api
dotnet user-secrets set "Authentication:Authority" "https://login.microsoftonline.com/<tenantId>/v2.0"
dotnet user-secrets set "Authentication:Audience"  "api://<clientId-API>"
# Restrictions (optionnelles mais recommandées) :
dotnet user-secrets set "Authentication:TenantId"           "<tenantId>"
dotnet user-secrets set "Authentication:AllowedEmailDomain" "uqtr.ca"
```
Renseigner `Authority` bascule automatiquement du mode dev vers Entra (le schéma
`Dev` et le repli `X-User-Id` se désactivent). L'app mobile (MSAL/PKCE) demande la
scope `api://<clientId-API>/access_as_user` et envoie l'access token en
`Authorization: Bearer …`.

## Reste à faire (itérations suivantes)

**Auth Entra** : **active en dev** (config `Authority`/`Audience`/`AdminEmails`
fournie via user-secrets), validation des jetons + provisioning JIT + rôles DB.
Voir la section *Authentification* ci-dessus.

**Non encore implémenté** (hors périmètre de cette itération) :
- **Loi 25** : `GET /me/export`, `DELETE /me` (anonymisation), registre de consentement.
- **Webhook Google Forms** (`POST /webhooks/google-forms`) pour la réconciliation.
- **Push réel** (FCM/APNs) : l'adaptateur actuel (`LoggingPushSender`) est un stub ;
  le **stockage** objet (`StubStorageService`) fabrique des URL sans provisionner S3.
- **Jobs CRON** (clôture des inscriptions à l'échéance, rappels push, recalcul `app_stats`).

Le reste du manifeste (catalogue filtré, catégories, profil, inscriptions + statut,
leaderboard, stats, back-office activités/référentiels/utilisateurs/modération,
dashboard + export CSV, notifications, uploads, réglages gamification) est **implémenté
et couvert par des tests** (unit + intégration).
