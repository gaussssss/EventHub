# Manifeste technique, Backend (API .NET 8)

> Guide de maintenance du backend d'**UQTR en santé** (EventHub). Décrit
> l'architecture, le modèle de données et les endpoints **tels qu'implémentés**.
> Commandes de lancement/build : [RUN_AND_BUILD.md](RUN_AND_BUILD.md).
> Détails d'architecture logicielle : [../api/README.md](../api/README.md).

- **Rôle** : source de vérité unique, consommée par l'app mobile et le back-office.
- **Public** : étudiants et personnel UQTR (comptes Microsoft 365 du tenant).
- **Conventions** : JSON **camelCase**, dates **ISO-8601 UTC** (affichage local
  délégué aux clients), auth `Authorization: Bearer <JWT>`, erreurs en
  **ProblemDetails** (RFC 7807). Base des routes : `/api/…`.

---

## 1. Stack & architecture

| Élément | Choix |
|---|---|
| Runtime | **.NET 8**, ASP.NET Core (contrôleurs) |
| Architecture | **Clean Architecture** : `Domain` → `Application` → `Infrastructure` → `Api` |
| CQRS | Médiateur **maison** `ISender` (pas de MediatR) ; handlers découverts par scan d'assembly |
| Domaine | Agrégats riches (DDD) : fabriques `Create`, setters privés, invariants via `Guard`/`DomainException` |
| Persistance | **EF Core 8 + SQLite** (`eventhub.db`), migrations appliquées au démarrage |
| Identité | **ASP.NET Core Identity** (`IdentityCore` + rôles, clé `Guid`) |
| Auth | **Microsoft Entra ID** (JWT Bearer) + repli schéma dev (voir §5) |
| Temps réel | **SignalR** (`/hubs/notifications`) |
| Tests | xUnit, FluentAssertions, Moq, `WebApplicationFactory` (SQLite in-memory) |

Découpage des projets :

```
api/src/
├── EventHub.Domain/          Entités, règles métier, ports (repos, services). Aucune dépendance.
├── EventHub.Application/     CQRS : Commands/Queries/Handlers, DTOs de résultat.
├── EventHub.Infrastructure/  EF Core, Identity, dépôts, médiateur, seeder dev.
└── EventHub.Api/             Contrôleurs, SignalR, DI, config, JSON, sécurité.
```

---

## 2. Modèle de données (agrégats EF)

Le schéma réel est celui des **migrations EF** (`Infrastructure/Persistence/Migrations`).
Compteurs (`currentParticipants`, `totalHearts`, `likesCount`…) **non stockés** :
calculés à la lecture.

