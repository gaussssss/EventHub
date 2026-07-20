# UQTR en santé, app mobile (Flutter)

Application mobile iOS/Android de la plateforme d'activités du campus UQTR.
Spécification d'origine : [../docs/MOBILE_APP_MANIFEST.md](../docs/MOBILE_APP_MANIFEST.md).

## Fonctionnalités

- **Connexion Microsoft Entra** (compte du tenant), rafraîchissement silencieux
  du jeton, déconnexion automatique si la session est révoquée
- **Accueil** : logo officiel, activités « à la une » (carrousel auto),
  calendrier mensuel à marqueurs (🟢 inscrit, 🔵 événement, 🔴 manqué,
  clic sur un jour = tous les événements), activités à venir, fil communautaire
- **Catalogue** : recherche, catégories dynamiques, filtres poussés au backend
  (dispo, dates, passées, inscrits), pull-to-refresh
- **Fiche activité** : places restantes, coût de participation (informatif),
  inscription via formulaire web (webview), partage, bouton
  **« Scanner ma présence »** (QR de l'organisateur → cœurs crédités)
- **Profil** : photo (upload persisté), mes activités, cœurs santé + niveau,
  classement (top 20 + rang personnel), page « À propos » (contributeurs gérés
  au back office)
- **Social** : publier une photo (galerie/caméra), commenter, aimer, signaler

## Architecture

- **Flutter 3 + Riverpod** (`Notifier`/`AsyncNotifier`), navigation **go_router**
  (StatefulShellRoute à 3 branches : Accueil / Catalogue / Profil)
- **Clean Architecture par feature** : `features/<feature>/{domain,data,presentation}`
- **Réseau** : `dio` (`core/network/api_client.dart`), Bearer automatique,
  retry sur 401 via refresh token, erreurs traduites en `Failure`
- **Mode démo** : sans configuration Entra, l'app tourne sur des données mock
  locales (`AppConfig.useMockData`)

## Lancer l'app

### Mode démo (aucune config)

```bash
flutter pub get
flutter run
```

### Mode réel (API + Microsoft Entra)

```bash
flutter run \
  --dart-define=ENTRA_TENANT_ID=<tenant-guid> \
  --dart-define=ENTRA_CLIENT_ID=<clientId-app-mobile> \
  --dart-define=ENTRA_API_SCOPE=api://<clientId-API>/access_as_user \
  --dart-define=API_BASE_URL=http://<hôte>:5199
```

- `API_BASE_URL` : émulateur Android → `http://10.0.2.2:5199` ; appareil
  physique → IP LAN de la machine qui héberge l'API (même réseau).
- `ENTRA_API_SCOPE` est **indispensable** : sans elle, le jeton vise Microsoft
  Graph et l'API répond 401 (boucle de reconnexion).
- Optionnel : `--dart-define=USE_MOCK_DATA=true` force le mode démo,
  `SHARE_BASE_URL` fixe la base des liens de partage.

## Icônes de lanceur

Générées depuis le logo client (`assets/logo/logo_light.png`) via
`flutter_launcher_icons` (config dans `pubspec.yaml`) :

```bash
dart run flutter_launcher_icons
```

## Tests & qualité

```bash
flutter analyze
flutter test
```
