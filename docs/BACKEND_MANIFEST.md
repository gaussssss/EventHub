# EventHub — Manifeste Backend

> Spécification de tout ce que le serveur doit fournir pour l'app Flutter `ca.uqtr.eventhub`.
> Dérivé des entités et écrans réellement présents dans l'app, + les manques à prévoir.

- **Public** : étudiants/personnel UQTR (auth par courriel `@uqtr.ca`).
- **Stack recommandée** : PostgreSQL + API REST (Node/NestJS, Django/DRF ou Laravel), stockage objet S3-compatible (+ CDN), Redis (cache/queues), service de push (FCM/APNs).
- **Conventions API** : `https://api.eventhub.uqtr.ca/v1`, JSON en **`camelCase`** (⚠️ pas `snake_case` — les colonnes BD `snake_case` sont mappées en `camelCase` dans les réponses), dates **ISO‑8601 UTC**, auth `Authorization: Bearer <JWT>`, pagination `?page=&limit=` (réponse `{ data, meta:{ page, limit, total } }`), erreurs `{ error:{ code, message, details } }`.
- **Contrat figé côté app** : les modèles Flutter (`*.fromJson`) attendent déjà des payloads précis — voir §3.0. Toute divergence de nommage casse le parsing.

---

## 1. Authentification & autorisation — **Microsoft Entra ID (Azure AD)**

L'authentification se fait **exclusivement via le compte Microsoft 365 de l'UQTR** (Microsoft Entra ID / Azure AD), en **OAuth 2.0 / OpenID Connect**. Aucun mot de passe n'est stocké par EventHub.

| Élément | Détail |
|---|---|
| Fournisseur | **Microsoft Entra ID** (tenant UQTR). Flux **Authorization Code + PKCE** côté mobile (paquet `msal` / `flutter_appauth` ou `microsoft_graph` via WebView). |
| Restriction | Seuls les comptes du tenant UQTR (domaine `@uqtr.ca`) sont acceptés ; les invités/externes sont rejetés. |
| Échange | L'app obtient un **id_token Microsoft** → l'envoie au backend → le backend le **valide** (signature JWKS Microsoft, `aud`, `iss`, `tid` = tenant UQTR) → émet ses **propres JWT** (access ~15 min + refresh ~30 j). |
| Provisionnement | Création/MAJ automatique du `user` au premier login (nom, courriel, `oid` Microsoft, avatar via Microsoft Graph). |
| Rôles | `student` (défaut), `organizer`, `moderator`, `admin` — gérés dans EventHub (ou mappés depuis des **groupes Entra**). |
| Sécurité | Validation stricte du token Microsoft, révocation refresh token, journal de connexions, déconnexion globale. |

```
# Flux mobile : l'app fait le login Microsoft (PKCE) puis :
POST /auth/microsoft        { idToken }            -> { accessToken, refreshToken, user }
POST /auth/refresh          { refreshToken }        -> { accessToken, refreshToken }
POST /auth/logout           { refreshToken }        -> 204
GET  /auth/me               -> profil courant complet
```

> Détails Entra à fournir à l'app : `tenantId` UQTR, `clientId` (app enregistrée dans Entra),
> `redirectUri` (`ca.uqtr.eventhub://auth`), scopes `openid profile email User.Read`.

---

## 2. Modèle de données (schémas BD — PostgreSQL)

> `currentParticipants`, `totalHearts`, `likesCount`, niveaux… **ne sont pas stockés en dur** : ce sont des agrégats calculés (vues ou compteurs maintenus). L'app les reçoit déjà calculés.

