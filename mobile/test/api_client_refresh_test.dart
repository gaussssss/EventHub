import 'dart:typed_data';

import 'package:dio/dio.dart';
import 'package:eventhub/core/error/failure.dart';
import 'package:eventhub/core/network/api_client.dart';
import 'package:eventhub/core/storage/token_storage.dart';
import 'package:flutter_test/flutter_test.dart';

/// TokenStorage inerte (aucun keychain en test).
class _FakeTokenStorage extends TokenStorage {
  @override
  Future<String?> readAccessToken() async => 'stale-token';
}

/// Adaptateur Dio qui rejoue une file de réponses (statut + corps) dans l'ordre.
class _QueueAdapter implements HttpClientAdapter {
  final List<({int status, String body})> _responses;
  int calls = 0;

  _QueueAdapter(this._responses);

  @override
  Future<ResponseBody> fetch(RequestOptions options,
      Stream<Uint8List>? requestStream, Future<void>? cancelFuture) async {
    final r = _responses[calls.clamp(0, _responses.length - 1)];
    calls++;
    return ResponseBody.fromString(r.body, r.status, headers: {
      Headers.contentTypeHeader: [Headers.jsonContentType],
    });
  }

  @override
  void close({bool force = false}) {}
}

void main() {
  test('rafraîchit le jeton sur 401 puis rejoue la requête (succès)', () async {
    final adapter = _QueueAdapter([
      (status: 401, body: '{"detail":"expired"}'),
      (status: 200, body: '{"ok":true}'),
    ]);
    var refreshCalls = 0;
    var unauthorizedCalls = 0;

    final client = ApiClient(
      _FakeTokenStorage(),
      dio: Dio(BaseOptions(baseUrl: 'http://test'))..httpClientAdapter = adapter,
      onRefreshToken: () async {
        refreshCalls++;
        return 'fresh-token';
      },
      onUnauthorized: () => unauthorizedCalls++,
    );

    final data = await client.get('/api/me');

    expect(data, {'ok': true});
    expect(refreshCalls, 1); // un seul refresh
    expect(adapter.calls, 2); // requête initiale + rejeu
    expect(unauthorizedCalls, 0); // pas de déconnexion : le refresh a réussi
  });

  test('déconnecte quand le refresh échoue sur 401', () async {
    final adapter = _QueueAdapter([
      (status: 401, body: '{"detail":"expired"}'),
    ]);
    var unauthorizedCalls = 0;

    final client = ApiClient(
      _FakeTokenStorage(),
      dio: Dio(BaseOptions(baseUrl: 'http://test'))..httpClientAdapter = adapter,
      onRefreshToken: () async => null, // refresh impossible
      onUnauthorized: () => unauthorizedCalls++,
    );

    await expectLater(
      client.get('/api/me'),
      throwsA(isA<Failure>()
          .having((f) => f.type, 'type', FailureType.unauthorized)),
    );
    expect(unauthorizedCalls, 1); // déconnexion déclenchée
  });
}
