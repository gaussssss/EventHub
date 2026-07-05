import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/network/network_providers.dart';
import '../../data/datasources/stats_remote_datasource.dart';
import '../../domain/entities/community_stats.dart';

final statsRemoteDataSourceProvider = Provider<StatsRemoteDataSource>((ref) {
  return StatsRemoteDataSource(ref.watch(apiClientProvider));
});

/// Statistiques communautaires. En mock : valeurs de démonstration ; sinon
/// `GET /api/stats/community`. Les scalaires dérivés (inscrits, cœurs UQTR)
/// retombent sur ces valeurs tant que la requête n'a pas abouti.
final communityStatsProvider = FutureProvider<CommunityStats>((ref) async {
  if (AppConfig.useMockData) {
    await Future.delayed(AppConfig.mockLatency);
    return const CommunityStats(totalRegisteredUsers: 1243, totalUqtrHearts: 12480);
  }
  return ref.watch(statsRemoteDataSourceProvider).getCommunityStats();
});
