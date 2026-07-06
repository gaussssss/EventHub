import '../../../../core/network/api_client.dart';
import '../../domain/entities/leaderboard_entry.dart';
import '../models/leaderboard_entry_model.dart';

/// Accès distant au classement des cœurs (API REST EventHub).
/// La route joint le `Bearer` Entra pour marquer la ligne « c'est moi ».
class LeaderboardRemoteDataSource {
  final ApiClient _client;

  LeaderboardRemoteDataSource(this._client);

  /// `GET /api/leaderboard?page=N` → classement paginé (rang 1-indexé).
  Future<List<LeaderboardEntry>> getLeaderboard({int page = 1}) async {
    final data = await _client.get('/api/leaderboard', query: {'page': page});
    return (data as List)
        .map((json) =>
            LeaderboardEntryModel.fromJson(json as Map<String, dynamic>))
        .toList();
  }
}
