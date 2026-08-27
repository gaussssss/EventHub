# Manifeste technique, Application mobile (Flutter)

> Guide de maintenance de l'app **UQTR en santé** (`ca.uqtr.eventhub`), iOS &
> Android. Décrit l'architecture et les écrans **tels qu'implémentés**.
> Commandes de lancement/build : [RUN_AND_BUILD.md](RUN_AND_BUILD.md).
> Détails pratiques : [../mobile/README.md](../mobile/README.md).

- **Plateformes** : iOS & Android (Flutter, un seul code base).
- **Langue** : français (Québec). **Thème** : clair uniquement.
- **Auth** : compte Microsoft 365 UQTR (Entra ID), voir [BACKEND_MANIFEST.md](BACKEND_MANIFEST.md) §3.

---

## 1. Stack & architecture

| Domaine | Choix |
|---|---|
| Framework | Flutter 3 / Dart 3 |
| État | **Riverpod** (`Notifier`/`AsyncNotifier`/`FutureProvider`) |
| Navigation | **go_router** (`StatefulShellRoute` à 3 branches : Accueil / Catalogue / Profil) |
| Réseau | **dio** (`core/network/api_client.dart`) : Bearer auto, retry sur 401 via refresh, erreurs → `Failure` |
| Auth | **flutter_appauth** (OAuth2 + PKCE), jetons dans **flutter_secure_storage** (Keychain/Keystore) |
| QR | **mobile_scanner** (émargement de présence) |
| Divers | `cached_network_image`, `iconsax_flutter`, `webview_flutter`, `share_plus`, `image_picker`, `smooth_page_indicator` |

**Architecture** : Clean Architecture par fonctionnalité :
`features/<feature>/{data: datasources+models+repositories, domain: entities+repositories, presentation: pages+widgets+providers}`.

**Mode démo** : sans dart-defines Entra (`AppConfig.useMockData`), l'app tourne
sur des **données mock locales** (`*_local_datasource.dart`). Dès que la config
Entra est fournie, elle lit l'**API réelle**.

---

## 2. Navigation & routes

| Route | Écran |
|---|---|
| `/splash` | Démarrage (logo, vérif. session) |
| `/login` | Connexion Microsoft |
| `/home` · `/catalogue` · `/profile` | Onglets (barre flottante) |
| `/activity/:id` | Détail d'une activité |
| `/activity/:id/register?url=` | WebView du formulaire d'inscription |
| `/activity/:id/confirmation` | Confirmation d'inscription |
| `/scan` | Scanner le QR de présence |
| `/post/:id` · `/create-post` | Détail post · Publier une photo |
| `/hearts` | Cœurs santé (niveaux, classement, historique) |
| `/about` | À propos (contributeurs) |

---

## 3. Fonctionnalités par écran

### Authentification
- Splash → restaure la session, redirige `/login` ou `/home`. Déconnexion
  automatique si le jeton expire/est révoqué (401).
- Login **Microsoft Entra** (PKCE) + rafraîchissement silencieux du jeton.

### Accueil (`/home`)
- Logo officiel, badge cœurs de l'utilisateur, badge nombre d'inscrits.
- **Carrousel « à la une »** (toutes les activités marquées, défilement auto).
- Barre de catégories, bouton filtre.
- **Calendrier mensuel** à marqueurs (🟢 inscrit · 🔵 événement · 🔴 manqué ;
  clic sur un jour → **tous** les événements du jour, badge de statut).
- **Activités à venir** (indépendantes des filtres), **fil communautaire**.

### Catalogue (`/catalogue`)
- Recherche titre/lieu, catégories dynamiques, **filtres poussés au backend**
  (dispo, dates, passées, inscrits), **pull-to-refresh**.
- Cartes d'activité (catégorie, date/lieu, cœurs, places restantes/Complet,
  badge Inscrit).

### Détail d'activité (`/activity/:id`)
- **Rechargé depuis l'API à chaque ouverture** (données à jour). Affiche coût de
  participation, date, lieu, organisateur, échéance, places, partage.
- **Bouton S'inscrire** → formulaire en WebView (n'importe quelle URL).
- Une fois inscrit : message « points attribués après confirmation de présence »
  + **période de confirmation** ; **bouton « Scanner ma présence »** visible
  **uniquement pendant la fenêtre** d'émargement ; remerciement une fois présent.

### Émargement (`/scan`)
- Scanne le QR affiché par l'organisateur → `POST /check-in` → présence
  confirmée + cœurs crédités, avec messages clairs (déjà pointé, QR invalide,
  non inscrit, hors fenêtre, remerciement).

### Social
- Fil (photo, auteur, activité liée, like ❤️, commentaires), détail post,
  **publier une photo** (galerie/caméra), **commenter**, **signaler**.

### Profil (`/profile`) & Cœurs (`/hearts`)
- Avatar **modifiable** (upload persisté côté serveur), mes activités, carte
  cœurs santé (niveau + progression), lien vers À propos.
- Cœurs : total, **niveau** (Bronze/Argent/Or), **classement** (top 20 + rang
  personnel), **historique** par activité.

### À propos (`/about`)
- Identité de l'app + **contributeurs** (gérés depuis le back-office).

---

## 4. Gamification, « cœurs santé »

- Chaque activité rapporte un nombre de cœurs (`heartsReward`).
- **Crédités à la présence confirmée** (scan QR sur place ou marquage
  back-office), jamais à la simple inscription.
- Cumul → niveaux : Bronze (<200), Argent (≥200), Or (≥500). Niveaux et seuils
  **calculés côté app** à partir du total.

---

## 5. Configuration & fuseau horaire

- Dart-defines : `ENTRA_TENANT_ID`, `ENTRA_CLIENT_ID`, `ENTRA_API_SCOPE`,
  `API_BASE_URL` (+ `USE_MOCK_DATA`, `SHARE_BASE_URL` optionnels). Voir
  [RUN_AND_BUILD.md](RUN_AND_BUILD.md).
- **Fuseau** : l'API sert de l'UTC ; l'app parse en UTC puis **affiche en heure
  locale** (`.toLocal()`) et renvoie en UTC (`.toUtc()`). La fenêtre d'émargement
  est un **miroir exact** des constantes serveur.
- Logo/icônes générés depuis `assets/logo/logo_light.png`.

---

## 6. Limites connues

Non réalisés (à prévoir pour une version ultérieure) :

- **Notifications push** (FCM/APNs) et écran de préférences push.
- **Deep links** (`ca.uqtr.eventhub://…`) pour le partage.
- **Pagination** sur certaines listes (fil, classement).
- **Tests** widget/golden et télémétrie/crash (Sentry/Firebase).
- **i18n** formelle (fr-CA) et audit d'accessibilité complet.
