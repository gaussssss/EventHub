import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/config/app_config.dart';
import '../../../stats/presentation/providers/stats_provider.dart';
import '../../../../core/network/network_providers.dart';
import '../../domain/entities/user_profile.dart';
import '../../data/datasources/profile_local_datasource.dart';
import '../../data/datasources/profile_remote_datasource.dart';

final profileDataSourceProvider = Provider<ProfileLocalDataSource>((ref) {
  return ProfileLocalDataSource();
});

final profileRemoteDataSourceProvider =
    Provider<ProfileRemoteDataSource>((ref) {
  return ProfileRemoteDataSource(ref.watch(apiClientProvider));
});

/// Profil de l'utilisateur connecté (`GET /api/me`). Sous **auth réelle Entra**,
/// on lit le vrai profil via le Bearer ; sinon on retombe sur le mock (login
/// mock / dev). C'est l'identité affichée : elle doit refléter qui est connecté.
final currentUserProvider = FutureProvider<UserProfile>((ref) async {
  if (AppConfig.useRealAuth) {
    return ref.watch(profileRemoteDataSourceProvider).getMyProfile();
  }
  if (AppConfig.useMockData) await Future.delayed(AppConfig.mockLatency);
  return ref.watch(profileDataSourceProvider).currentUser;
});

/// Total des cœurs UQTR (badge), dérivé des stats communautaires
/// (`GET /api/stats/community`) avec repli sur une valeur d'affichage.
final totalUqtrHeartsProvider = Provider<int>((ref) {
  return ref.watch(communityStatsProvider).valueOrNull?.totalUqtrHearts ?? 12480;
});

final avatarUrlProvider = StateProvider<String?>((ref) => null);
