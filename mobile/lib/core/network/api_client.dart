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

  ApiClient(TokenStorage tokens, {Dio? dio})
      : _dio = dio ??
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
    if (status == 401) return const Failure.unauthorized();
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
