import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/widgets/async_value_widget.dart';
import '../../domain/entities/user_profile.dart';
import '../providers/profile_provider.dart';

class _LeaderboardEntry {
  final int rank;
  final String name;
  final String avatarUrl;
  final int hearts;
  final bool isCurrentUser;

  const _LeaderboardEntry({
    required this.rank,
    required this.name,
    required this.avatarUrl,
    required this.hearts,
    this.isCurrentUser = false,
  });
}

const _leaderboard = [
  _LeaderboardEntry(
    rank: 1,
    name: 'Sophie Martin',
    avatarUrl: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150&fit=crop&q=80',
    hearts: 520,
  ),
  _LeaderboardEntry(
    rank: 2,
    name: 'Jean Lapointe',
    avatarUrl: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&fit=crop&q=80',
    hearts: 480,
  ),
  _LeaderboardEntry(
    rank: 3,
    name: 'Alex Tremblay',
    avatarUrl: 'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&fit=crop&q=80',
    hearts: 340,
    isCurrentUser: true,
  ),
  _LeaderboardEntry(
    rank: 4,
    name: 'Marie Tremblay',
    avatarUrl: 'https://images.unsplash.com/photo-1494790108755-2616b612b74c?w=150&fit=crop&q=80',
    hearts: 280,
  ),
  _LeaderboardEntry(
    rank: 5,
    name: 'Camille Roy',
    avatarUrl: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&fit=crop&q=80',
    hearts: 195,
  ),
];

class HeartsPage extends ConsumerWidget {
  const HeartsPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final userAsync = ref.watch(currentUserProvider);
    final uqtrHearts = ref.watch(totalUqtrHeartsProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.surface,
        leading: IconButton(
          icon: const Icon(Iconsax.arrow_left),
          onPressed: () => context.pop(),
        ),
        title: const Text('Cœurs santé'),
      ),
      body: AsyncValueWidget<UserProfile>(
        value: userAsync,
        onRetry: () => ref.invalidate(currentUserProvider),
        data: (user) => SingleChildScrollView(
        child: Column(
          children: [
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(24),
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topLeft,
                  end: Alignment.bottomRight,
                  colors: [Color(0xFF006534), Color(0xFF1A7A4A)],
                ),
              ),
              child: Column(
                children: [
                  const Icon(Iconsax.heart_copy,
                      color: Colors.white, size: 48),
                  const SizedBox(height: 12),
                  Text(
                    '${user.totalHearts}',
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 56,
                      fontWeight: FontWeight.bold,
                      height: 1,
                    ),
                  ),
                  const SizedBox(height: 4),
                  const Text(
                    'cœurs santé accumulés',
                    style: TextStyle(
                        color: Colors.white70, fontSize: 16),
                  ),
                  const SizedBox(height: 20),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: [
                      _LevelBadge(
                          level: 'Bronze',
                          threshold: 200,
                          current: user.totalHearts,
                          isActive: user.level == 'Bronze'),
                      _LevelConnector(
                          reached: user.totalHearts >= 200),
                      _LevelBadge(
                          level: 'Argent',
                          threshold: 500,
                          current: user.totalHearts,
                          isActive: user.level == 'Argent'),
                      _LevelConnector(
                          reached: user.totalHearts >= 500),
                      _LevelBadge(
                          level: 'Or',
                          threshold: 1000,
                          current: user.totalHearts,
                          isActive: user.level == 'Or'),
                    ],
                  ),
                  const SizedBox(height: 20),
                  ClipRRect(
                    borderRadius: BorderRadius.circular(6),
                    child: LinearProgressIndicator(
                      value: ((user.totalHearts -
                                  user.previousLevelThreshold) /
                              (user.nextLevelThreshold -
                                  user.previousLevelThreshold))
                          .clamp(0.0, 1.0),
                      backgroundColor:
                          Colors.white.withValues(alpha: 0.25),
                      valueColor: const AlwaysStoppedAnimation<Color>(
                          Colors.white),
                      minHeight: 10,
                    ),
                  ),
                  const SizedBox(height: 8),
                  Text(
                    '${user.nextLevelThreshold - user.totalHearts} cœurs pour atteindre le niveau suivant',
                    style: const TextStyle(
                        color: Colors.white70, fontSize: 13),
                    textAlign: TextAlign.center,
                  ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(16),
              child: Container(
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(
                  color: AppColors.secondary.withValues(alpha: 0.08),
                  borderRadius: BorderRadius.circular(16),
                  border: Border.all(
                    color: AppColors.secondary.withValues(alpha: 0.3),
                  ),
                ),
                child: Row(
                  children: [
                    const Icon(Iconsax.people,
                        color: AppColors.secondary, size: 28),
                    const SizedBox(width: 14),
                    Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        const Text(
                          'Total communauté UQTR',
                          style: TextStyle(
                            fontSize: 13,
                            color: AppColors.textMedium,
                          ),
                        ),
                        Text(
                          '$uqtrHearts cœurs santé',
                          style: const TextStyle(
                            fontSize: 20,
                            fontWeight: FontWeight.bold,
                            color: AppColors.secondary,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            const Padding(
              padding: EdgeInsets.fromLTRB(16, 8, 16, 16),
              child: Text(
                'Classement',
                style: TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.bold,
                  color: AppColors.textDark,
                ),
              ),
            ),
            // Podium Top 3
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: Row(
                crossAxisAlignment: CrossAxisAlignment.end,
                children: [
                  Expanded(child: _PodiumCard(entry: _leaderboard[1])),
                  const SizedBox(width: 8),
                  Expanded(child: _PodiumCard(entry: _leaderboard[0])),
                  const SizedBox(width: 8),
                  Expanded(child: _PodiumCard(entry: _leaderboard[2])),
                ],
              ),
            ),
            const SizedBox(height: 12),
            // Top 4 & 5
            ListView.separated(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              padding: const EdgeInsets.fromLTRB(16, 0, 16, 24),
              itemCount: 2,
              separatorBuilder: (_, _) => const SizedBox(height: 8),
              itemBuilder: (context, index) =>
                  _RankRow(entry: _leaderboard[3 + index]),
            ),
          ],
        ),
      ),
        ),
    );
  }
}