| Entité | Champs clés |
|---|---|
| **ApplicationUser** (Identity) | Id (Guid), Name, Email, AvatarUrl, Role(s), Status, `EntraObjectId` (null pour comptes seed) |
| **Category** | Id, Slug (unique), Label, Color, Icon |
| **Organizer** | Id, Name, ContactEmail |
| **Activity** | Id, Title, Description, CategoryId, OrganizerId, StartsAt, EndsAt, Location, ImageUrl, HeartsReward, MaxParticipants, **ParticipationCost**, RegistrationUrl, RegistrationDeadline, IsFeatured, Status, **CheckInToken**, Version (concurrence) |
| **Registration** | Id, UserId, ActivityId, Status (`registered`/`waitlisted`/`attended`/`noshow`/`cancelled`), Source, RegisteredAt, AttendedAt |
| **HeartTransaction** | Id, UserId, ActivityId, ActivityTitle (snapshot), Hearts, Reason, CreatedAt |
| **Post / Comment / PostLike** | Fil social (auteur, image, légende, statut de modération) |
| **Report** | Signalement (reporterId, targetType, targetId, reason, status) |
| **Notification / Device** | Notifications in-app + jetons d'appareil |
| **GamificationSettings** | Seuils Argent/Or (+ récompense par défaut, non exposée dans l'UI) |
| **Contributor** | Page « À propos » : Name, Role, AvatarUrl, SortOrder |

**Règles métier notables** :
- **Capacité / liste d'attente** : inscription sur une activité pleine →
  `waitlisted` ; à l'annulation, promotion automatique du premier en attente.
  Protégé par **concurrence optimiste** (`Activity.Version`) contre la
  sur-réservation de la dernière place.
- **Cœurs santé** : crédités **à la présence confirmée**, jamais à la simple
  inscription. Cumul → niveaux Bronze (<200), Argent (≥200), Or (≥500).
  Crédit **idempotent** (pas de double comptage).

---

## 3. Authentification & autorisation

Flux **Option B** (Microsoft Entra ID, OAuth 2.0/OIDC) :

1. Le client (mobile/SPA) se connecte à Entra et obtient un **access token**
   pour la scope de l'API (`api://<API_CLIENT_ID>/access_as_user`).
2. Il l'envoie en `Authorization: Bearer`. L'API **valide** le jeton (signature
   JWKS, `aud`, issuer = tenant configuré).
3. **Provisioning JIT** : au 1er appel, l'utilisateur interne est créé/retrouvé
   depuis le claim `oid` (middleware `EntraProvisioningMiddleware`).
4. **Autorisation par rôles stockés en base** (`student`/`organizer`/
   `moderator`/`admin`), injectés comme claims puis vérifiés par
   `[Authorize(Roles=…)]`. Le premier admin est amorcé via
   `Authentication:AdminEmails`.

Restrictions optionnelles (si configurées) : tenant (`tid`) et domaine de
courriel (`@uqtr.ca`).

---

## 4. API REST (routes implémentées)

> Réponses **camelCase**, conformes au contrat parsé par l'app mobile.

### Public / mobile
```
GET  /api/activities            ?category=&q=&availableOnly=&from=&to=   catalogue filtré
GET  /api/activities/featured                                            « à la une »
GET  /api/activities/{id}                                                détail
POST /api/activities/{id}/register        -> registered | waitlisted (idempotent)
POST /api/activities/{id}/cancel          -> annule, promeut la liste d'attente
POST /api/activities/{id}/check-in        { token }  auto-émargement QR (voir §6)
GET  /api/activities/{id}/registration    statut d'inscription de l'utilisateur
GET  /api/categories · /api/leaderboard · /api/stats/community
GET  /api/about/contributors              page « À propos »

GET  /api/me · PATCH /api/me               profil (nom)
GET  /api/me/hearts · GET /api/me/registrations   (avec myStatus)
POST /api/me/avatar        { avatarUrl }    persiste la photo (chemin déjà uploadé)
GET/PATCH /api/me/notifications · /notification-settings · POST /api/me/devices

GET/POST /api/posts · GET /api/posts/{id}          fil + détail (likes, commentaires)
POST/DELETE /api/posts/{id}/like · POST /api/posts/{id}/comments
POST /api/reports         { targetType, targetId, reason }   signalement

POST /api/uploads/image   multipart 'file'  -> { url: "/uploads/…" }  (voir §6)
```

### Back-office (`/api/admin/*`, rôles requis)
```
GET/POST/PUT /api/admin/activities[/{id}] · /{id}/publish|cancel|feature
GET  /api/admin/activities/{id}/registrations
POST /api/admin/activities/{id}/attendance   { userIds[] }  marque présents + crédite cœurs
GET/POST/PATCH/DELETE /api/admin/categories · /api/admin/organizers · /api/admin/contributors
GET  /api/admin/users ?q= · PATCH /api/admin/users/{id} · POST /api/admin/users/{id}/hearts
GET  /api/admin/reports · POST /api/admin/posts/{id}/hide · /api/admin/comments/{id}/hide
GET/PATCH /api/admin/settings/gamification
POST /api/admin/notifications/broadcast
GET  /api/admin/dashboard/overview · /dashboard/activities/{id} · /exports/registrations.csv
POST /api/admin/dev/seed        (Development uniquement)
WS   /hubs/notifications        événements temps réel (participants, promotion liste d'attente)
```

---

## 5. Sécurité & transverse

- **Schéma d'auth dev** (en-têtes `X-User-Id`/`X-User-Roles`) : actif seulement
  si aucune config Entra, et **uniquement** en environnement Development/Testing.
  Hors de là, l'API **refuse de démarrer** (garde anti-déploiement ouvert).
- **Uploads durcis** : type déterminé par les **octets de signature**
  (JPEG/PNG/WEBP/GIF), jamais par le Content-Type/extension du client ;
  `/uploads` servi avec `X-Content-Type-Options: nosniff` + `Content-Disposition:
  attachment`. Limite 10 Mo. `[Authorize]` requis.
- **ProblemDetails** : invariants domaine → 400, conflit de concurrence → 409,
  reste → 500 (détail interne journalisé, non divulgué).
- **CORS** (`Cors:AllowedOrigins`), **rate limiting** (fenêtre fixe par IP),
  **HttpLogging**, `GET /health`.
- **Secrets** : `appsettings` < user-secrets (dev) < variables d'environnement.
  Rien de sensible en dur dans le dépôt.

---

## 6. Émargement par QR & uploads

**QR de présence** : chaque activité porte un `CheckInToken` (GUID secret,
généré à la création). Le back-office affiche le QR
(`uqtrsante://checkin?a=<activityId>&k=<token>`) ; l'étudiant le scanne dans
l'app → `POST /api/activities/{id}/check-in`. Le serveur valide : jeton correct,
utilisateur **inscrit**, et scan **dans la fenêtre horaire** (2 h avant le début
→ 2 h après la fin, durée par défaut 4 h). Marque `attended` + crédite les cœurs,
sans double crédit.

**Uploads** : stockage **local** sous `wwwroot/uploads`, servi en statique. Les
URLs stockées en base sont **relatives** (`/uploads/…`, jamais de domaine) ;
chaque client les résout contre sa propre base API.

---

## 7. Limites connues (à cadrer avant production)

Points **volontairement stubés ou non réalisés**, importants pour un futur
déploiement :

- **Stockage objet** : les images sont sur le disque local (`wwwroot/uploads`),
  pas sur un blob store/CDN. `POST /api/uploads/sign` (`StubStorageService`)
  fabrique des URLs de substitution sans provisionner S3.
- **Push réel** : `IPushSender` est un stub qui journalise (`LoggingPushSender`) ;
  aucune intégration FCM/APNs. Les notifications sont stockées et diffusées
  in-app uniquement.
- **Base SQLite** : adaptée au dev/démo ; prévoir une base serveur (PostgreSQL…)
  et un stockage objet pour la production.
- **Intégrité de l'émargement** : le QR encode un jeton **statique** (partageable) ;
  la fenêtre horaire limite l'abus mais ne garantit pas la présence physique.
- **Conformité Loi 25** (export/suppression de données, registre de consentement) :
  non implémentée.
