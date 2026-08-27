# Lancer & construire, guide des commandes

Référence unique des commandes pour lancer, construire et outiller les trois
tiers d'**UQTR en santé** (EventHub). Chemins relatifs à la racine du dépôt.

Ordre de démarrage habituel : **API d'abord**, puis back-office et/ou mobile.

---

## 1. API (.NET 8), `api/`

### Configuration (user-secrets)

Les valeurs sensibles ne sont **jamais** dans le dépôt : elles vivent en
*user-secrets* de dev (`UserSecretsId = eventhub-api-secrets`). Sans elles,
l'API démarre en **mode dev** (schéma d'en-tête `X-User-Id`, voir sécurité).
Avec elles, elle valide les vrais jetons Microsoft Entra.

```bash
cd api

# Renseigner l'auth Microsoft Entra (à faire une fois par poste) :
dotnet user-secrets set "Authentication:Authority" "https://login.microsoftonline.com/<TENANT_ID>/v2.0" --project src/EventHub.Api
dotnet user-secrets set "Authentication:Audience"  "api://<API_CLIENT_ID>"                              --project src/EventHub.Api
dotnet user-secrets set "Authentication:AdminEmails:0" "prenom.nom@uqtr.ca"                             --project src/EventHub.Api

# Optionnels (durcissement) :
dotnet user-secrets set "Authentication:TenantId"          "<TENANT_ID>" --project src/EventHub.Api
dotnet user-secrets set "Authentication:AllowedEmailDomain" "uqtr.ca"    --project src/EventHub.Api

# Vérifier / vider :
dotnet user-secrets list  --project src/EventHub.Api
dotnet user-secrets clear --project src/EventHub.Api
```

> `AdminEmails` amorce le premier administrateur : au 1er login, ce compte
> reçoit le rôle `admin` (les rôles suivants se gèrent dans le back-office).

### Lancer

```bash
cd api
dotnet run --urls "http://0.0.0.0:5199" --project src/EventHub.Api
```

- `0.0.0.0` : écoute sur toutes les interfaces → joignable depuis un **appareil
  mobile physique** (IP LAN) et depuis l'**émulateur Android** (`10.0.2.2`).
- Les **migrations EF sont appliquées au démarrage** (`Database.Migrate()`), la
  base SQLite `eventhub.db` est créée/à jour automatiquement.
- Swagger (dev) : `http://localhost:5199/swagger`. Santé : `GET /health`.

### Construire, tester, migrations

```bash
cd api
dotnet build                 # compiler la solution
dotnet test EventHub.sln     # tous les tests (unitaires + intégration)

# Migrations EF (schéma sous src/EventHub.Infrastructure/Persistence/Migrations) :
dotnet ef migrations add <Nom> --project src/EventHub.Infrastructure --startup-project src/EventHub.Api --output-dir Persistence/Migrations
dotnet ef database update      --project src/EventHub.Infrastructure --startup-project src/EventHub.Api
```

> Données de démo : bouton **« Seed dev data »** du tableau de bord back-office
> (endpoint `POST /api/admin/dev/seed`, dispo uniquement en environnement
> Development). Réinitialise et régénère un jeu réaliste.

---

## 2. Back-office (Angular 20), `backoffice/`

### Configuration

Éditer `src/environments/environment.development.ts` (gitignoré, contient les
identifiants Entra locaux) :

```ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5199/api',
  entra: {
    tenantId: '<TENANT_ID>',
    clientId: '<SPA_CLIENT_ID>',   // app registration SPA du back-office (≠ API, ≠ mobile)
    apiScope: 'api://<API_CLIENT_ID>/access_as_user',
    redirectUri: 'http://localhost:4200',
  },
};
```

> L'app SPA doit avoir la plateforme **Single-page application** dans Entra
> (redirect URI `http://localhost:4200`) et une permission déléguée sur la
> scope de l'API (consentement admin accordé).

### Lancer, construire, tester

```bash
cd backoffice
npm install
npm start                       # ng serve → http://localhost:4200

npm run build                   # build de production (dist/)
npm test -- --watch=false --browsers=ChromeHeadless   # tests (Karma/Jasmine)
```

---

## 3. Mobile (Flutter), `mobile/`

### Mode démo (aucune configuration)

Sans dart-defines Entra, l'app tourne sur des **données mock** locales.

```bash
cd mobile
flutter pub get
flutter run
```

### Mode réel (API + Microsoft Entra)

```bash
cd mobile
flutter run \
  --dart-define=ENTRA_TENANT_ID=<TENANT_ID> \
  --dart-define=ENTRA_CLIENT_ID=<MOBILE_CLIENT_ID> \
  --dart-define=ENTRA_API_SCOPE=api://<API_CLIENT_ID>/access_as_user \
  --dart-define=API_BASE_URL=http://127.0.0.1:5199
```

**`API_BASE_URL` selon la cible** :

| Cible | Valeur |
|---|---|
| Simulateur iOS | `http://127.0.0.1:5199` |
| Émulateur Android | `http://10.0.2.2:5199` |
| Appareil physique (iOS/Android) | `http://<IP-LAN-de-la-machine>:5199` (même réseau) |

> ⚠️ `ENTRA_API_SCOPE` est **indispensable** : sans elle, le jeton vise
> Microsoft Graph et l'API répond 401 (l'app reboucle sur le login).
> Optionnel : `--dart-define=USE_MOCK_DATA=true` force le mode démo,
> `--dart-define=SHARE_BASE_URL=…` fixe la base des liens de partage.

### Construire, outiller

```bash
cd mobile
flutter analyze                 # analyse statique
flutter test                    # tests

# Builds (ajouter les mêmes --dart-define qu'au lancement pour le mode réel) :
flutter build apk               # Android
flutter build ipa               # iOS (macOS + Xcode requis)

dart run flutter_launcher_icons # régénérer les icônes depuis assets/logo/logo_light.png
```
