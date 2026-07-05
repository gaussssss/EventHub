/**
 * MODÈLE de l'environnement de développement.
 *
 * Copiez ce fichier en `environment.development.ts` (qui est **git-ignoré**) et
 * renseignez vos identifiants Entra locaux. Ce sont des identifiants de client
 * public (SPA) — pas des secrets — mais on évite de les publier dans le dépôt.
 *
 *   cp src/environments/environment.development.example.ts \
 *      src/environments/environment.development.ts
 */
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5199/api',

  entra: {
    // App registration SPA du back-office (portail Azure).
    tenantId: '<TENANT_ID>',
    clientId: '<SPA_CLIENT_ID>',
    apiScope: 'api://<API_CLIENT_ID>/access_as_user',
    redirectUri: 'http://localhost:4200',
  },
};
