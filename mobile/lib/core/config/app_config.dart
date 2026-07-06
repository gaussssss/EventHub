/// Configuration centrale de l'application.
///
/// Le mode « données » suit la configuration : dès que l'auth Microsoft Entra
/// est renseignée (dart-defines tenant + client), l'app lit l'**API réelle** ;
/// sans config, elle tourne en **mock** (démo hors-ligne). On peut forcer le
/// mock même avec l'auth réelle via `--dart-define=USE_MOCK_DATA=true`
/// (voir docs/BACKEND_MANIFEST.md §1).
class AppConfig {
  const AppConfig._();

  /// Force le mode mock même si l'auth réelle est configurée (débogage / démo).
  static const bool _forceMockData =
      bool.fromEnvironment('USE_MOCK_DATA', defaultValue: false);

  /// Bascule sources locales ⇄ API distante. `true` = sources locales (mock).
  /// Live dès que l'auth réelle est configurée, sauf override explicite.
  static bool get useMockData => _forceMockData || !useRealAuth;

  /// Latence simulée des sources locales (rend visibles les états de chargement).
  static const Duration mockLatency = Duration(milliseconds: 350);

  // --- API -----------------------------------------------------------------
  // Racine de l'API (les routes sont préfixées « /api/… »). Surchargée au build
  // via --dart-define=API_BASE_URL=… . En dev local : http://localhost:5199
  // (émulateur Android : http://10.0.2.2:5199). En prod : https://api.eventhub.uqtr.ca
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://localhost:5199',
  );

  // --- Microsoft Entra ID (Azure AD) --------------------------------------
  static const String entraTenantId =
      String.fromEnvironment('ENTRA_TENANT_ID', defaultValue: '');
  static const String entraClientId =
      String.fromEnvironment('ENTRA_CLIENT_ID', defaultValue: '');
  // NB: le slash final est requis pour Azure AD sur iOS. Sans lui, l'AppAuth
  // iOS SDK échoue à valider le redirect (le retour ne revient pas au plugin →
  // timeout), alors qu'Android fonctionne. Cf. FAQ officielle flutter_appauth.
  static const String entraRedirectUri = 'ca.uqtr.eventhub://auth/';

  /// Scope de l'API EventHub (Option B) : l'app demande un access token pour
  /// cette scope et l'envoie en `Bearer`. Ex. `api://<clientId-API>/access_as_user`.
  /// Surchargée via --dart-define=ENTRA_API_SCOPE=… .
  static const String entraApiScope =
      String.fromEnvironment('ENTRA_API_SCOPE', defaultValue: '');

  /// Endpoints Entra v2 (dérivés du tenant).
  static String get entraAuthorizationEndpoint =>
      'https://login.microsoftonline.com/$entraTenantId/oauth2/v2.0/authorize';
  static String get entraTokenEndpoint =>
      'https://login.microsoftonline.com/$entraTenantId/oauth2/v2.0/token';

  /// Scopes demandés au login (OIDC + refresh + scope API si fournie).
  static List<String> get entraOAuthScopes => [
        'openid',
        'profile',
        'email',
        'offline_access',
        if (entraApiScope.isNotEmpty) entraApiScope,
      ];

  /// L'auth réelle Microsoft est active dès que tenant + client sont renseignés.
  /// Sinon l'app retombe sur l'auth mock (aucune config = démo hors-ligne).
  static bool get useRealAuth =>
      entraTenantId.isNotEmpty && entraClientId.isNotEmpty;
}
