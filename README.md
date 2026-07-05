# EventHub — Monorepo UQTR

Plateforme d'activités sportives et socioculturelles du campus UQTR.
Le dépôt regroupe les **trois tiers** du produit + la documentation transverse.

## Structure

```
EventHub/
├── mobile/      # App Flutter (iOS & Android) — ca.uqtr.eventhub
├── api/         # API Web REST (backend, source de vérité)
├── backoffice/  # Interface d'administration (organizer/moderator/admin)
└── docs/        # Documentation transverse aux trois tiers
```

## Tiers

| Dossier | Description | État |
|---|---|---|
| [`mobile/`](mobile) | Application mobile Flutter/Riverpod | ✅ en cours |
| [`api/`](api) | Backend REST + auth Microsoft Entra ID | ⏳ à venir |
| [`backoffice/`](backoffice) | Administration & modération | ⏳ à venir |

## Documentation

- [docs/MOBILE_APP_MANIFEST.md](docs/MOBILE_APP_MANIFEST.md) — fonctionnalités et
  architecture de l'app mobile.
- [docs/BACKEND_MANIFEST.md](docs/BACKEND_MANIFEST.md) — API, schémas BD,
  back office, contrats JSON et authentification.

## Démarrage — mobile

```bash
cd mobile
flutter pub get
flutter run
```