```sql
-- Utilisateurs & profil ----------------------------------------------------
CREATE TABLE users (
  id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name          TEXT NOT NULL,
  email         CITEXT UNIQUE NOT NULL,          -- contrainte domaine @uqtr.ca
  password_hash TEXT,                            -- NULL si SSO
  avatar_url    TEXT,
  role          TEXT NOT NULL DEFAULT 'student', -- student|organizer|moderator|admin
  status        TEXT NOT NULL DEFAULT 'active',  -- active|suspended|deleted
  email_verified_at TIMESTAMPTZ,
  created_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Catégories & organisateurs (aujourd'hui codés en dur dans l'app) ---------
CREATE TABLE categories (
  id     UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  slug   TEXT UNIQUE NOT NULL,        -- 'sport' | 'socioculturel' | ...
  label  TEXT NOT NULL,
  color  TEXT,                        -- code couleur badge
  icon   TEXT
);

CREATE TABLE organizers (
  id     UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name   TEXT NOT NULL,               -- 'Club de course UQTR'
  contact_email TEXT
);

-- Activités ----------------------------------------------------------------
CREATE TABLE activities (
  id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  title                 TEXT NOT NULL,
  description           TEXT NOT NULL,
  category_id           UUID NOT NULL REFERENCES categories(id),
  organizer_id          UUID REFERENCES organizers(id),
  starts_at             TIMESTAMPTZ NOT NULL,         -- 'date' dans l'app
  ends_at               TIMESTAMPTZ,
  location              TEXT NOT NULL,
  image_url             TEXT NOT NULL,
  hearts_reward         INT  NOT NULL DEFAULT 0,      -- 'hearts'
  max_participants      INT  NOT NULL,
  registration_url      TEXT,                         -- lien Google Form
  registration_deadline TIMESTAMPTZ,
  is_featured           BOOLEAN NOT NULL DEFAULT false,-- carrousel "à la une"
  status                TEXT NOT NULL DEFAULT 'published', -- draft|published|cancelled|archived
  created_at            TIMESTAMPTZ NOT NULL DEFAULT now(),
  updated_at            TIMESTAMPTZ NOT NULL DEFAULT now()
);
CREATE INDEX ON activities (starts_at);
CREATE INDEX ON activities (category_id);

-- Inscriptions (+ liste d'attente, évoquée dans l'UI) ----------------------
CREATE TABLE registrations (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id      UUID NOT NULL REFERENCES users(id),
  activity_id  UUID NOT NULL REFERENCES activities(id),
  status       TEXT NOT NULL DEFAULT 'registered', -- registered|waitlisted|attended|cancelled|no_show
  source       TEXT,                               -- 'google_form'|'app'
  form_response_id TEXT,                            -- réconciliation Google Forms
  registered_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  attended_at  TIMESTAMPTZ,
  UNIQUE (user_id, activity_id)
);
-- currentParticipants = COUNT(status IN ('registered','attended'))

-- Gamification : grand livre des cœurs -------------------------------------
CREATE TABLE heart_transactions (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id      UUID NOT NULL REFERENCES users(id),
  activity_id  UUID REFERENCES activities(id),
  activity_title TEXT,                 -- snapshot (heartHistory.activityTitle)
  hearts       INT  NOT NULL,          -- + (gain) ou - (ajustement admin)
  reason       TEXT NOT NULL,          -- 'attendance'|'bonus'|'admin_adjust'
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);
-- totalHearts = SUM(hearts) ; niveaux Bronze<200, Argent<500, Or>=500

-- Réseau social ------------------------------------------------------------
CREATE TABLE posts (
  id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  author_id    UUID NOT NULL REFERENCES users(id),
  activity_id  UUID REFERENCES activities(id),   -- 'activityName' lié
  image_url    TEXT NOT NULL,
  caption      TEXT NOT NULL,
  status       TEXT NOT NULL DEFAULT 'published',-- published|hidden|removed
  created_at   TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE post_likes (
  post_id UUID REFERENCES posts(id),
  user_id UUID REFERENCES users(id),
  created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
  PRIMARY KEY (post_id, user_id)
);
-- likesCount = COUNT(post_likes)

CREATE TABLE comments (
  id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  post_id    UUID NOT NULL REFERENCES posts(id),
  author_id  UUID NOT NULL REFERENCES users(id),
  text       TEXT NOT NULL,
  status     TEXT NOT NULL DEFAULT 'published',
  created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Transverse ---------------------------------------------------------------
CREATE TABLE devices (         -- jetons push
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID REFERENCES users(id),
  push_token TEXT NOT NULL, platform TEXT, created_at TIMESTAMPTZ DEFAULT now()
);
CREATE TABLE notifications (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID REFERENCES users(id),
  type TEXT, title TEXT, body TEXT, data JSONB,
  read_at TIMESTAMPTZ, created_at TIMESTAMPTZ DEFAULT now()
);
CREATE TABLE reports (         -- modération
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  reporter_id UUID REFERENCES users(id),
  target_type TEXT, target_id UUID, reason TEXT,
  status TEXT DEFAULT 'open', created_at TIMESTAMPTZ DEFAULT now()
);
CREATE TABLE app_stats (       -- '1.2k inscrits', total UQTR cœurs (cache)
  key TEXT PRIMARY KEY, value BIGINT, updated_at TIMESTAMPTZ DEFAULT now()
);
```

