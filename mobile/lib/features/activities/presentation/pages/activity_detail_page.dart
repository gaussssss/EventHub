import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:share_plus/share_plus.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/date_formatter.dart';
import '../../../../core/widgets/async_value_widget.dart';
import '../../domain/entities/activity.dart';
import '../providers/activity_provider.dart';

class ActivityDetailPage extends ConsumerWidget {
  final String activityId;

  const ActivityDetailPage({super.key, required this.activityId});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final activityAsync = ref.watch(activityByIdProvider(activityId));
    final registeredIds = ref.watch(registeredActivitiesProvider);

    return activityAsync.when(
      loading: () => const Scaffold(body: Center(child: AppLoader())),
      error: (e, _) => Scaffold(
        body: AppErrorView(
          message: '$e',
          onRetry: () => ref.invalidate(allActivitiesProvider),
        ),
      ),
      data: (activity) {
        if (activity == null) {
          return const Scaffold(
            body: Center(child: Text('Activité introuvable')),
          );
        }

        final isRegistered = registeredIds.contains(activity.id);
        final isSport = activity.category == ActivityCategory.sport;
        final categoryColor =
            isSport ? AppColors.sportBadge : AppColors.culturalBadge;
        final spotsLeft =
            activity.maxParticipants - activity.currentParticipants;
        final isFull = spotsLeft <= 0;
        final deadlinePassed = activity.registrationDeadline != null &&
            activity.registrationDeadline!.isBefore(DateTime.now());

        return Scaffold(
      backgroundColor: AppColors.background,
      body: CustomScrollView(
        slivers: [
          SliverAppBar(
            expandedHeight: 280,
            pinned: true,
            backgroundColor: AppColors.surface,
            elevation: 0,
            leading: Padding(
              padding: const EdgeInsets.all(8),
              child: CircleAvatar(
                backgroundColor: Colors.black.withValues(alpha: 0.45),
                child: IconButton(
                  icon: const Icon(Iconsax.arrow_left,
                      color: Colors.white, size: 20),
                  onPressed: () => context.pop(),
                ),
              ),
            ),
            actions: [
              Padding(
                padding: const EdgeInsets.all(8),
                child: CircleAvatar(
                  backgroundColor: Colors.black.withValues(alpha: 0.45),
                  child: IconButton(
                    icon: const Icon(Iconsax.share,
                        color: Colors.white, size: 18),
                    onPressed: () => _shareActivity(activity),
                  ),
                ),
              ),
            ],
            flexibleSpace: FlexibleSpaceBar(
              background: CachedNetworkImage(
                imageUrl: activity.imageUrl,
                fit: BoxFit.cover,
                placeholder: (_, _) =>
                    Container(color: AppColors.divider),
                errorWidget: (_, _, _) =>
                    Container(color: AppColors.divider),
              ),
            ),
          ),
          SliverToBoxAdapter(
            child: Container(
              margin: const EdgeInsets.all(16),
              padding: const EdgeInsets.all(20),
              decoration: BoxDecoration(
                color: AppColors.surface,
                borderRadius: BorderRadius.circular(22),
                boxShadow: const [
                  BoxShadow(
                    color: AppColors.cardShadow,
                    blurRadius: 16,
                    offset: Offset(0, 4),
                  ),
                ],
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 10, vertical: 5),
                        decoration: BoxDecoration(
                          color: categoryColor.withValues(alpha: 0.12),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Text(
                          activity.categoryLabel,
                          style: TextStyle(
                            fontSize: 12,
                            fontWeight: FontWeight.w700,
                            color: categoryColor,
                          ),
                        ),
                      ),
                      const Spacer(),
                      Container(
                        padding: const EdgeInsets.symmetric(
                            horizontal: 10, vertical: 5),
                        decoration: BoxDecoration(
                          color: AppColors.heart.withValues(alpha: 0.1),
                          borderRadius: BorderRadius.circular(8),
                        ),
                        child: Row(
                          children: [
                            const Icon(Iconsax.heart_copy,
                                color: AppColors.heart, size: 14),
                            const SizedBox(width: 4),
                            Text(
                              '+${activity.hearts} cœurs',
                              style: const TextStyle(
                                fontSize: 12,
                                fontWeight: FontWeight.w700,
                                color: AppColors.heart,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 14),
                  Text(
                    activity.title,
                    style: const TextStyle(
                      fontSize: 24,
                      fontWeight: FontWeight.w700,
                      color: AppColors.textDark,
                      height: 1.2,
                      letterSpacing: -0.5,
                    ),
                  ),
                  const SizedBox(height: 20),
                  _InfoRow(
                    icon: Iconsax.calendar,
                    label: DateFormatter.dateFull(activity.date),
                  ),
                  const SizedBox(height: 10),
                  _InfoRow(
                    icon: Iconsax.clock,
                    label: DateFormatter.time(activity.date),
                  ),
                  const SizedBox(height: 10),
                  _InfoRow(
                    icon: Iconsax.location,
                    label: activity.location,
                  ),
                  const SizedBox(height: 10),
                  _InfoRow(
                    icon: Iconsax.profile_circle,
                    label: activity.organizer,
                  ),
                  if (activity.registrationDeadline != null) ...[
                    const SizedBox(height: 10),
                    _InfoRow(
                      icon: Iconsax.calendar_1,
                      label:
                          'Inscription avant le ${DateFormatter.dateFull(activity.registrationDeadline!)}',
                      color: deadlinePassed
                          ? AppColors.heart
                          : AppColors.secondary,
                    ),
                  ],
                  const SizedBox(height: 20),
                  const Divider(color: AppColors.divider),
                  const SizedBox(height: 16),
                  const Text(
                    'Description',
                    style: TextStyle(
                      fontSize: 16,
                      fontWeight: FontWeight.w700,
                      color: AppColors.textDark,
                    ),
                  ),
                  const SizedBox(height: 10),
                  Text(
                    activity.description,
                    style: const TextStyle(
                      fontSize: 14,
                      color: AppColors.textMedium,
                      height: 1.6,
                    ),
                  ),
                  const SizedBox(height: 20),
                  const Divider(color: AppColors.divider),
                  const SizedBox(height: 16),
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      const Text(
                        'Places',
                        style: TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.w700,
                          color: AppColors.textDark,
                        ),
                      ),
                      Text(
                        '${activity.currentParticipants}/${activity.maxParticipants}',
                        style: TextStyle(
                          fontSize: 14,
                          color: isFull
                              ? AppColors.heart
                              : AppColors.textMedium,
                          fontWeight: FontWeight.w600,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 10),
                  ClipRRect(
                    borderRadius: BorderRadius.circular(6),
                    child: LinearProgressIndicator(
                      value: activity.currentParticipants /
                          activity.maxParticipants,
                      backgroundColor: AppColors.divider,
                      valueColor:
                          AlwaysStoppedAnimation<Color>(categoryColor),
                      minHeight: 8,
                    ),
                  ),
                  const SizedBox(height: 6),
                  Text(
                    isFull
                        ? 'Complet — liste d\'attente disponible'
                        : '$spotsLeft place${spotsLeft > 1 ? 's' : ''} restante${spotsLeft > 1 ? 's' : ''}',
                    style: TextStyle(
                      fontSize: 12,
                      color: isFull
                          ? AppColors.heart
                          : AppColors.textLight,
                    ),
                  ),
                  const SizedBox(height: 80),
                ],
              ),
            ),
          ),
        ],
      ),
      bottomNavigationBar: Container(
        padding: const EdgeInsets.fromLTRB(20, 12, 20, 28),
        decoration: const BoxDecoration(
          color: AppColors.surface,
          boxShadow: [
            BoxShadow(
              color: Color(0x1A000000),
              blurRadius: 16,
              offset: Offset(0, -4),
            ),
          ],
        ),
        child: SizedBox(
          height: 56,
          child: ElevatedButton(
            onPressed: (isRegistered || isFull || deadlinePassed)
                ? null
                : () => _openRegistration(context, activity),
            style: ElevatedButton.styleFrom(
              backgroundColor: isRegistered
                  ? AppColors.divider
                  : isFull || deadlinePassed
                      ? AppColors.divider
                      : AppColors.primary,
              foregroundColor: isRegistered || isFull || deadlinePassed
                  ? AppColors.textMedium
                  : Colors.white,
              elevation: 0,
              shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(14)),
            ),
            child: Text(
              isRegistered
                  ? '✓ Déjà inscrit(e)'
                  : isFull
                      ? 'Complet'
                      : deadlinePassed
                          ? 'Inscriptions fermées'
                          : 'S\'inscrire',
              style: const TextStyle(
                  fontSize: 16, fontWeight: FontWeight.w600),
            ),
          ),
        ),
      ),
    );
      },
    );
  }

  void _openRegistration(BuildContext context, Activity activity) {
    if (activity.registrationUrl != null) {
      context.push(
          '/activity/${activity.id}/register?url=${Uri.encodeComponent(activity.registrationUrl!)}');
    } else {
      context.push('/activity/${activity.id}/confirmation');
    }
  }

  void _shareActivity(Activity activity) {
    SharePlus.instance.share(ShareParams(
      text: '🎉 ${activity.title}\n'
          '📅 ${DateFormatter.dateFull(activity.date)} à ${DateFormatter.time(activity.date)}\n'
          '📍 ${activity.location}\n\n'
          'Découvre cet événement sur EventHub UQTR !',
      subject: activity.title,
    ));
  }
}

class _InfoRow extends StatelessWidget {
  final IconData icon;
  final String label;
  final Color? color;

  const _InfoRow({required this.icon, required this.label, this.color});

  @override
  Widget build(BuildContext context) {
    final effectiveColor = color ?? AppColors.primary;
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 18, color: effectiveColor),
        const SizedBox(width: 10),
        Expanded(
          child: Text(
            label,
            style: TextStyle(
                fontSize: 14,
                color: color != null
                    ? effectiveColor
                    : AppColors.textMedium),
          ),
        ),
      ],
    );
  }
}
