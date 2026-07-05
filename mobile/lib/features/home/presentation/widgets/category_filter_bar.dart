import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../activities/domain/entities/activity.dart';
import '../../../activities/presentation/providers/activity_provider.dart';
import '../../../activities/presentation/widgets/category_chip.dart';

class CategoryFilterBar extends ConsumerWidget {
  const CategoryFilterBar({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final selected = ref.watch(activityFilterProvider).category;
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
            onTap: () =>
                notifier.setCategory(ActivityCategory.socioculturel),
          ),
        ],
      ),
    );
  }
}
