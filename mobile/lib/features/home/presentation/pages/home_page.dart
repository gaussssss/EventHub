import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:smooth_page_indicator/smooth_page_indicator.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/date_formatter.dart';
import '../../../activities/domain/entities/activity.dart';
import '../../../activities/presentation/providers/activity_provider.dart';
import '../../../activities/presentation/widgets/activity_filter_sheet.dart';
import '../../../profile/presentation/providers/profile_provider.dart';
import '../../../social/presentation/providers/post_provider.dart';
import '../../../stats/presentation/providers/stats_provider.dart';
import '../../../social/presentation/widgets/post_card.dart';
import '../widgets/category_filter_bar.dart';
import '../widgets/mini_calendar.dart';

class HomePage extends ConsumerWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    // Tableau de bord : chaque section s'affiche dès que sa donnée arrive
    // (fallback liste vide / profil null pendant le chargement).
    final activities =
        ref.watch(filteredActivitiesProvider).valueOrNull ?? const [];
    final posts = ref.watch(allPostsProvider).valueOrNull ?? const [];
    final user = ref.watch(currentUserProvider).valueOrNull;
    final totalUsers = ref.watch(totalAppUsersProvider);

    final featuredActivities =
        ref.watch(featuredActivitiesProvider).valueOrNull ?? const [];

    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: RefreshIndicator(
          color: AppColors.primary,
          onRefresh: () async {
            ref.invalidate(allActivitiesProvider);
            ref.invalidate(filteredActivitiesProvider);
            ref.invalidate(featuredActivitiesProvider);
            ref.invalidate(allPostsProvider);
            ref.invalidate(communityStatsProvider);
            ref.invalidate(currentUserProvider);
            await Future.wait([
              ref.read(featuredActivitiesProvider.future),
              ref.read(allPostsProvider.future),
            ]);
          },
          child: CustomScrollView(
            slivers: [
            SliverAppBar(
              backgroundColor: AppColors.background,
              surfaceTintColor: Colors.transparent,
              floating: true,
              snap: true,
              elevation: 0,
              title: Row(
                children: [
                  Container(
                    width: 34,
                    height: 34,
                    decoration: BoxDecoration(
                      gradient: const LinearGradient(
                        colors: [AppColors.primary, AppColors.primaryGlow],
                        begin: Alignment.topLeft,
                        end: Alignment.bottomRight,
                      ),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: const Icon(Iconsax.heart_copy,
                        color: Colors.white, size: 18),
                  ),
                  const SizedBox(width: 10),
                  const Text(
                    'EventHub',
                    style: TextStyle(
                      fontWeight: FontWeight.w700,
                      fontSize: 18,
                      letterSpacing: -0.3,
                    ),
                  ),
                ],
              ),
              actions: [
                _AppUsersBadge(totalUsers: totalUsers),
                const SizedBox(width: 8),
                GestureDetector(
                  onTap: () => context.go('/catalogue'),
                  child: Container(
                    padding: const EdgeInsets.symmetric(
                        horizontal: 10, vertical: 6),
                    margin: const EdgeInsets.only(right: 12),
                    decoration: BoxDecoration(
                      color: AppColors.heart.withValues(alpha: 0.1),
                      borderRadius: BorderRadius.circular(20),
                    ),
                    child: Row(
                      children: [
                        const Icon(Iconsax.heart_copy,
                            color: AppColors.heart, size: 14),
                        const SizedBox(width: 5),
                        Text(
                          '${user?.totalHearts ?? 0}',
                          style: const TextStyle(
                            fontSize: 14,
                            fontWeight: FontWeight.w700,
                            color: AppColors.heart,
                          ),
                        ),
                      ],
                    ),
                  ),
                ),
              ],
            ),
            const SliverToBoxAdapter(child: SizedBox(height: 16)),
            const SliverToBoxAdapter(child: CategoryFilterBar()),
            const SliverToBoxAdapter(child: SizedBox(height: 20)),
            if (featuredActivities.isNotEmpty)
              SliverToBoxAdapter(
                child: _FeaturedCarousel(activities: featuredActivities),
              ),
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(20, 28, 20, 12),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Mes inscriptions',
                      style: TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.w700,
                        color: AppColors.textDark,
                        letterSpacing: -0.4,
                      ),
                    ),
                    GestureDetector(
                      onTap: () => _showFilterSheet(context, ref),
                      child: Container(
                        padding: const EdgeInsets.all(8),
                        decoration: BoxDecoration(
                          color: AppColors.surface,
                          borderRadius: BorderRadius.circular(10),
                          boxShadow: const [
                            BoxShadow(
                              color: AppColors.cardShadow,
                              blurRadius: 8,
                              offset: Offset(0, 2),
                            ),
                          ],
                        ),
                        child: const Icon(Iconsax.filter,
                            color: AppColors.textMedium, size: 20),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SliverToBoxAdapter(child: MiniCalendar()),
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(20, 28, 20, 12),
                child: const Text(
                  'Activités à venir',
                  style: TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.w700,
                    color: AppColors.textDark,
                    letterSpacing: -0.4,
                  ),
                ),
              ),
            ),
            SliverToBoxAdapter(
              child: SizedBox(
                height: 186,
                child: ListView.separated(
                  scrollDirection: Axis.horizontal,
                  padding: const EdgeInsets.symmetric(horizontal: 20),
                  itemCount: activities.length.clamp(0, 8),
                  separatorBuilder: (_, _) => const SizedBox(width: 12),
                  itemBuilder: (context, index) {
                    final activity = activities[index];
                    return _MiniActivityCard(
                      activity: activity,
                      onTap: () =>
                          context.push('/activity/${activity.id}'),
                    );
                  },
                ),
              ),
            ),
            SliverToBoxAdapter(
              child: Padding(
                padding: const EdgeInsets.fromLTRB(20, 28, 20, 12),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.spaceBetween,
                  children: [
                    const Text(
                      'Fil communautaire',
                      style: TextStyle(
                        fontSize: 20,
                        fontWeight: FontWeight.w700,
                        color: AppColors.textDark,
                        letterSpacing: -0.4,
                      ),
                    ),
                    GestureDetector(
                      onTap: () => context.push('/create-post'),
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 12, vertical: 7),
                        decoration: BoxDecoration(
                          color: AppColors.primary.withValues(alpha: 0.1),
                          borderRadius: BorderRadius.circular(20),
                        ),
                        child: const Row(
                          children: [
                            Icon(Iconsax.add,
                                size: 16, color: AppColors.primary),
                            SizedBox(width: 4),
                            Text(
                              'Publier',
                              style: TextStyle(
                                fontSize: 13,
                                color: AppColors.primary,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ],
                ),
              ),
            ),
            SliverPadding(
              padding: const EdgeInsets.fromLTRB(20, 0, 20, 100),
              sliver: SliverList.separated(
                itemCount: posts.length,
                separatorBuilder: (_, _) => const SizedBox(height: 16),
                itemBuilder: (context, index) {
                  final post = posts[index];
                  return PostCard(
                    post: post,
                    onTap: () => context.push('/post/${post.id}'),
                  );
                },
              ),
            ),
            ],
          ),
        ),
      ),
    );
  }

  void _showFilterSheet(BuildContext context, WidgetRef ref) {
    ActivityFilterSheet.show(
      context,
      // Après "Appliquer", on bascule vers le catalogue qui affiche les résultats.
      onApplied: () => context.go('/catalogue'),
    );
  }
}

