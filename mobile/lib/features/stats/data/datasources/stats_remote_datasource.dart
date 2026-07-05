import '../../../../core/network/api_client.dart';
import '../../domain/entities/community_stats.dart';
import '../models/community_stats_model.dart';

/// Accès distant aux statistiques communautaires (API REST EventHub).
class StatsRemoteDataSource {
  final ApiClient _client;

  StatsRemoteDataSource(this._client);

  /// `GET /api/stats/community` → totaux inscrits + cœurs UQTR.
  Future<CommunityStats> getCommunityStats() async {
    final data = await _client.get('/api/stats/community');
    return CommunityStatsModel.fromJson(data as Map<String, dynamic>);
  }
}
