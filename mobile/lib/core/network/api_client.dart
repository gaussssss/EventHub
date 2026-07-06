import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

import '../config/app_config.dart';
import '../error/failure.dart';
import '../storage/token_storage.dart';

/// Client HTTP unique de l'application (au-dessus de Dio).
///
/// - Base URL : [AppConfig.apiBaseUrl].
/// - Injecte automatiquement l'en-tête `Authorization: Bearer <token>` quand une
///   session existe (jeton d'accès Entra stocké par la couche Auth).
/// - Traduit les erreurs réseau/HTTP en [Failure] présentables (401, 404, 5xx…).
class ApiClient {
  final Dio _dio;

  /// Appelé quand l'API répond `401` et que le rafraîchissement silencieux a
  /// échoué (ou est absent) → la couche Auth clôt la session et le routeur
  /// redirige vers l'écran de connexion.
  final void Function()? _onUnauthorized;

  /// Rafraîchit silencieusement l'access token (via le refresh token) et renvoie
  /// le nouveau, ou `null` si impossible. Déclenché une seule fois par requête
  /// sur un `401` avant d'envisager la déconnexion.
  final Future<String?> Function()? _onRefreshToken;

  ApiClient(
    TokenStorage tokens, {
    Dio? dio,
    void Function()? onUnauthorized,
    Future<String?> Function()? onRefreshToken,
  })  : _onUnauthorized = onUnauthorized,
        _onRefreshToken = onRefreshToken,
        _dio = dio ??
            Dio(BaseOptions(
              baseUrl: AppConfig.apiBaseUrl,
              connectTimeout: const Duration(seconds: 10),
              receiveTimeout: const Duration(seconds: 15),
              headers: {'Accept': 'application/json'},
            )) {
    _dio.interceptors.add(InterceptorsWrapper(
      onRequest: (options, handler) async {
        final token = await tokens.readAccessToken();
        if (token != null && token.isNotEmpty) {
          options.headers['Authorization'] = 'Bearer $token';
        }
        handler.next(options);
      },
      onError: (e, handler) async {
        final is401 = e.response?.statusCode == 401;
        final alreadyRetried = e.requestOptions.extra['__retried__'] == true;
        // Sur un 401 : une seule tentative de refresh silencieux + rejeu de la
        // requête. Si le refresh aboutit, l'appelant ne voit jamais l'erreur.
        if (is401 && !alreadyRetried && _onRefreshToken != null) {
          final newToken = await _onRefreshToken();
          if (newToken != null && newToken.isNotEmpty) {
            final req = e.requestOptions;
            req.extra['__retried__'] = true; // garde-fou anti-boucle
            req.headers['Authorization'] = 'Bearer $newToken';
            try {
              final response = await _dio.fetch(req);
              return handler.resolve(response);
            } on DioException catch (retryError) {
              return handler.next(retryError);
            }
          }
        }
        handler.next(e);
      },
    ));
  }

  Future<dynamic> get(String path, {Map<String, dynamic>? query}) =>
      _send(() => _dio.get(path, queryParameters: query));

  Future<dynamic> post(String path, {Object? data}) =>
      _send(() => _dio.post(path, data: data));

  Future<dynamic> patch(String path, {Object? data}) =>
      _send(() => _dio.patch(path, data: data));

  Future<dynamic> delete(String path) => _send(() => _dio.delete(path));

  Future<dynamic> _send(Future<Response<dynamic>> Function() run) async {
    try {
      final response = await run();
      return response.data;
    } on DioException catch (e) {
      throw _map(e);
    }
  }

  Failure _map(DioException e) {
    if (e.type == DioExceptionType.connectionError ||
        e.type == DioExceptionType.connectionTimeout ||
        e.type == DioExceptionType.receiveTimeout ||
        e.type == DioExceptionType.sendTimeout) {
      // Diagnostic : URL réellement visée + cause bas niveau (ATS, refus, DNS…).
      debugPrint('[Api] ÉCHEC RÉSEAU vers ${e.requestOptions.uri} '
          '| baseUrl=${AppConfig.apiBaseUrl} | type=${e.type} '
          '| error=${e.error} | msg=${e.message}');
      return const Failure.network();
    }

    final status = e.response?.statusCode ?? 0;
    if (status == 401) {
      // Jeton refusé par l'API → déconnexion automatique.
      _onUnauthorized?.call();
      return const Failure.unauthorized();
    }
    if (status == 404) return const Failure.notFound();
    if (status >= 500) return const Failure.server();

    // 4xx métier : remonter le message serveur (ProblemDetails ou { error }).
    return Failure(_messageFrom(e.response?.data) ?? 'Requête invalide.');
  }

  String? _messageFrom(dynamic data) {
    if (data is Map) {
      final detail = data['detail'] ?? data['title'] ?? data['error'];
      if (detail is String && detail.isNotEmpty) return detail;
    }
    return null;
  }
}
