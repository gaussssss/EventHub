import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/config/app_config.dart';
import '../../../stats/presentation/providers/stats_provider.dart';
import '../../../../core/network/network_providers.dart';
import '../../domain/entities/leaderboard_entry.dart';
import '../../domain/entities/user_profile.dart';
import '../../data/datasources/leaderboard_remote_datasource.dart';
import '../../data/datasources/profile_local_datasource.dart';
import '../../data/datasources/profile_remote_datasource.dart';

final profileDataSourceProvider = Provider<ProfileLocalDataSource>((ref) {
  return ProfileLocalDataSource();
});

final profileRemoteDataSourceProvider =
    Provider<ProfileRemoteDataSource>((ref) {
  return ProfileRemoteDataSource(ref.watch(apiClientProvider));
});

final leaderboardRemoteDataSourceProvider =
    Provider<LeaderboardRemoteDataSource>((ref) {
  return LeaderboardRemoteDataSource(ref.watch(apiClientProvider));
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

/// Classement des cœurs (`GET /api/leaderboard`). Sous **auth réelle**, on lit
/// le vrai classement (la ligne « c'est moi » vient du Bearer) ; sinon on
/// retombe sur un jeu de démonstration.
final leaderboardProvider = FutureProvider<List<LeaderboardEntry>>((ref) async {
  if (AppConfig.useRealAuth) {
    return ref.watch(leaderboardRemoteDataSourceProvider).getLeaderboard();
  }
  if (AppConfig.useMockData) await Future.delayed(AppConfig.mockLatency);
  return _mockLeaderboard;
});

const _mockLeaderboard = <LeaderboardEntry>[
  LeaderboardEntry(
      rank: 1,
      name: 'Sophie Martin',
      hearts: 520,
      avatarUrl:
          'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150&fit=crop&q=80'),
  LeaderboardEntry(
      rank: 2,
      name: 'Jean Lapointe',
      hearts: 480,
      avatarUrl:
          'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&fit=crop&q=80'),
  LeaderboardEntry(
      rank: 3,
      name: 'Alex Tremblay',
      hearts: 340,
      isMe: true,
      avatarUrl:
          'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&fit=crop&q=80'),
  LeaderboardEntry(
      rank: 4,
      name: 'Marie Tremblay',
      hearts: 280,
      avatarUrl:
          'https://images.unsplash.com/photo-1494790108755-2616b612b74c?w=150&fit=crop&q=80'),
  LeaderboardEntry(
      rank: 5,
      name: 'Camille Roy',
      hearts: 195,
      avatarUrl:
          'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&fit=crop&q=80'),
];

/// Total des cœurs UQTR (badge), dérivé des stats communautaires
/// (`GET /api/stats/community`) avec repli sur une valeur d'affichage.
final totalUqtrHeartsProvider = Provider<int>((ref) {
  return ref.watch(communityStatsProvider).valueOrNull?.totalUqtrHearts ?? 12480;
});

final avatarUrlProvider = StateProvider<String?>((ref) => null);
