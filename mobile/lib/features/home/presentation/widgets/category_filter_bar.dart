import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../activities/presentation/providers/activity_provider.dart';
import '../../../activities/presentation/widgets/category_chip.dart';

class CategoryFilterBar extends ConsumerWidget {
  const CategoryFilterBar({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final selected = ref.watch(activityFilterProvider).categorySlug;
    final notifier = ref.read(activityFilterProvider.notifier);
    final categories = ref.watch(categoriesProvider).valueOrNull ?? const [];

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
          for (final c in categories) ...[
            const SizedBox(width: 8),
            CategoryChip(
              label: c.label,
              isSelected: selected == c.slug,
              onTap: () => notifier.setCategory(c.slug),
            ),
          ],
        ],
      ),
    );
  }
}
