import 'dart:async';
import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter_appauth/flutter_appauth.dart';

import '../../../../core/config/app_config.dart';
import '../../../../core/error/failure.dart';
import '../../../../core/storage/token_storage.dart';
import '../../domain/entities/user.dart';
import '../../domain/repositories/auth_repository.dart';

/// Connexion réelle **Microsoft Entra ID** (OAuth 2.0 / OIDC, Authorization Code
/// + PKCE via AppAuth). Conforme à l'Option B : l'app obtient un **access token**
/// pour la scope de l'API EventHub et le persiste ; l'`ApiClient` l'envoie en
/// `Authorization: Bearer`. Le backend valide le jeton et provisionne l'utilisateur.
class AuthRepositoryEntra implements AuthRepository {
  final FlutterAppAuth _appAuth;
  final TokenStorage _tokens;

  AuthRepositoryEntra(this._tokens, {FlutterAppAuth? appAuth})
      : _appAuth = appAuth ?? FlutterAppAuth();

  AuthorizationServiceConfiguration get _serviceConfiguration =>
      AuthorizationServiceConfiguration(
        authorizationEndpoint: AppConfig.entraAuthorizationEndpoint,
        tokenEndpoint: AppConfig.entraTokenEndpoint,
      );

  @override
  Future<User> signInWithMicrosoft() async {
    try {
      debugPrint('[Entra] 1/2 authorize → ${AppConfig.entraAuthorizationEndpoint} '
          'scopes=${AppConfig.entraOAuthScopes} redirect=${AppConfig.entraRedirectUri}');

      // 1) Autorisation : ouvre le navigateur, l'utilisateur se connecte, puis le
      //    redirect ca.uqtr.eventhub://auth doit ramener le CODE au plugin.
      final auth = await _appAuth
          .authorize(
            AuthorizationRequest(
              AppConfig.entraClientId,
              AppConfig.entraRedirectUri,
              serviceConfiguration: _serviceConfiguration,
              scopes: AppConfig.entraOAuthScopes,
              promptValues: const ['select_account'],
              // Agent par défaut (ASWebAuthenticationSession) : chemin « best
              // practices » qui s'appuie sur le CFBundleURLTypes déclaré dans
              // Info.plist. Combiné au redirect à slash final, c'est le setup
              // canonique qui capture le retour sur iOS (y compris simulateur).
            ),
          )
          .timeout(const Duration(seconds: 90));

      final code = auth.authorizationCode;
      if (code == null || code.isEmpty) {
        throw const Failure('Autorisation Microsoft annulée.');
      }
      // Si cette ligne s'affiche, LE REDIRECT FONCTIONNE (le code est revenu).
      debugPrint('[Entra] ✓ code reçu (redirect OK) → 2/2 échange du token…');

      // 2) Échange code → tokens : appel HTTPS direct au token endpoint.
      final result = await _appAuth
          .token(
            TokenRequest(
              AppConfig.entraClientId,
              AppConfig.entraRedirectUri,
              serviceConfiguration: _serviceConfiguration,
              authorizationCode: code,
              codeVerifier: auth.codeVerifier,
              nonce: auth.nonce,
              scopes: AppConfig.entraOAuthScopes,
            ),
          )
          .timeout(const Duration(seconds: 30));

      final accessToken = result.accessToken;
      if (accessToken == null || accessToken.isEmpty) {
        throw const Failure.unauthorized();
      }

      await _tokens.save(access: accessToken, refresh: result.refreshToken);
      debugPrint('[Entra] ✓ token reçu (len=${accessToken.length})');
      return _userFromClaims(result.idToken) ??
          _userFromClaims(accessToken) ??
          const User(id: '', name: '', email: '');
    } on Failure {
      rethrow;
    } on TimeoutException {
      debugPrint('[Entra] TIMEOUT — regarde la dernière ligne : sans "code reçu" '
          '= le redirect ne revient pas ; sinon = l\'échange du token bloque.');
      throw const Failure(
          'Connexion expirée (le retour depuis Microsoft n\'a pas abouti).');
    } catch (e, st) {
      // Trace la cause réelle (PlatformException code/message, etc.).
      debugPrint('[Entra] ÉCHEC login : $e');
      debugPrint('$st');
      throw Failure('Échec de la connexion Microsoft : $e');
    }
  }

  @override
  Future<User?> restoreSession() async {
    // Un access token stocké = session ouverte. (Le rafraîchissement silencieux
    // via le refresh token pourra être branché ici avant l'expiration.)
    final accessToken = await _tokens.readAccessToken();
    if (accessToken == null || accessToken.isEmpty) return null;
    return _userFromClaims(accessToken) ??
        const User(id: '', name: '', email: '');
  }

  @override
  Future<void> signOut() => _tokens.clear();

  /// Extrait id/nom/courriel des claims d'un JWT (id_token ou access token).
  User? _userFromClaims(String? jwt) {
    final claims = _decodeJwtPayload(jwt);
    if (claims == null) return null;
    return User(
      id: (claims['oid'] ?? claims['sub'] ?? '') as String,
      name: (claims['name'] ?? '') as String,
      email: (claims['preferred_username'] ?? claims['email'] ?? '') as String,
    );
  }

  Map<String, dynamic>? _decodeJwtPayload(String? jwt) {
    if (jwt == null) return null;
    final parts = jwt.split('.');
    if (parts.length != 3) return null;
    try {
      final payload =
          utf8.decode(base64Url.decode(base64Url.normalize(parts[1])));
      return jsonDecode(payload) as Map<String, dynamic>;
    } catch (_) {
      return null;
    }
  }
}
