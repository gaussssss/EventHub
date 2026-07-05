/**
 * Environnement de PRODUCTION (valeurs à renseigner au déploiement).
 * Remplacé par `environment.development.ts` en dev via `fileReplacements`.
 */
export const environment = {
  production: true,
  apiUrl: 'https://api.eventhub.uqtr.ca/api',

  // Microsoft Entra (SPA / MSAL). Renseigner l'app registration SPA du back-office.
  entra: {
    tenantId: '',
    clientId: '',
    // Scope de l'API EventHub (Option B) : le back-office demande un access token
    // pour cette scope et l'envoie en Bearer.
    apiScope: 'api://<API_CLIENT_ID>/access_as_user',
    redirectUri: 'https://admin.eventhub.uqtr.ca',
  },
};
