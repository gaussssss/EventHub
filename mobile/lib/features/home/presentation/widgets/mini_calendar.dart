import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/date_formatter.dart';
import '../../../activities/domain/entities/activity.dart';
import '../../../activities/presentation/providers/activity_provider.dart';

class MiniCalendar extends ConsumerStatefulWidget {
  const MiniCalendar({super.key});

  @override
  ConsumerState<MiniCalendar> createState() => _MiniCalendarState();
}

class _MiniCalendarState extends ConsumerState<MiniCalendar> {
  DateTime _selected = DateTime.now();

  static bool _sameDay(DateTime a, DateTime b) =>
      a.year == b.year && a.month == b.month && a.day == b.day;

  @override
  Widget build(BuildContext context) {
    final today = DateTime.now();
    final registered =
        ref.watch(myRegistrationsProvider).valueOrNull ?? const <Activity>[];

    final days = List.generate(7, (i) {
      return today.add(Duration(days: i - today.weekday % 7));
    });

    List<Activity> eventsOn(DateTime day) =>
        registered.where((a) => _sameDay(a.date, day)).toList();

    return SizedBox(
      height: 84,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: const EdgeInsets.symmetric(horizontal: 20),
        itemCount: days.length,
        separatorBuilder: (_, _) => const SizedBox(width: 8),
        itemBuilder: (context, index) {
          final day = days[index];
          final isSelected = _sameDay(day, _selected);
          final isToday = _sameDay(day, today);
          final hasEvents = eventsOn(day).isNotEmpty;

          return GestureDetector(
            onTap: () {
              setState(() => _selected = day);
              _showDaySheet(context, day, eventsOn(day));
            },
            child: AnimatedContainer(
              duration: const Duration(milliseconds: 200),
              width: 46,
              decoration: BoxDecoration(
                color: isSelected ? AppColors.primary : AppColors.surface,
                borderRadius: BorderRadius.circular(14),
                border: Border.all(
                  color: isToday && !isSelected
                      ? AppColors.primary
                      : Colors.transparent,
                  width: 2,
                ),
                boxShadow: isSelected
                    ? [
                        BoxShadow(
                          color: AppColors.primary.withValues(alpha: 0.3),
                          blurRadius: 8,
                          offset: const Offset(0, 3),
                        )
                      ]
                    : null,
              ),
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text(
                    DateFormatter.dayAbbr(day),
                    style: TextStyle(
                      fontSize: 12,
                      color:
                          isSelected ? Colors.white70 : AppColors.textLight,
                    ),
                  ),
                  const SizedBox(height: 4),
                  Text(
                    '${day.day}',
                    style: TextStyle(
                      fontSize: 18,
                      fontWeight: FontWeight.bold,
                      color: isSelected ? Colors.white : AppColors.textDark,
                    ),
                  ),
                  const SizedBox(height: 4),
                  // Marqueur : jour où l'utilisateur est inscrit à un événement.
                  Container(
                    width: 6,
                    height: 6,
                    decoration: BoxDecoration(
                      shape: BoxShape.circle,
                      color: hasEvents
                          ? (isSelected ? Colors.white : AppColors.primary)
                          : Colors.transparent,
                    ),
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }

  void _showDaySheet(
      BuildContext context, DateTime day, List<Activity> events) {
    showModalBottomSheet<void>(
      context: context,
      backgroundColor: AppColors.surface,
      useRootNavigator: true,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (sheetContext) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(20, 16, 20, 24),
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
              const SizedBox(height: 16),
              Text(
                DateFormatter.dateFull(day),
                style: const TextStyle(
                  fontSize: 18,
                  fontWeight: FontWeight.w700,
                  color: AppColors.textDark,
                  letterSpacing: -0.3,
                ),
              ),
              const SizedBox(height: 4),
              Text(
                events.isEmpty
                    ? 'Aucune inscription ce jour'
                    : '${events.length} inscription${events.length > 1 ? 's' : ''}',
                style: const TextStyle(fontSize: 13, color: AppColors.textLight),
              ),
              const SizedBox(height: 16),
              if (events.isEmpty)
                Padding(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  child: Row(
                    children: [
                      const Icon(Iconsax.calendar_remove,
                          color: AppColors.textLight, size: 20),
                      const SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          'Vous n\'êtes inscrit à aucun événement ce jour-là.',
                          style: const TextStyle(color: AppColors.textMedium),
                        ),
                      ),
                    ],
                  ),
                )
              else
                ...events.map(
                  (e) => _DayEventTile(
                    activity: e,
                    onTap: () {
                      Navigator.of(sheetContext).pop();
                      context.push('/activity/${e.id}');
                    },
                  ),
                ),
            ],
          ),
        ),
      ),
    );
  }
}

class _DayEventTile extends StatelessWidget {
  final Activity activity;
  final VoidCallback onTap;

  const _DayEventTile({required this.activity, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      child: Container(
        margin: const EdgeInsets.only(bottom: 10),
        padding: const EdgeInsets.all(12),
        decoration: BoxDecoration(
          color: AppColors.background,
          borderRadius: BorderRadius.circular(14),
          border: Border.all(color: AppColors.divider),
        ),
        child: Row(
          children: [
            Container(
              width: 42,
              height: 42,
              decoration: BoxDecoration(
                color: AppColors.primary.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(10),
              ),
              child: const Icon(Iconsax.calendar_tick,
                  color: AppColors.primary, size: 20),
            ),
            const SizedBox(width: 12),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    activity.title,
                    style: const TextStyle(
                      fontSize: 14,
                      fontWeight: FontWeight.w700,
                      color: AppColors.textDark,
                    ),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: 3),
                  Row(
                    children: [
                      const Icon(Iconsax.clock,
                          size: 12, color: AppColors.textLight),
                      const SizedBox(width: 4),
                      Text(
                        DateFormatter.time(activity.date),
                        style: const TextStyle(
                            fontSize: 12, color: AppColors.textLight),
                      ),
                      const SizedBox(width: 10),
                      const Icon(Iconsax.location,
                          size: 12, color: AppColors.textLight),
                      const SizedBox(width: 4),
                      Expanded(
                        child: Text(
                          activity.location,
                          style: const TextStyle(
                              fontSize: 12, color: AppColors.textLight),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
            const Icon(Iconsax.arrow_right_3,
                size: 16, color: AppColors.textLight),
          ],
        ),
      ),
    );
  }
}
