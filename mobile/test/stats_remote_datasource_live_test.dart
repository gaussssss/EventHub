@Tags(['live'])
library;

import 'package:eventhub/core/network/api_client.dart';
import 'package:eventhub/core/storage/token_storage.dart';
import 'package:eventhub/features/stats/data/datasources/stats_remote_datasource.dart';
import 'package:flutter_test/flutter_test.dart';

/// Test live : API en marche (http://localhost:5199).
///   flutter test --run-skipped --tags live test/stats_remote_datasource_live_test.dart
class _NoTokenStorage extends TokenStorage {
  @override
  Future<String?> readAccessToken() async => null;
}

void main() {
  test('StatsRemoteDataSource récupère et parse les stats communautaires', () async {
    final dataSource = StatsRemoteDataSource(ApiClient(_NoTokenStorage()));

    final stats = await dataSource.getCommunityStats();

    // Endpoint public : la requête aboutit et se parse (valeurs ≥ 0).
    expect(stats.totalRegisteredUsers, greaterThanOrEqualTo(0));
    expect(stats.totalUqtrHearts, greaterThanOrEqualTo(0));
  });
}