class _LevelBadge extends StatelessWidget {
  final String level;
  final int threshold;
  final int current;
  final bool isActive;

  const _LevelBadge({
    required this.level,
    required this.threshold,
    required this.current,
    required this.isActive,
  });

  @override
  Widget build(BuildContext context) {
    final reached = current >= (level == 'Bronze' ? 0 : threshold);
    return Column(
      children: [
        Container(
          width: 44,
          height: 44,
          decoration: BoxDecoration(
            color: reached
                ? Colors.white
                : Colors.white.withValues(alpha: 0.2),
            shape: BoxShape.circle,
            border: isActive
                ? Border.all(color: AppColors.secondary, width: 2)
                : null,
          ),
          child: Center(
            child: Text(
              level == 'Bronze'
                  ? '🥉'
                  : level == 'Argent'
                      ? '🥈'
                      : '🥇',
              style: TextStyle(
                  fontSize: 20,
                  color: reached ? null : Colors.transparent),
            ),
          ),
        ),
        const SizedBox(height: 4),
        Text(
          level,
          style: TextStyle(
            color: reached ? Colors.white : Colors.white38,
            fontSize: 11,
            fontWeight:
                isActive ? FontWeight.bold : FontWeight.normal,
          ),
        ),
      ],
    );
  }
}

class _LevelConnector extends StatelessWidget {
  final bool reached;

  const _LevelConnector({required this.reached});

  @override
  Widget build(BuildContext context) {
    return Container(
      width: 32,
      height: 2,
      margin: const EdgeInsets.only(bottom: 20),
      color: reached
          ? Colors.white
          : Colors.white.withValues(alpha: 0.25),
    );
  }
}

class _PodiumCard extends StatelessWidget {
  final _LeaderboardEntry entry;

  const _PodiumCard({required this.entry});