---

## 3. API REST — toutes les routes

### 3.0 Contrats JSON figés par l'app (⚠️ à respecter)

L'app est déjà « API-ready » : ses modèles désérialisent des payloads **précis**. Le backend doit produire **exactement** ces formes (nommage `camelCase`, `organizer` = **chaîne**, `category` = **slug**, dates ISO‑8601, cœurs sous `heartsReward`).

**Activity** (`GET /activities`, `/activities/featured`, `/activities/:id`) — tel que parsé par `ActivityModel.fromJson` :
```json
{
  "id": "act003",
  "title": "Tournoi de basketball",
  "description": "Tournoi interéquipes en format 3 contre 3...",
  "category": "sport",                     // slug: "sport" | "socioculturel"
  "organizer": "Association sportive UQTR",// chaîne, pas un objet
  "startsAt": "2026-07-11T18:00:00Z",      // ISO-8601 UTC (affiché en heure locale)
  "location": "Complexe sportif Gilles-Côté",
  "imageUrl": "https://.../photo.jpg",
  "heartsReward": 50,                      // (fallback accepté: "hearts")
  "maxParticipants": 60,
  "currentParticipants": 42,               // agrégat calculé côté serveur
  "registrationUrl": "https://forms...",   // nullable
  "registrationDeadline": "2026-07-08T23:59:00Z"  // nullable
}
```

**À aligner (au choix backend, mais à décider) :** `Post` et `UserProfile` n'ont pas encore de `fromJson` figé côté app — leur contrat reste **ouvert**. Champs attendus par l'UI :
- **Post** : `id, authorName, authorAvatarUrl, imageUrl, caption, activityName, createdAt, likesCount, comments[]` (chaque commentaire : `authorName, text, createdAt`).
- **UserProfile** (`GET /me`) : `id, name, email, avatarUrl, totalHearts, completedActivityIds[], heartHistory[]` (chaque entrée : `activityTitle, hearts, date`). Le **niveau** (Bronze/Argent/Or) et les seuils sont **calculés côté app** — inutile de les renvoyer, mais rester cohérent avec §2.

### 3.1 Application mobile

**Profil / utilisateur courant**
```
GET   /me                       -> { id, name, email, avatarUrl, totalHearts,
                                      level, nextLevelThreshold, previousLevelThreshold,
                                      completedActivityIds, registrationCount }
PATCH /me                       { name }                 -> profil mis à jour   (Modifier profil)
POST  /me/avatar                multipart 'file'         -> { avatarUrl }       (Modifier la photo)
GET   /me/hearts                -> { totalHearts, level, history:[{activityTitle,hearts,date}] }
GET   /me/registrations         -> activités inscrites de l'utilisateur (page Profil "Mes activités")
GET   /me/notifications         ; PATCH /me/notifications/:id/read
GET   /me/notification-settings ; PATCH /me/notification-settings
```

