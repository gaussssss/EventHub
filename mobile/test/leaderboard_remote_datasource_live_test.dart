@Tags(['live'])
library;

import 'package:eventhub/core/network/api_client.dart';
import 'package:eventhub/core/storage/token_storage.dart';
import 'package:eventhub/features/profile/data/datasources/leaderboard_remote_datasource.dart';
import 'package:flutter_test/flutter_test.dart';

/// Test live : API en marche (http://localhost:5199).
///   flutter test --run-skipped --tags live test/leaderboard_remote_datasource_live_test.dart
///
/// L'endpoint `/api/leaderboard` n'exige pas d'auth (le drapeau « c'est moi »
/// vient du Bearer si présent) → appelable sans jeton.
class _NoTokenStorage extends TokenStorage {
  @override
  Future<String?> readAccessToken() async => null;
}

void main() {
  test('LeaderboardRemoteDataSource récupère et parse le classement', () async {
    final dataSource = LeaderboardRemoteDataSource(ApiClient(_NoTokenStorage()));

    final rows = await dataSource.getLeaderboard();

    // La requête aboutit et se parse : rangs 1-indexés, cœurs ≥ 0.
    for (final row in rows) {
      expect(row.rank, greaterThanOrEqualTo(1));
      expect(row.hearts, greaterThanOrEqualTo(0));
      expect(row.name, isNotEmpty);
    }
  });
}
