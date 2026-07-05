import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/date_formatter.dart';
import '../../../../core/widgets/async_value_widget.dart';
import '../../domain/entities/activity.dart';
import '../../domain/entities/activity_filter.dart';
import '../providers/activity_provider.dart';
import '../widgets/activity_card.dart';
import '../widgets/activity_filter_sheet.dart';
import '../widgets/category_chip.dart';

/// Texte de recherche libre (état éphémère propre à la page).
final catalogueSearchProvider = StateProvider.autoDispose<String>((ref) => '');

class CataloguePage extends ConsumerWidget {
  const CataloguePage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final activitiesAsync = ref.watch(filteredActivitiesProvider);
    final searchQuery = ref.watch(catalogueSearchProvider);
    final filter = ref.watch(activityFilterProvider);
    final registeredIds = ref.watch(registeredActivitiesProvider);

    List<Activity> applySearch(List<Activity> activities) =>
        searchQuery.isEmpty
            ? activities
            : activities
                .where((a) =>
                    a.title
                        .toLowerCase()
                        .contains(searchQuery.toLowerCase()) ||
                    a.location
                        .toLowerCase()
                        .contains(searchQuery.toLowerCase()))
                .toList();

    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        bottom: false,
        child: Column(
          children: [
            _Header(filter: filter),
            const SizedBox(height: 4),
            _SearchField(query: searchQuery),
            const SizedBox(height: 14),
            _CategoryRow(selected: filter.category),
            if (filter.availableOnly || filter.dateRange != null) ...[
              const SizedBox(height: 10),
              _ActiveFilterRow(filter: filter),
            ],
            const SizedBox(height: 14),
            Expanded(
              child: AsyncValueWidget<List<Activity>>(
                value: activitiesAsync,
                onRetry: () => ref.invalidate(allActivitiesProvider),
                data: (all) {
                  final displayed = applySearch(all);
                  return Column(
                    children: [
                      Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 20),
                        child: Row(
                          children: [
                            Text(
                              '${displayed.length} activité${displayed.length > 1 ? 's' : ''}',
                              style: const TextStyle(
                                fontSize: 13,
                                color: AppColors.textMedium,
                                fontWeight: FontWeight.w600,
                              ),
                            ),
                            const Spacer(),
                          ],
                        ),
                      ),
                      const SizedBox(height: 8),
                      Expanded(
                        child: displayed.isEmpty
                            ? const _EmptyState()
                            : ListView.separated(
                                padding: const EdgeInsets.fromLTRB(
                                    16, 4, 16, 110),
                                itemCount: displayed.length,
                                separatorBuilder: (_, _) =>
                                    const SizedBox(height: 12),
                                itemBuilder: (context, index) {
                                  final activity = displayed[index];
                                  return ActivityCard(
                                    activity: activity,
                                    isRegistered:
                                        registeredIds.contains(activity.id),
                                    onTap: () => context
                                        .push('/activity/${activity.id}'),
                                  );
                                },
                              ),
                      ),
                    ],
                  );
                },
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _Header extends StatelessWidget {
  final ActivityFilter filter;
  const _Header({required this.filter});

  @override
  Widget build(BuildContext context) {
    final count = filter.activeCount;
    return Padding(
      padding: const EdgeInsets.fromLTRB(20, 8, 16, 8),
      child: Row(
        children: [
          const Text(
            'Catalogue',
            style: TextStyle(
              fontSize: 30,
              fontWeight: FontWeight.w800,
              color: AppColors.textDark,
              letterSpacing: -0.6,
            ),
          ),
          const Spacer(),
          GestureDetector(
            onTap: () => ActivityFilterSheet.show(context),
            child: Stack(
              clipBehavior: Clip.none,
              children: [
                Container(
                  padding: const EdgeInsets.all(11),
                  decoration: BoxDecoration(
                    color: AppColors.surface,
                    borderRadius: BorderRadius.circular(14),
                    boxShadow: const [
                      BoxShadow(
                        color: AppColors.cardShadow,
                        blurRadius: 8,
                        offset: Offset(0, 2),
                      ),
                    ],
                  ),
                  child: const Icon(Iconsax.filter,
                      color: AppColors.textDark, size: 20),
                ),
                if (count > 0)
                  Positioned(
                    top: -4,
                    right: -4,
                    child: Container(
                      padding: const EdgeInsets.all(4),
                      constraints:
                          const BoxConstraints(minWidth: 18, minHeight: 18),
                      decoration: BoxDecoration(
                        color: AppColors.primary,
                        shape: BoxShape.circle,
                        border:
                            Border.all(color: AppColors.background, width: 2),
                      ),
                      child: Text(
                        '$count',
                        textAlign: TextAlign.center,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 10,
                          fontWeight: FontWeight.w700,
                          height: 1,
                        ),
                      ),
                    ),
                  ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _SearchField extends ConsumerWidget {
  final String query;
  const _SearchField({required this.query});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 20),
      child: Container(
        decoration: BoxDecoration(
          color: AppColors.surface,
          borderRadius: BorderRadius.circular(16),
          boxShadow: const [
            BoxShadow(
              color: AppColors.cardShadow,
              blurRadius: 8,
              offset: Offset(0, 2),
            ),
          ],
        ),
        child: TextField(
          onChanged: (v) =>
              ref.read(catalogueSearchProvider.notifier).state = v,
          decoration: InputDecoration(
            hintText: 'Rechercher une activité, un lieu...',
            hintStyle: const TextStyle(color: AppColors.textLight),
            prefixIcon: const Icon(Iconsax.search_normal,
                color: AppColors.textLight, size: 20),
            suffixIcon: query.isEmpty
                ? null
                : IconButton(
                    icon: const Icon(Iconsax.close_circle,
                        color: AppColors.textLight, size: 18),
                    onPressed: () =>
                        ref.read(catalogueSearchProvider.notifier).state = '',
                  ),
            border: InputBorder.none,
            contentPadding:
                const EdgeInsets.symmetric(horizontal: 16, vertical: 14),
          ),
        ),
      ),
    );
  }
}

class _CategoryRow extends ConsumerWidget {
  final ActivityCategory? selected;
  const _CategoryRow({required this.selected});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final notifier = ref.read(activityFilterProvider.notifier);
    return SizedBox(
      height: 40,
      child: ListView(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 20),
        children: [
          CategoryChip(
            label: 'Tout',
            isSelected: selected == null,
            onTap: () => notifier.setCategory(null),
          ),
          const SizedBox(width: 8),
          CategoryChip(
            label: 'Sport',
            icon: Iconsax.activity,
            isSelected: selected == ActivityCategory.sport,
            onTap: () => notifier.setCategory(ActivityCategory.sport),
          ),
          const SizedBox(width: 8),
          CategoryChip(
            label: 'Socioculturel',
            icon: Iconsax.music,
            isSelected: selected == ActivityCategory.socioculturel,
            onTap: () => notifier.setCategory(ActivityCategory.socioculturel),
          ),
        ],
      ),
    );
  }
}

/// Rappel visuel des filtres avancés actifs, avec suppression au tap.
class _ActiveFilterRow extends ConsumerWidget {
  final ActivityFilter filter;
  const _ActiveFilterRow({required this.filter});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final notifier = ref.read(activityFilterProvider.notifier);
    final range = filter.dateRange;
    return SizedBox(
      height: 34,
      child: ListView(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 20),
        children: [
          if (filter.availableOnly)
            _RemovableChip(
              label: 'Places dispos',
              onRemove: () => notifier.setAvailableOnly(false),
            ),
          if (range != null)
            _RemovableChip(
              label:
                  '${DateFormatter.date(range.start)} — ${DateFormatter.date(range.end)}',
              onRemove: () => notifier.setDateRange(null),
            ),
        ],
      ),
    );
  }
}

class _RemovableChip extends StatelessWidget {
  final String label;
  final VoidCallback onRemove;
  const _RemovableChip({required this.label, required this.onRemove});

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(right: 8),
      padding: const EdgeInsets.only(left: 12, right: 6),
      decoration: BoxDecoration(
        color: AppColors.primary.withValues(alpha: 0.1),
        borderRadius: BorderRadius.circular(18),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(
            label,
            style: const TextStyle(
              fontSize: 13,
              fontWeight: FontWeight.w600,
              color: AppColors.primary,
            ),
          ),
          const SizedBox(width: 2),
          IconButton(
            visualDensity: VisualDensity.compact,
            constraints: const BoxConstraints(minWidth: 28, minHeight: 28),
            padding: EdgeInsets.zero,
            icon: const Icon(Iconsax.close_circle,
                size: 16, color: AppColors.primary),
            onPressed: onRemove,
          ),
        ],
      ),
    );
  }
}

class _EmptyState extends StatelessWidget {
  const _EmptyState();

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Container(
            padding: const EdgeInsets.all(20),
            decoration: BoxDecoration(
              color: AppColors.surface,
              shape: BoxShape.circle,
            ),
            child: const Icon(Iconsax.search_normal,
                size: 36, color: AppColors.textLight),
          ),
          const SizedBox(height: 16),
          const Text(
            'Aucune activité trouvée',
            style: TextStyle(
              fontSize: 16,
              fontWeight: FontWeight.w600,
              color: AppColors.textDark,
            ),
          ),
          const SizedBox(height: 6),
          const Text(
            'Essayez d\'ajuster votre recherche ou vos filtres.',
            style: TextStyle(fontSize: 13, color: AppColors.textLight),
          ),
        ],
      ),
    );
  }
}
