import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/widgets/async_value_widget.dart';
import '../../domain/entities/leaderboard_entry.dart';
import '../../domain/entities/user_profile.dart';
import '../providers/profile_provider.dart';

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
            _LeaderboardSection(
                async: ref.watch(leaderboardProvider),
                onRetry: () => ref.invalidate(leaderboardProvider)),
          ],
        ),
      ),
        ),
    );
  }
}

/// Section « Classement » : podium Top 3 (si assez d'entrées) puis les rangs
/// suivants. Robuste à un nombre variable d'entrées (0, 1, 2, ou plus).
class _LeaderboardSection extends StatelessWidget {
  final AsyncValue<List<LeaderboardEntry>> async;
  final VoidCallback onRetry;

  const _LeaderboardSection({required this.async, required this.onRetry});

  @override
  Widget build(BuildContext context) {
    return async.when(
      loading: () => const Padding(
        padding: EdgeInsets.symmetric(vertical: 32),
        child: Center(child: CircularProgressIndicator()),
      ),
      error: (_, _) => Padding(
        padding: const EdgeInsets.symmetric(vertical: 24, horizontal: 16),
        child: Column(
          children: [
            const Text('Classement indisponible',
                style: TextStyle(color: AppColors.textLight)),
            const SizedBox(height: 8),
            OutlinedButton(onPressed: onRetry, child: const Text('Réessayer')),
          ],
        ),
      ),
      data: (entries) {
        if (entries.isEmpty) {
          return const Padding(
            padding: EdgeInsets.fromLTRB(16, 0, 16, 24),
            child: Text('Aucun classement pour le moment.',
                style: TextStyle(color: AppColors.textLight)),
          );
        }

        // On limite l'affichage aux 20 premiers. Si l'utilisateur courant est
        // au-delà du 20e, on l'ajoute en dernier avec son vrai rang.
        const limit = 20;
        final capped = entries.take(limit).toList();
        final meIndex = entries.indexWhere((e) => e.isMe);
        final overflowMe = meIndex >= limit ? entries[meIndex] : null;

        final top = capped.take(3).toList();
        final rest = capped.skip(3).toList();
        final hasPodium = top.length >= 3;
        final rows = hasPodium ? rest : capped;

        return Column(
          children: [
            // Podium : rendu seulement si l'on a bien un Top 3 (ordre 2-1-3).
            if (hasPodium)
              Padding(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.end,
                  children: [
                    Expanded(child: _PodiumCard(entry: top[1])),
                    const SizedBox(width: 8),
                    Expanded(child: _PodiumCard(entry: top[0])),
                    const SizedBox(width: 8),
                    Expanded(child: _PodiumCard(entry: top[2])),
                  ],
                ),
              ),
            const SizedBox(height: 12),
            ListView.separated(
              shrinkWrap: true,
              physics: const NeverScrollableScrollPhysics(),
              padding: EdgeInsets.fromLTRB(16, 0, 16, overflowMe == null ? 24 : 8),
              itemCount: rows.length,
              separatorBuilder: (_, _) => const SizedBox(height: 8),
              itemBuilder: (context, index) => _RankRow(entry: rows[index]),
            ),
            // Ligne « c'est moi » quand l'utilisateur est hors du top 20.
            if (overflowMe != null) ...[
              const Padding(
                padding: EdgeInsets.symmetric(horizontal: 16, vertical: 2),
                child: Row(
                  children: [
                    Expanded(child: Divider(color: AppColors.divider)),
                    Padding(
                      padding: EdgeInsets.symmetric(horizontal: 8),
                      child: Text('•••',
                          style: TextStyle(color: AppColors.textLight)),
                    ),
                    Expanded(child: Divider(color: AppColors.divider)),
                  ],
                ),
              ),
              Padding(
                padding: const EdgeInsets.fromLTRB(16, 0, 16, 24),
                child: _RankRow(entry: overflowMe),
              ),
            ],
          ],
        );
      },
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

/// Fournisseur d'image d'avatar, ou `null` si aucune URL (→ initiale de repli).
ImageProvider? _avatarImage(String? url) =>
    (url != null && url.isNotEmpty) ? CachedNetworkImageProvider(url) : null;

String _initial(String name) =>
    name.trim().isEmpty ? '?' : name.trim()[0].toUpperCase();

class _PodiumCard extends StatelessWidget {
  final LeaderboardEntry entry;

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
        color: entry.isMe
            ? AppColors.primary.withValues(alpha: 0.08)
            : AppColors.surface,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(
          color: entry.isMe
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
                backgroundImage: _avatarImage(entry.avatarUrl),
                backgroundColor: AppColors.divider,
                child: _avatarImage(entry.avatarUrl) == null
                    ? Text(_initial(entry.name),
                        style: TextStyle(
                            fontSize: isFirst ? 22 : 18,
                            fontWeight: FontWeight.bold,
                            color: AppColors.textMedium))
                    : null,
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
          if (entry.isMe) ...[
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
  final LeaderboardEntry entry;

  const _RankRow({required this.entry});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: entry.isMe
            ? AppColors.primary.withValues(alpha: 0.06)
            : AppColors.surface,
        borderRadius: BorderRadius.circular(14),
        border: entry.isMe
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
            backgroundImage: _avatarImage(entry.avatarUrl),
            backgroundColor: AppColors.divider,
            child: _avatarImage(entry.avatarUrl) == null
                ? Text(_initial(entry.name),
                    style: const TextStyle(
                        fontWeight: FontWeight.bold,
                        color: AppColors.textMedium))
                : null,
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
          if (entry.isMe)
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