class _AppUsersBadge extends StatelessWidget {
  final int totalUsers;
  const _AppUsersBadge({required this.totalUsers});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      decoration: BoxDecoration(
        color: AppColors.primary.withValues(alpha: 0.08),
        borderRadius: BorderRadius.circular(20),
      ),
      child: Row(
        children: [
          const Icon(Iconsax.profile_circle,
              color: AppColors.primary, size: 14),
          const SizedBox(width: 5),
          Text(
            '${_formatCount(totalUsers)} inscrits',
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w600,
              color: AppColors.primary,
            ),
          ),
        ],
      ),
    );
  }

  String _formatCount(int n) =>
      n >= 1000 ? '${(n / 1000).toStringAsFixed(1)}k' : '$n';
}

class _FeaturedCarousel extends StatefulWidget {
  final List<Activity> activities;
  const _FeaturedCarousel({required this.activities});

  @override
  State<_FeaturedCarousel> createState() => _FeaturedCarouselState();
}

class _FeaturedCarouselState extends State<_FeaturedCarousel> {
  final _controller = PageController(viewportFraction: 0.88);

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        SizedBox(
          height: 228,
          child: PageView.builder(
            controller: _controller,
            itemCount: widget.activities.length,
            itemBuilder: (context, index) {
              final activity = widget.activities[index];
              return GestureDetector(
                onTap: () =>
                    context.push('/activity/${activity.id}'),
                child: _FeaturedCard(activity: activity),
              );
            },
          ),
        ),
        const SizedBox(height: 12),
        SmoothPageIndicator(
          controller: _controller,
          count: widget.activities.length,
          effect: WormEffect(
            dotWidth: 6,
            dotHeight: 6,
            activeDotColor: AppColors.primary,
            dotColor: AppColors.divider,
            spacing: 5,
          ),
        ),
      ],
    );
  }
}

