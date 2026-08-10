import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:share_plus/share_plus.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/date_formatter.dart';
import '../../../../core/utils/platform_icons.dart';
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

    // La fiche recharge l'activité depuis l'API à chaque ouverture (provider
    // autoDispose). Pour éviter un flash de chargement, on affiche la version
    // déjà connue du catalogue pendant que la fraîche arrive.
    final cached = ref
        .watch(allActivitiesProvider)
        .valueOrNull
        ?.where((a) => a.id == activityId)
        .firstOrNull;
    final activity =
        activityAsync.valueOrNull ?? (activityAsync.isLoading ? cached : null);

    if (activity == null) {
      if (activityAsync.isLoading) {
        return const Scaffold(body: Center(child: AppLoader()));
      }
      if (activityAsync.hasError) {
        return Scaffold(
          body: AppErrorView(
            message: '${activityAsync.error}',
            onRetry: () => ref.invalidate(activityByIdProvider(activityId)),
          ),
        );
      }
      return const Scaffold(
        body: Center(child: Text('Activité introuvable')),
      );
    }

        final isRegistered = registeredIds.contains(activity.id);
        const categoryColor = AppColors.primary;
        final spotsLeft =
            activity.maxParticipants - activity.currentParticipants;
        final isFull = spotsLeft <= 0;
        final now = DateTime.now();
        final deadlinePassed = activity.registrationDeadline != null &&
            activity.registrationDeadline!.isBefore(now);

        // Statut de MON inscription (présent/absent…) : porté par « mes
        // inscriptions », pas par le catalogue.
        Activity? myReg;
        final myRegs =
            ref.watch(myRegistrationsProvider).valueOrNull ?? const <Activity>[];
        for (final a in myRegs) {
          if (a.id == activity.id) {
            myReg = a;
            break;
          }
        }
        final hasAttended = myReg?.myStatus == 'attended';
        // Fenêtre d'émargement (miroir serveur) : le bouton de scan n'apparaît
        // que pendant cette période.
        final checkInOpen = activity.isCheckInOpen(now);

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
                  icon: Icon(PlatformIcons.back,
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
                    icon: Icon(PlatformIcons.share,
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
                    icon: Iconsax.dollar_circle,
                    label: activity.participationCost <= 0
                        ? 'Participation gratuite'
                        : 'Coût de participation : ${activity.participationCost.toStringAsFixed(0)} \$',
                  ),
                  const SizedBox(height: 10),
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
                  // Présence déjà confirmée : remerciement.
                  if (isRegistered && hasAttended) ...[
                    const SizedBox(height: 16),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 14, vertical: 12),
                      decoration: BoxDecoration(
                        color: AppColors.primary.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: const Row(
                        children: [
                          Icon(Iconsax.tick_circle,
                              color: AppColors.primary, size: 18),
                          SizedBox(width: 10),
                          Expanded(
                            child: Text(
                              'Présence confirmée. Merci de votre participation ! 🎉',
                              style: TextStyle(
                                fontSize: 13,
                                color: AppColors.primary,
                                fontWeight: FontWeight.w600,
                                height: 1.3,
                              ),
                            ),
                          ),
                        ],
                      ),
                    ),
                  ]
                  // Inscrit, présence pas encore confirmée : période de
                  // confirmation + bouton de scan UNIQUEMENT pendant la fenêtre.
                  else if (isRegistered) ...[
                    const SizedBox(height: 16),
                    Container(
                      padding: const EdgeInsets.symmetric(
                          horizontal: 14, vertical: 12),
                      decoration: BoxDecoration(
                        color: AppColors.secondary.withValues(alpha: 0.1),
                        borderRadius: BorderRadius.circular(12),
                      ),
                      child: Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          const Icon(Iconsax.info_circle,
                              color: AppColors.secondary, size: 18),
                          const SizedBox(width: 10),
                          Expanded(
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                const Text(
                                  'Vos points seront attribués après confirmation de présence.',
                                  style: TextStyle(
                                    fontSize: 13,
                                    color: AppColors.secondary,
                                    fontWeight: FontWeight.w600,
                                    height: 1.3,
                                  ),
                                ),
                                const SizedBox(height: 4),
                                Text(
                                  'Confirmation sur place (scan du QR) : ${_checkInPeriodLabel(activity)}.',
                                  style: const TextStyle(
                                    fontSize: 12.5,
                                    color: AppColors.secondary,
                                    height: 1.3,
                                  ),
                                ),
                              ],
                            ),
                          ),
                        ],
                      ),
                    ),
                    if (checkInOpen) ...[
                      const SizedBox(height: 12),
                      // Visible uniquement pendant la période de confirmation
                      // (fenêtre re-validée côté serveur au scan).
                      SizedBox(
                        width: double.infinity,
                        height: 48,
                        child: OutlinedButton.icon(
                          onPressed: () => context.push('/scan'),
                          icon: const Icon(Iconsax.scan_barcode, size: 20),
                          label: const Text(
                            'Scanner ma présence',
                            style: TextStyle(
                                fontSize: 15, fontWeight: FontWeight.w600),
                          ),
                          style: OutlinedButton.styleFrom(
                            foregroundColor: AppColors.primary,
                            side: const BorderSide(color: AppColors.primary),
                            shape: RoundedRectangleBorder(
                                borderRadius: BorderRadius.circular(14)),
                          ),
                        ),
                      ),
                    ],
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
                        ? 'Complet, liste d\'attente disponible'
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
  }

  /// Libellé lisible de la période de confirmation de présence (heure locale) :
  /// « le 12 mars, de 16 h 00 à 22 h 00 », ou « du … au … » si elle chevauche
  /// deux jours.
  static String _checkInPeriodLabel(Activity activity) {
    final opens = activity.checkInOpensAt;
    final closes = activity.checkInClosesAt;
    final sameDay = opens.year == closes.year &&
        opens.month == closes.month &&
        opens.day == closes.day;
    if (sameDay) {
      return 'le ${DateFormatter.date(opens)}, de ${DateFormatter.time(opens)} '
          'à ${DateFormatter.time(closes)}';
    }
    return 'du ${DateFormatter.date(opens)} ${DateFormatter.time(opens)} '
        'au ${DateFormatter.date(closes)} ${DateFormatter.time(closes)}';
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
    // Partage texte + lien de l'événement. (On n'attache PAS d'image : cela
    // ferait basculer iOS en « partage de document » avec des actions Save
    // Image / Print. Un vrai aperçu riche viendra avec la page web + OpenGraph.)
    final link = '${AppConfig.shareBaseUrl}/activities/${activity.id}';
    SharePlus.instance.share(ShareParams(
      text: '🎉 ${activity.title}\n'
          '📅 ${DateFormatter.dateFull(activity.date)} à ${DateFormatter.time(activity.date)}\n'
          '📍 ${activity.location}\n\n'
          'Découvre cet événement sur UQTR en santé !\n$link',
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