**Activités (Accueil + Catalogue + Détail)**
```
GET /activities                 ?category=&q=&availableOnly=&from=&to=&page=&limit=&sort=
                                 -> liste filtrée (recherche titre/lieu, intervalle de dates,
                                    places dispo) — alimente Catalogue & "Activités à venir"
GET /activities/featured        -> 3 activités du carrousel "à la une"
GET /activities/:id             -> détail complet (+ spotsLeft, isRegistered, deadlinePassed)
GET /categories                 -> chips de catégories
GET /activities/:id/share       -> { shareUrl, title }   (bouton Partager / deep link)
```

**Inscription (WebView Google Form)**
```
GET  /activities/:id/registration-url   -> { url }   (à ouvrir en WebView)
POST /activities/:id/register           { formResponseId? }
        -> crée/maj registration (status registered|waitlisted si complet) ; idempotent
POST /activities/:id/cancel             -> annule l'inscription
GET  /activities/:id/registration       -> statut d'inscription de l'utilisateur
```

**Fil communautaire (social)**
```
GET    /posts                   ?page=&limit=          -> fil
GET    /posts/:id               -> post + commentaires
POST   /posts                   multipart { image, caption, activityId? }   (Publier une photo)
DELETE /posts/:id               (auteur ou modérateur)
POST   /posts/:id/like          ; DELETE /posts/:id/like   (toggle ❤️)
GET    /posts/:id/comments      ; POST /posts/:id/comments { text }
POST   /reports                 { targetType, targetId, reason }   (signalement)
```

**Classement / gamification (page Cœurs santé)**
```
GET /leaderboard                ?scope=global&page=  -> [{ rank, name, avatarUrl, hearts, isMe }]
GET /stats/community            -> { totalRegisteredUsers, totalUqtrHearts }  (badges Accueil)
```

**Upload média (avatars, photos)**
```
POST /uploads/sign              { type, contentType } -> { uploadUrl, fileUrl }  (presigned S3)
```

### 3.2 Back office (préfixe `/admin`, rôles `organizer`/`moderator`/`admin`)
```
# Activités
GET/POST/PATCH/DELETE /admin/activities[/:id]
POST  /admin/activities/:id/publish | /cancel | /feature   (toggle "à la une")
GET   /admin/activities/:id/registrations   -> liste inscrits + liste d'attente
POST  /admin/activities/:id/attendance      { userIds[] }  -> marque présents + crédite cœurs
# Référentiels
GET/POST/PATCH/DELETE /admin/categories[/:id]
GET/POST/PATCH/DELETE /admin/organizers[/:id]
# Utilisateurs
GET   /admin/users ?q= ; PATCH /admin/users/:id  { role, status }
POST  /admin/users/:id/hearts  { hearts, reason }   (ajustement manuel)
# Modération
GET   /admin/reports ; POST /admin/posts/:id/hide ; POST /admin/comments/:id/hide
# Gamification & contenu
GET/PATCH /admin/settings/gamification   (seuils Bronze/Argent/Or, règles de cœurs)
POST  /admin/notifications/broadcast     { audience, title, body }
# Tableaux de bord
GET   /admin/dashboard/overview          (KPIs)
GET   /admin/dashboard/activities/:id    (taux de participation, no-show…)
GET   /admin/exports/registrations.csv
```

---

## 4. Fonctionnalités du back office

