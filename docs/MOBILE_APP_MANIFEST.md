# EventHub, Manifeste de l'application mobile

> Description fonctionnelle et technique de l'app Flutter `ca.uqtr.eventhub`.
> Compagnon de [BACKEND_MANIFEST.md](BACKEND_MANIFEST.md).

---

## 1. Vision

EventHub est l'application **mobile UQTR** qui réunit en un seul endroit les **activités sportives et socioculturelles** du campus. Les étudiant·es découvrent les événements, s'y inscrivent, gagnent des **cœurs santé** (gamification du bien-être) et partagent leurs moments dans un **fil communautaire**.

- **Plateformes** : iOS & Android (Flutter, un seul code).
- **Identifiant app** : `ca.uqtr.eventhub`.
- **Langue** : français (Québec).
- **Authentification** : compte **Microsoft 365 UQTR** (Entra ID), voir backend §1.

---

## 2. Stack technique

| Domaine | Choix |
|---|---|
| Framework | Flutter 3.x / Dart 3 |
| Gestion d'état | **Riverpod** (`flutter_riverpod`), `Notifier`/`Provider` |
| Navigation | **go_router** (`StatefulShellRoute` pour la barre d'onglets) |
| Réseau | `dio` + couche `repository` (branché à l'API réelle ; repli mock sans config Entra) |
| Images | `cached_network_image` |
| Icônes | **Iconsax** (`iconsax_flutter`) |
| WebView | `webview_flutter` (formulaires d'inscription) |
| Partage | `share_plus` |
| Sélection photo | `image_picker` |
| Carrousel | `smooth_page_indicator` + `PageView` |

**Architecture** : *Clean Architecture* par fonctionnalité,
`feature/{data: datasources, models, repositories | domain: entities, repositories | presentation: pages, widgets, providers}`.

---

## 3. Données gérées (entités)

| Entité | Champs clés |
|---|---|
| **Activity** | id, title, description, category (`sport`/`socioculturel`), date, location, organizer, imageUrl, hearts (récompense), maxParticipants, currentParticipants, registrationUrl, registrationDeadline |
| **ActivityFilter** | category, availableOnly, dateRange (modèle de filtre immuable) |
| **UserProfile** | id, name, email, avatarUrl, totalHearts, level (Bronze/Argent/Or, dérivé), completedActivityIds, heartHistory |
| **Post** | id, authorName, authorAvatarUrl, imageUrl, caption, activityName, createdAt, likesCount, comments |
| **PostComment** | authorName, text, createdAt |

**État applicatif (Riverpod)** :
- `activityFilterProvider` (NotifierProvider), filtres catalogue centralisés.
- `registeredActivitiesProvider` (Notifier), inscriptions : `register/unregister/toggle/isRegistered`.
- `likedPostsProvider` (Notifier), likes : `toggle/isLiked`.
- `filteredActivitiesProvider`, `hasActiveFiltersProvider`, `catalogueSearchProvider`, `avatarUrlProvider`…

---

## 4. Navigation & routes (go_router)

| Route | Écran | Onglet ? |
|---|---|---|
| `/splash` | Démarrage / vérif. session |, |
| `/login` | Connexion Microsoft UQTR |, |
| `/home` | **Accueil** | Onglet 1 |
| `/catalogue` | **Catalogue** (liste + recherche + filtres) | Onglet 2 |
| `/profile` | **Profil** | Onglet 3 |
| `/activity/:id` | Détail d'une activité |, |
| `/activity/:id/register?url=` | WebView formulaire d'inscription |, |
| `/activity/:id/confirmation` | Confirmation d'inscription (animation) |, |
| `/post/:id` | Détail d'une publication |, |
| `/create-post` | Publier une photo |, |
| `/hearts` | Cœurs santé (niveaux + classement + historique) |, |

Barre de navigation **flottante façon Apple Music** (verre dépoli, onglet actif avec libellé).

---

## 5. Fonctionnalités par écran

### 5.1 Authentification (Splash / Login)
- Écran de démarrage → vérifie la session, redirige `/login` ou `/home`.
- Connexion via **Microsoft Entra ID** (OAuth2/PKCE), restreinte au tenant UQTR.

### 5.2 Accueil (`/home`)
- Badge **nombre d'inscrits à l'app** (« 1.2k inscrits »).
- Badge **cœurs de l'utilisateur connecté** (remplace l'icône de recherche) → tape vers le Catalogue.
- **Carrousel « à la une »** : 3 activités qui défilent (PageView + points indicateurs).
- **Barre de catégories** (Tout / Sport / Socioculturel).
- **Bouton filtre** → feuille de filtres ; « Appliquer » redirige vers le Catalogue.
- Mini-calendrier, **Activités à venir** (liste horizontale), **Fil communautaire** (posts) + bouton Publier.

### 5.3 Catalogue (`/catalogue`)
- Grand titre + **barre de recherche** (titre/lieu) avec bouton effacer.
- **Bouton filtre** avec badge du nombre de filtres actifs.
- Puces de catégories + **puces de filtres actifs supprimables** (places dispo, intervalle de dates).
- Compteur de résultats, **cartes d'activité** (badge catégorie, date/lieu, cœurs, **barre de places restantes / Complet**, badge Inscrit), état vide soigné.
- **Page de résultats unique** : toute recherche/filtre (y compris depuis l'accueil) aboutit ici.

### 5.4 Détail d'activité (`/activity/:id`)
- Image, catégorie, titre, **récompense en cœurs**, date, heure, lieu, organisateur.
- **Date limite d'inscription** (rouge si dépassée).
- **Indicateur de places** + « Complet, liste d'attente disponible ».
- **Bouton Partager** (`share_plus`).
- **Bouton S'inscrire** : ouvre le formulaire **en WebView** (reste dans l'app) ; désactivé si complet ou échéance passée.

### 5.5 Inscription (WebView + confirmation)
- `/activity/:id/register` : WebView du formulaire (Google Form aujourd'hui, voir backend §6) ; **détecte la soumission** → bouton « J'ai soumis le formulaire ».
- `/activity/:id/confirmation` : animation de succès, marque l'activité comme **inscrite** (`registeredActivitiesProvider`).

### 5.6 Social (Fil / Détail / Publier)
- Fil de publications (photo, auteur, activité liée, **like ❤️**, nombre de commentaires).
- Détail post : likes, **commentaires**, légende.
- **Publier une photo** : sélection image, légende, association à une activité.

### 5.7 Profil (`/profile`)
- En-tête : avatar (**modifiable**, caméra/galerie via `image_picker`), nom, courriel, stats (activités, cœurs).
- **Carte Cœurs santé** (niveau, progression vers le niveau suivant, total communauté).
- **Mes activités** (inscriptions), **Modifier le profil** (nom).

### 5.8 Cœurs santé (`/hearts`)
- Total de cœurs, **niveau** (Bronze/Argent/Or) + progression.
- **Classement** de la communauté UQTR.
- **Historique** des cœurs gagnés par activité.

---

## 6. Gamification, « cœurs santé »

- Chaque activité rapporte un nombre de **cœurs** (`hearts`).
- Cumul → **niveaux** : Bronze (<200), Argent (≥200), Or (≥500), seuil suivant à 1000.
- ⚠️ **Règle d'attribution à confirmer côté backend** : les cœurs devraient être crédités à la **présence confirmée**, pas à la simple inscription (voir backend §7).

---

## 7. Branchement à l'API : RÉALISÉ

L'app est **branchée au backend réel** : login Microsoft Entra (PKCE + refresh
silencieux), catalogue/filtres serveur, inscriptions et annulations persistées,
profil et avatar réels, classement, stats communautaires, fil social, page
« À propos », auto-émargement par QR. Le **mode mock** subsiste comme démo
hors-ligne quand la config Entra n'est pas fournie (voir mobile/README.md).

---

## 8. À prévoir côté app (manques)

- Intégration **MSAL/AppAuth** pour le login Microsoft + stockage sécurisé des tokens (`flutter_secure_storage`).
- **Gestion hors-ligne / chargement / erreurs** (états `AsyncValue` Riverpod, retry).
- **Pagination** des listes (activités, posts, commentaires, classement).
- **Notifications push** (FCM/APNs) + écran de préférences.
- **Deep links** (`ca.uqtr.eventhub://activity/:id`) pour le partage.
- **Accessibilité** (tailles de police, contrastes, `Semantics`) et **i18n** propre (fr-CA).
- **Tests** (widget + golden) et **télémétrie/crash** (Sentry/Firebase).
- Conformité **Loi 25** : écran de consentement, suppression/export du compte.