class _FeaturedCard extends StatelessWidget {
  final Activity activity;
  const _FeaturedCard({required this.activity});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 6),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(22),
        boxShadow: const [
          BoxShadow(
            color: Color(0x20000000),
            blurRadius: 20,
            offset: Offset(0, 6),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(22),
        child: Stack(
          fit: StackFit.expand,
          children: [
            CachedNetworkImage(
              imageUrl: activity.imageUrl,
              fit: BoxFit.cover,
              placeholder: (_, _) =>
                  Container(color: AppColors.divider),
              errorWidget: (_, _, _) =>
                  Container(color: AppColors.divider),
            ),
            Container(
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: [Colors.transparent, Color(0xCC000000)],
                  stops: [0.35, 1.0],
                ),
              ),
            ),
            Positioned(
              top: 14,
              left: 14,
              child: Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                decoration: BoxDecoration(
                  color: Colors.black.withValues(alpha: 0.45),
                  borderRadius: BorderRadius.circular(8),
                ),
                child: Text(
                  activity.categoryLabel,
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 11,
                    fontWeight: FontWeight.w700,
                    letterSpacing: 0.3,
                  ),
                ),
              ),
            ),
            Positioned(
              top: 12,
              right: 12,
              child: Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 10, vertical: 5),
                decoration: BoxDecoration(
                  color: Colors.black.withValues(alpha: 0.45),
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Row(
                  children: [
                    const Icon(Iconsax.heart_copy,
                        color: AppColors.heart, size: 13),
                    const SizedBox(width: 4),
                    Text(
                      '+${activity.hearts}',
                      style: const TextStyle(
                        color: Colors.white,
                        fontSize: 12,
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                  ],
                ),
              ),
            ),
            Positioned(
              bottom: 16,
              left: 16,
              right: 16,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    activity.title,
                    style: const TextStyle(
                      color: Colors.white,
                      fontSize: 19,
                      fontWeight: FontWeight.w700,
                      height: 1.2,
                      letterSpacing: -0.3,
                    ),
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 8),
                  Row(
                    children: [
                      const Icon(Iconsax.calendar,
                          color: Colors.white70, size: 12),
                      const SizedBox(width: 4),
                      Text(
                        DateFormatter.dateFull(activity.date),
                        style: const TextStyle(
                            color: Colors.white70, fontSize: 12),
                      ),
                      const SizedBox(width: 12),
                      const Icon(Iconsax.clock,
                          color: Colors.white70, size: 12),
                      const SizedBox(width: 4),
                      Text(
                        DateFormatter.time(activity.date),
                        style: const TextStyle(
                            color: Colors.white70, fontSize: 12),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _MiniActivityCard extends StatelessWidget {
  final Activity activity;
  final VoidCallback onTap;

  const _MiniActivityCard({
    required this.activity,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final spotsLeft = activity.maxParticipants - activity.currentParticipants;
    final isFull = spotsLeft <= 0;

    return GestureDetector(
      onTap: onTap,
      child: Container(
        width: 148,
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(18),
          boxShadow: const [
            BoxShadow(
              color: AppColors.cardShadow,
              blurRadius: 10,
              offset: Offset(0, 3),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            ClipRRect(
              borderRadius: const BorderRadius.only(
                topLeft: Radius.circular(18),
                topRight: Radius.circular(18),
              ),
              child: Stack(
                children: [
                  CachedNetworkImage(
                    imageUrl: activity.imageUrl,
                    width: 148,
                    height: 92,
                    fit: BoxFit.cover,
                    placeholder: (_, _) =>
                        Container(height: 92, color: AppColors.divider),
                    errorWidget: (_, _, _) =>
                        Container(height: 92, color: AppColors.divider),
                  ),
                  if (isFull)
                    Positioned(
                      top: 6,
                      right: 6,
                      child: Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 7, vertical: 3),
                        decoration: BoxDecoration(
                          color: AppColors.heart,
                          borderRadius: BorderRadius.circular(6),
                        ),
                        child: const Text(
                          'Complet',
                          style: TextStyle(
                              color: Colors.white,
                              fontSize: 9,
                              fontWeight: FontWeight.w700),
                        ),
                      ),
                    ),
                ],
              ),
            ),
            Padding(
              padding: const EdgeInsets.fromLTRB(10, 9, 10, 9),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    activity.title,
                    style: const TextStyle(
                      fontSize: 13,
                      fontWeight: FontWeight.w700,
                      color: AppColors.textDark,
                      letterSpacing: -0.2,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 4),
                  Text(
                    DateFormatter.dateTime(activity.date),
                    style: const TextStyle(
                        fontSize: 11, color: AppColors.textLight),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 5),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Row(
                        children: [
                          const Icon(Iconsax.heart_copy,
                              size: 11, color: AppColors.heart),
                          const SizedBox(width: 3),
                          Text(
                            '+${activity.hearts}',
                            style: const TextStyle(
                              fontSize: 11,
                              fontWeight: FontWeight.w700,
                              color: AppColors.heart,
                            ),
                          ),
                        ],
                      ),
                      if (!isFull)
                        Text(
                          '$spotsLeft pl.',
                          style: const TextStyle(
                              fontSize: 10,
                              color: AppColors.textLight),
                        ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