1. **Gestion des activités** : CRUD, brouillon→publication, annulation, archivage, **mise « à la une »** (carrousel), capacité, date limite, récompense en cœurs, lien Google Form, image.
2. **Référentiels** : catégories (aujourd'hui figées à Sport/Socioculturel) et organisateurs gérés en base.
3. **Inscriptions & présence** : voir inscrits + **liste d'attente**, **marquer les présences** (= déclencheur d'attribution des cœurs), export CSV.
4. **Gamification** : configurer seuils de niveaux et règles d'attribution, ajustements manuels de cœurs, voir le classement.
5. **Modération sociale** : file de signalements, masquer/supprimer posts & commentaires, suspendre un utilisateur.
6. **Utilisateurs & rôles** : recherche, attribution de rôles (organizer/moderator/admin), suspension.
7. **Notifications** : campagnes push ciblées (rappels d'échéance, nouveautés).
8. **Tableaux de bord** : utilisateurs actifs, activités populaires, taux de remplissage, no-show, distribution des cœurs.

---

## 5. Services transverses & tâches planifiées

- **Stockage objet + CDN** pour avatars et photos (URL pré-signées, redimensionnement, anti-virus optionnel).
- **Push notifications** (FCM/APNs) : rappel J‑2 / J‑0 avant échéance d'inscription, place libérée (liste d'attente), cœurs gagnés, nouveau commentaire.
- **Jobs CRON** :
  - clôture automatique des inscriptions à `registration_deadline` ;
  - promotion liste d'attente → inscrit quand une place se libère ;
  - recalcul des compteurs `app_stats` (inscrits, cœurs UQTR) ;
  - rappels push.
- **Cache** (Redis) pour `featured`, `categories`, `leaderboard`, `stats/community`.
- **Recherche** : `q` côté serveur sur titre + lieu (ILIKE/trigram, ou moteur dédié plus tard).

---

## 6. ⚠️ Point délicat — Intégration Google Forms

L'app ouvre un **Google Form** en WebView et détecte la soumission via l'URL (`formResponse`). C'est **fragile** : la vraie inscription vit chez Google, pas chez vous. À cadrer côté backend :

- **Option recommandée** : remplacer le formulaire externe par un **formulaire natif in-app** → `POST /activities/:id/register` direct, capacité/échéance/liste d'attente gérées par le serveur (source de vérité unique).
- **Si Google Forms reste** : ajouter un **Google Apps Script `onFormSubmit`** qui appelle un webhook `POST /webhooks/google-forms` avec `{ activityId, email, formResponseId }`, pour réconcilier l'inscription. Le `POST /register` côté app ne fait alors que pré-marquer « en attente de confirmation ». Pré-remplir l'email UQTR dans l'URL du form pour la correspondance.

---

## 7. Ce qui manquait (ajouté ici)

- **Authentification réelle via Microsoft Entra ID** (OAuth2/OIDC, refresh tokens) — absente du prototype (l'app charge un utilisateur fictif).
- **Catégories & organisateurs** en base (codés en dur dans l'app).
- **Liste d'attente** : mentionnée dans l'UI (« liste d'attente disponible ») mais aucune logique.
- **Attribution des cœurs** : *quand/comment* sont-ils crédités ? → via **présence confirmée**, pas la simple inscription. À définir (check-in QR ? validation organisateur ?).
- **Commentaires** : affichés/saisis dans l'app mais besoin endpoints CRUD + modération.
- **Notifications push** + préférences.
- **Modération & signalement** des contenus sociaux.
- **Upload/stockage d'images** (avatars, posts).
- **Pagination** sur toutes les listes (activités, posts, commentaires, classement).
- **Deep links / partage** : l'URL renvoyée par le bouton Partager doit ouvrir l'activité dans l'app.
- **Stats agrégées** (`1.2k inscrits`, cœurs UQTR) servies par l'API.
- **Conformité Loi 25 (Québec)** : consentement, export et suppression des données personnelles, journal d'accès.
- **Versionnement d'API**, rate-limiting, validation d'entrée, observabilité (logs/متriques/health `GET /health`).
- **Fuseaux horaires** : stocker en UTC, afficher en `America/Toronto` (fr‑CA).

---

## 8. Sécurité & conformité (rappel)

- HTTPS partout, JWT signés (rotation des clés), scopes par rôle sur chaque route admin.
- Validation stricte des entrées, taille/type des fichiers uploadés.
- RGPD/**Loi 25** : `GET /me/export`, `DELETE /me` (suppression/anonymisation), registre de consentement.
- Journaux d'audit pour les actions admin (modération, ajustement de cœurs, changement de rôle).
```
GET  /health        -> { status:'ok', version }
GET  /me/export     -> archive des données personnelles
DELETE /me          -> suppression/anonymisation du compte
```