  @override
  Widget build(BuildContext context) {
    final isFirst = entry.rank == 1;
    final colors = {
      1: const Color(0xFFFFD700),
      2: const Color(0xFFB0BEC5),
      3: const Color(0xFFCD7F32),
    };
    final rankColor = colors[entry.rank]!;
    final height = isFirst ? 180.0 : 150.0;

    return Container(
      height: height,
      decoration: BoxDecoration(
        color: entry.isCurrentUser
            ? AppColors.primary.withValues(alpha: 0.08)
            : AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: entry.isCurrentUser
              ? AppColors.primary.withValues(alpha: 0.4)
              : rankColor.withValues(alpha: 0.5),
          width: isFirst ? 2 : 1.5,
        ),
        boxShadow: isFirst
            ? [
                BoxShadow(
                  color: rankColor.withValues(alpha: 0.25),
                  blurRadius: 12,
                  offset: const Offset(0, 4),
                )
              ]
            : null,
      ),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          Stack(
            clipBehavior: Clip.none,
            children: [
              CircleAvatar(
                radius: isFirst ? 30 : 24,
                backgroundImage:
                    CachedNetworkImageProvider(entry.avatarUrl),
                backgroundColor: AppColors.divider,
              ),
              Positioned(
                bottom: -4,
                right: -4,
                child: Container(
                  width: isFirst ? 24 : 20,
                  height: isFirst ? 24 : 20,
                  decoration: BoxDecoration(
                    color: rankColor,
                    shape: BoxShape.circle,
                    border: const Border.fromBorderSide(
                        BorderSide(color: Colors.white, width: 2)),
                  ),
                  child: Center(
                    child: Text(
                      '${entry.rank}',
                      style: TextStyle(
                        fontSize: isFirst ? 11 : 9,
                        fontWeight: FontWeight.bold,
                        color: entry.rank == 1
                            ? Colors.black87
                            : Colors.white,
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: 10),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: 6),
            child: Text(
              entry.name.split(' ').first,
              style: TextStyle(
                fontSize: isFirst ? 13 : 12,
                fontWeight: FontWeight.bold,
                color: AppColors.textDark,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
              textAlign: TextAlign.center,
            ),
          ),
          const SizedBox(height: 4),
          Row(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Icon(Iconsax.heart_copy,
                  size: isFirst ? 12 : 10, color: AppColors.heart),
              const SizedBox(width: 3),
              Text(
                '${entry.hearts}',
                style: TextStyle(
                  fontSize: isFirst ? 14 : 12,
                  fontWeight: FontWeight.bold,
                  color: AppColors.heart,
                ),
              ),
            ],
          ),
          if (entry.isCurrentUser) ...[
            const SizedBox(height: 4),
            Container(
              padding: const EdgeInsets.symmetric(
                  horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: AppColors.primary,
                borderRadius: BorderRadius.circular(6),
              ),
              child: const Text(
                'Vous',
                style: TextStyle(
                    color: Colors.white,
                    fontSize: 10,
                    fontWeight: FontWeight.bold),
              ),
            ),
          ],
        ],
      ),
    );
  }
}

class _RankRow extends StatelessWidget {
  final _LeaderboardEntry entry;

  const _RankRow({required this.entry});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: entry.isCurrentUser
            ? AppColors.primary.withValues(alpha: 0.06)
            : AppColors.surface,
        borderRadius: BorderRadius.circular(14),
        border: entry.isCurrentUser
            ? Border.all(
                color: AppColors.primary.withValues(alpha: 0.3))
            : null,
      ),
      child: Row(
        children: [
          SizedBox(
            width: 28,
            child: Text(
              '${entry.rank}',
              style: TextStyle(
                fontSize: 16,
                fontWeight: FontWeight.bold,
                color: AppColors.textLight,
              ),
              textAlign: TextAlign.center,
            ),
          ),
          const SizedBox(width: 10),
          CircleAvatar(
            radius: 20,
            backgroundImage:
                CachedNetworkImageProvider(entry.avatarUrl),
            backgroundColor: AppColors.divider,
          ),
          const SizedBox(width: 12),
          Expanded(
            child: Text(
              entry.name,
              style: const TextStyle(
                fontWeight: FontWeight.w600,
                fontSize: 14,
                color: AppColors.textDark,
              ),
            ),
          ),
          if (entry.isCurrentUser)
            Container(
              margin: const EdgeInsets.only(right: 8),
              padding: const EdgeInsets.symmetric(
                  horizontal: 6, vertical: 2),
              decoration: BoxDecoration(
                color: AppColors.primary,
                borderRadius: BorderRadius.circular(6),
              ),
              child: const Text(
                'Vous',
                style: TextStyle(
                    color: Colors.white,
                    fontSize: 10,
                    fontWeight: FontWeight.bold),
              ),
            ),
          Row(
            children: [
              const Icon(Iconsax.heart_copy,
                  size: 13, color: AppColors.heart),
              const SizedBox(width: 4),
              Text(
                '${entry.hearts}',
                style: const TextStyle(
                  fontWeight: FontWeight.bold,
                  fontSize: 14,
                  color: AppColors.heart,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}
