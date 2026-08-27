# UQTR en santé (EventHub)

Plateforme d'activités sportives et socioculturelles du campus UQTR :
les étudiants s'inscrivent aux activités, confirment leur présence en scannant
un QR sur place et cumulent des « cœurs santé » (niveaux Bronze/Argent/Or,
classement communautaire).

Le dépôt regroupe les **trois tiers** du produit + la documentation transverse.

## Structure

```
EventHub/
├── mobile/      # App Flutter iOS & Android « UQTR en santé » (ca.uqtr.eventhub)
├── api/         # API REST .NET 8 (backend, source de vérité)
├── backoffice/  # Administration Angular 20 (organizer/moderator/admin)
└── docs/        # Manifestes fonctionnels transverses
```

## Tiers

| Dossier | Description | État |
|---|---|---|
| [`mobile/`](mobile) | Application mobile Flutter/Riverpod, branchée à l'API réelle (auth Microsoft Entra) | ✅ fonctionnel |
| [`api/`](api) | Backend REST .NET 8, Clean Architecture, EF Core + SQLite, auth Entra, 155 tests | ✅ fonctionnel |
| [`backoffice/`](backoffice) | Administration & modération (Angular 20, MSAL) | ✅ fonctionnel |

## Fonctionnalités clés

- Catalogue d'activités (catégories, filtres serveur, recherche, « à la une »)
- Inscriptions avec liste d'attente et promotion automatique, échéances
- **Présence par QR** : le back office affiche le QR de l'événement, l'étudiant
  le scanne dans l'app, les cœurs sont crédités (fenêtre horaire, anti-rejeu)
- Gamification : cœurs santé, niveaux, classement (top 20 + rang personnel)
- Fil communautaire : photos, commentaires, likes, signalements + modération
- Calendrier mensuel avec marqueurs (inscrit / événement / manqué)
- Page « À propos » dont les contributeurs sont gérés depuis le back office

## Démarrage rapide

```bash
# 1) API (http://localhost:5199), migrations appliquées au démarrage
cd api && dotnet run --project src/EventHub.Api

# 2) Back office (http://localhost:4200)
cd backoffice && npm install && npm start

# 3) App mobile (voir mobile/README.md pour les dart-defines Entra)
cd mobile && flutter pub get && flutter run
```

Chaque tier a son README détaillé : [api/](api/README.md),
[backoffice/](backoffice/README.md), [mobile/](mobile/README.md).

## Documentation

Manifestes techniques (guides de maintenance, fidèles au code) :

- [docs/BACKEND_MANIFEST.md](docs/BACKEND_MANIFEST.md), API .NET 8 : architecture,
  modèle de données, endpoints, sécurité.
- [docs/MOBILE_APP_MANIFEST.md](docs/MOBILE_APP_MANIFEST.md), app Flutter :
  architecture, écrans, configuration.
- [docs/BACKOFFICE_MANIFEST.md](docs/BACKOFFICE_MANIFEST.md), administration
  Angular : écrans et points techniques.
- [docs/RUN_AND_BUILD.md](docs/RUN_AND_BUILD.md), **toutes les commandes** de
  lancement, build et outillage des trois tiers.
