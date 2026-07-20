import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/date_formatter.dart';
import '../providers/activity_provider.dart';
import 'category_chip.dart';

/// Feuille de filtres partagée entre l'accueil et le catalogue.
///
/// [onApplied] est appelé après "Appliquer" (ex: rediriger vers le catalogue).
class ActivityFilterSheet extends ConsumerStatefulWidget {
  final VoidCallback? onApplied;

  const ActivityFilterSheet({super.key, this.onApplied});

  static Future<void> show(
    BuildContext context, {
    VoidCallback? onApplied,
  }) {
    return showModalBottomSheet(
      context: context,
      backgroundColor: Colors.transparent,
      isScrollControlled: true,
      // Couvre toute l'écran, y compris la barre de navigation flottante.
      useRootNavigator: true,
      builder: (_) => ActivityFilterSheet(onApplied: onApplied),
    );
  }

  @override
  ConsumerState<ActivityFilterSheet> createState() =>
      _ActivityFilterSheetState();
}

class _ActivityFilterSheetState extends ConsumerState<ActivityFilterSheet> {
  @override
  Widget build(BuildContext context) {
    final filter = ref.watch(activityFilterProvider);
    final notifier = ref.read(activityFilterProvider.notifier);
    final selected = filter.categorySlug;
    final availableOnly = filter.availableOnly;
    final range = filter.dateRange;
    final categories = ref.watch(categoriesProvider).valueOrNull ?? const [];

    return Container(
      decoration: const BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      padding: EdgeInsets.fromLTRB(
          24, 20, 24, MediaQuery.of(context).viewInsets.bottom + 32),
      child: Column(
        mainAxisSize: MainAxisSize.min,
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Center(
            child: Container(
              width: 36,
              height: 4,
              decoration: BoxDecoration(
                color: AppColors.divider,
                borderRadius: BorderRadius.circular(2),
              ),
            ),
          ),
          const SizedBox(height: 20),
          const Text(
            'Filtrer les activités',
            style: TextStyle(
              fontSize: 20,
              fontWeight: FontWeight.w700,
              color: AppColors.textDark,
              letterSpacing: -0.4,
            ),
          ),
          const SizedBox(height: 20),
          const _SectionLabel('Catégorie'),
          const SizedBox(height: 10),
          Wrap(
            spacing: 8,
            runSpacing: 8,
            children: [
              CategoryChip(
                label: 'Tout',
                restingColor: AppColors.background,
                isSelected: selected == null,
                onTap: () => notifier.setCategory(null),
              ),
              for (final c in categories)
                CategoryChip(
                  label: c.label,
                  restingColor: AppColors.background,
                  isSelected: selected == c.slug,
                  onTap: () => notifier.setCategory(c.slug),
                ),
            ],
          ),
          const SizedBox(height: 22),
          const _SectionLabel('Intervalle de dates'),
          const SizedBox(height: 10),
          GestureDetector(
            onTap: _pickDateRange,
            child: Container(
              padding:
                  const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
              decoration: BoxDecoration(
                color: AppColors.background,
                borderRadius: BorderRadius.circular(14),
                border: Border.all(
                  color: range != null
                      ? AppColors.primary
                      : AppColors.divider,
                ),
              ),
              child: Row(
                children: [
                  Icon(Iconsax.calendar_1,
                      size: 18,
                      color: range != null
                          ? AppColors.primary
                          : AppColors.textLight),
                  const SizedBox(width: 10),
                  Expanded(
                    child: Text(
                      range == null
                          ? 'Toutes les dates'
                          : '${DateFormatter.date(range.start)}, ${DateFormatter.date(range.end)}',
                      style: TextStyle(
                        fontSize: 15,
                        color: range != null
                            ? AppColors.textDark
                            : AppColors.textLight,
                        fontWeight: range != null
                            ? FontWeight.w600
                            : FontWeight.normal,
                      ),
                    ),
                  ),
                  if (range != null)
                    GestureDetector(
                      onTap: () => notifier.setDateRange(null),
                      child: const Icon(Iconsax.close_circle,
                          size: 18, color: AppColors.textLight),
                    )
                  else
                    const Icon(Iconsax.arrow_right_3,
                        size: 16, color: AppColors.textLight),
                ],
              ),
            ),
          ),
          const SizedBox(height: 22),
          const _SectionLabel('Options'),
          const SizedBox(height: 6),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Places disponibles seulement',
                style: TextStyle(fontSize: 15, color: AppColors.textDark),
              ),
              Switch.adaptive(
                value: availableOnly,
                onChanged: notifier.setAvailableOnly,
                activeThumbColor: Colors.white,
                activeTrackColor: AppColors.primary,
              ),
            ],
          ),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Voir les activités passées',
                style: TextStyle(fontSize: 15, color: AppColors.textDark),
              ),
              Switch.adaptive(
                value: filter.includePast,
                onChanged: notifier.setIncludePast,
                activeThumbColor: Colors.white,
                activeTrackColor: AppColors.primary,
              ),
            ],
          ),
          Row(
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              const Text(
                'Mes inscriptions seulement',
                style: TextStyle(fontSize: 15, color: AppColors.textDark),
              ),
              Switch.adaptive(
                value: filter.registeredOnly,
                onChanged: notifier.setRegisteredOnly,
                activeThumbColor: Colors.white,
                activeTrackColor: AppColors.primary,
              ),
            ],
          ),
          const SizedBox(height: 24),
          Row(
            children: [
              Expanded(
                child: OutlinedButton(
                  onPressed: notifier.reset,
                  style: OutlinedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    side: const BorderSide(color: AppColors.divider),
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14)),
                  ),
                  child: const Text('Réinitialiser',
                      style: TextStyle(color: AppColors.textMedium)),
                ),
              ),
              const SizedBox(width: 12),
              Expanded(
                child: ElevatedButton(
                  onPressed: () {
                    Navigator.pop(context);
                    widget.onApplied?.call();
                  },
                  style: ElevatedButton.styleFrom(
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    backgroundColor: AppColors.primary,
                    foregroundColor: Colors.white,
                    elevation: 0,
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14)),
                  ),
                  child: const Text('Appliquer',
                      style: TextStyle(fontWeight: FontWeight.w600)),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }

  Future<void> _pickDateRange() async {
    final now = DateTime.now();
    final current = ref.read(activityFilterProvider).dateRange;
    final picked = await showDateRangePicker(
      context: context,
      firstDate: DateTime(now.year - 1),
      lastDate: DateTime(now.year + 2),
      initialDateRange: current,
      helpText: 'Choisir un intervalle',
      saveText: 'OK',
      builder: (context, child) => Theme(
        data: Theme.of(context).copyWith(
          colorScheme: const ColorScheme.light(primary: AppColors.primary),
        ),
        child: child!,
      ),
    );
    if (picked != null) {
      ref.read(activityFilterProvider.notifier).setDateRange(picked);
    }
  }
}

class _SectionLabel extends StatelessWidget {
  final String text;
  const _SectionLabel(this.text);

  @override
  Widget build(BuildContext context) {
    return Text(
      text.toUpperCase(),
      style: const TextStyle(
        fontSize: 12,
        fontWeight: FontWeight.w700,
        color: AppColors.textLight,
        letterSpacing: 0.6,
      ),
    );
  }
}

