import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/date_formatter.dart';
import '../../domain/entities/activity.dart';

class ActivityCard extends StatelessWidget {
  final Activity activity;
  final bool isRegistered;
  final VoidCallback onTap;

  const ActivityCard({
    super.key,
    required this.activity,
    required this.isRegistered,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final isSport = activity.category == ActivityCategory.sport;
    final categoryColor =
        isSport ? AppColors.sportBadge : AppColors.culturalBadge;
    final spotsLeft = activity.maxParticipants - activity.currentParticipants;
    final isFull = spotsLeft <= 0;
    final fillRatio =
        (activity.currentParticipants / activity.maxParticipants)
            .clamp(0.0, 1.0);

    return GestureDetector(
      onTap: onTap,
      child: Container(
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(20),
          boxShadow: const [
            BoxShadow(
              color: AppColors.cardShadow,
              blurRadius: 12,
              offset: Offset(0, 4),
            ),
          ],
        ),
        child: Padding(
          padding: const EdgeInsets.all(10),
          child: Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _Thumbnail(
                imageUrl: activity.imageUrl,
                categoryColor: categoryColor,
                categoryLabel: activity.categoryLabel,
              ),
              const SizedBox(width: 12),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            activity.title,
                            style: const TextStyle(
                              fontSize: 15,
                              fontWeight: FontWeight.w700,
                              color: AppColors.textDark,
                              letterSpacing: -0.2,
                            ),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                        if (isRegistered) const _RegisteredBadge(),
                      ],
                    ),
                    const SizedBox(height: 6),
                    _MetaRow(
                      icon: Iconsax.calendar,
                      text: DateFormatter.dateTime(activity.date),
                    ),
                    const SizedBox(height: 3),
                    _MetaRow(
                      icon: Iconsax.location,
                      text: activity.location,
                    ),
                    const SizedBox(height: 9),
                    Row(
                      children: [
                        _HeartPill(hearts: activity.hearts),
                        const SizedBox(width: 8),
                        Expanded(
                          child: _SpotsIndicator(
                            fillRatio: fillRatio,
                            spotsLeft: spotsLeft,
                            isFull: isFull,
                          ),
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _Thumbnail extends StatelessWidget {
  final String imageUrl;
  final Color categoryColor;
  final String categoryLabel;

  const _Thumbnail({
    required this.imageUrl,
    required this.categoryColor,
    required this.categoryLabel,
  });

  @override
  Widget build(BuildContext context) {
    return ClipRRect(
      borderRadius: BorderRadius.circular(14),
      child: SizedBox(
        width: 96,
        height: 96,
        child: Stack(
          fit: StackFit.expand,
          children: [
            CachedNetworkImage(
              imageUrl: imageUrl,
              fit: BoxFit.cover,
              placeholder: (_, _) => Container(color: AppColors.divider),
              errorWidget: (_, _, _) => const ColoredBox(
                color: AppColors.divider,
                child: Icon(Iconsax.gallery, color: AppColors.textLight),
              ),
            ),
            Positioned(
              top: 6,
              left: 6,
              child: Container(
                padding:
                    const EdgeInsets.symmetric(horizontal: 7, vertical: 3),
                decoration: BoxDecoration(
                  color: categoryColor,
                  borderRadius: BorderRadius.circular(7),
                ),
                child: Text(
                  categoryLabel,
                  style: const TextStyle(
                    fontSize: 9,
                    fontWeight: FontWeight.w700,
                    color: Colors.white,
                    letterSpacing: 0.2,
                  ),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _MetaRow extends StatelessWidget {
  final IconData icon;
  final String text;

  const _MetaRow({required this.icon, required this.text});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        Icon(icon, size: 13, color: AppColors.textLight),
        const SizedBox(width: 5),
        Expanded(
          child: Text(
            text,
            style: const TextStyle(fontSize: 12, color: AppColors.textLight),
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
          ),
        ),
      ],
    );
  }
}

class _HeartPill extends StatelessWidget {
  final int hearts;
  const _HeartPill({required this.hearts});

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
      decoration: BoxDecoration(
        color: AppColors.heart.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(8),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          const Icon(Iconsax.heart_copy, size: 12, color: AppColors.heart),
          const SizedBox(width: 3),
          Text(
            '+$hearts',
            style: const TextStyle(
              fontSize: 12,
              fontWeight: FontWeight.w700,
              color: AppColors.heart,
            ),
          ),
        ],
      ),
    );
  }
}

class _SpotsIndicator extends StatelessWidget {
  final double fillRatio;
  final int spotsLeft;
  final bool isFull;

  const _SpotsIndicator({
    required this.fillRatio,
    required this.spotsLeft,
    required this.isFull,
  });

  @override
  Widget build(BuildContext context) {
    if (isFull) {
      return const Align(
        alignment: Alignment.centerRight,
        child: Text(
          'Complet',
          style: TextStyle(
            fontSize: 11,
            fontWeight: FontWeight.w700,
            color: AppColors.heart,
          ),
        ),
      );
    }

    // Devient rouge quand il reste peu de places.
    final barColor = fillRatio > 0.85 ? AppColors.heart : AppColors.primary;

    return Column(
      crossAxisAlignment: CrossAxisAlignment.end,
      mainAxisSize: MainAxisSize.min,
      children: [
        Text(
          '$spotsLeft places restantes',
          style: const TextStyle(fontSize: 11, color: AppColors.textLight),
        ),
        const SizedBox(height: 4),
        ClipRRect(
          borderRadius: BorderRadius.circular(3),
          child: LinearProgressIndicator(
            value: fillRatio,
            minHeight: 4,
            backgroundColor: AppColors.divider,
            valueColor: AlwaysStoppedAnimation<Color>(barColor),
          ),
        ),
      ],
    );
  }
}

class _RegisteredBadge extends StatelessWidget {
  const _RegisteredBadge();

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(left: 6),
      padding: const EdgeInsets.symmetric(horizontal: 7, vertical: 3),
      decoration: BoxDecoration(
        color: AppColors.primary.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(7),
      ),
      child: const Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(Iconsax.tick_circle, size: 11, color: AppColors.primary),
          SizedBox(width: 3),
          Text(
            'Inscrit',
            style: TextStyle(
              fontSize: 10,
              fontWeight: FontWeight.w700,
              color: AppColors.primary,
            ),
          ),
        ],
      ),
    );
  }
}
