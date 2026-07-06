import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/network/network_providers.dart';
import '../../data/datasources/stats_remote_datasource.dart';
import '../../domain/entities/community_stats.dart';

final statsRemoteDataSourceProvider = Provider<StatsRemoteDataSource>((ref) {
  return StatsRemoteDataSource(ref.watch(apiClientProvider));
});

/// Statistiques communautaires (`GET /api/stats/community`, endpoint public).
/// Live dès que l'app n'est pas en mock (auth réelle configurée) ; sinon valeurs
/// de démonstration. Les scalaires dérivés (inscrits, cœurs UQTR) retombent sur
/// le mock tant que la requête n'a pas abouti.
final communityStatsProvider = FutureProvider<CommunityStats>((ref) async {
  if (AppConfig.useMockData) {
    await Future.delayed(AppConfig.mockLatency);
    return const CommunityStats(totalRegisteredUsers: 1243, totalUqtrHearts: 12480);
  }
  return ref.watch(statsRemoteDataSourceProvider).getCommunityStats();
});
