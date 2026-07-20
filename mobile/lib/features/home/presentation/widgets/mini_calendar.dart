import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/date_formatter.dart';
import '../../../activities/domain/entities/activity.dart';
import '../../../activities/presentation/providers/activity_provider.dart';

/// Marqueurs de jour du calendrier :
/// 🟢 inscrit à ≥ 1 événement (à venir/présent) · 🔵 événement(s) mais aucune
/// inscription · 🔴 inscrit à ≥ 1 événement mais absent (no-show / jamais pointé).
const _markerGreen = Color(0xFF1FA85A);
const _markerBlue = Color(0xFF3B82F6);
const _markerRed = Color(0xFFE5484D);

const _weekdayLabels = ['Dim', 'Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam'];
const _monthNames = [
  'Janvier', 'Février', 'Mars', 'Avril', 'Mai', 'Juin',
  'Juillet', 'Août', 'Septembre', 'Octobre', 'Novembre', 'Décembre',
];

class MiniCalendar extends ConsumerStatefulWidget {
  const MiniCalendar({super.key});

  @override
  ConsumerState<MiniCalendar> createState() => _MiniCalendarState();
}

class _MiniCalendarState extends ConsumerState<MiniCalendar> {
  late DateTime _visibleMonth; // 1er du mois affiché

  @override
  void initState() {
    super.initState();
    final now = DateTime.now();
    _visibleMonth = DateTime(now.year, now.month);
  }

  static bool _sameDay(DateTime a, DateTime b) =>
      a.year == b.year && a.month == b.month && a.day == b.day;

  /// Couleur de statut d'un événement (même sémantique que les marqueurs) :
  /// 🟢 inscrit à venir / présent · 🔴 inscrit mais manqué · 🔵 sans inscription.
  static Color _statusColor(Activity a, DateTime startToday) {
    if (a.myStatus == 'attended' ||
        a.myStatus == 'waitlisted' ||
        (a.myStatus == 'registered' && !a.date.isBefore(startToday))) {
      return _markerGreen;
    }
    if (a.myStatus == 'noshow' ||
        (a.myStatus == 'registered' && a.date.isBefore(startToday))) {
      return _markerRed;
    }
    return _markerBlue;
  }

  void _shiftMonth(int delta) => setState(() =>
      _visibleMonth = DateTime(_visibleMonth.year, _visibleMonth.month + delta));

  @override
  Widget build(BuildContext context) {
    final now = DateTime.now();
    final startToday = DateTime(now.year, now.month, now.day);

    final registered =
        ref.watch(myRegistrationsProvider).valueOrNull ?? const <Activity>[];
    final monthEvents =
        ref.watch(monthActivitiesProvider(_visibleMonth)).valueOrNull ??
            const <Activity>[];

    // La liste publique du mois ne porte pas `myStatus` : on substitue la
    // version « mes inscriptions » quand elle existe (même activité), pour que
    // marqueurs et pastilles reflètent le statut réel.
    final regById = {for (final a in registered) a.id: a};
    List<Activity> eventsOn(DateTime day) => monthEvents
        .where((a) => _sameDay(a.date, day))
        .map((a) => regById[a.id] ?? a)
        .toList();

    Color? markerFor(DateTime day) {
      final events = eventsOn(day);
      if (events.isEmpty) return null;
      final colors =
          events.map((a) => _statusColor(a, startToday)).toSet();
      if (colors.contains(_markerGreen)) return _markerGreen;
      if (colors.contains(_markerRed)) return _markerRed;
      return _markerBlue;
    }

    // Grille : 6 semaines à partir du dimanche précédant (ou égal au) 1er du mois.
    final firstOfMonth = _visibleMonth;
    final leadingBlanks = firstOfMonth.weekday % 7; // dimanche = 0
    final gridStart = firstOfMonth.subtract(Duration(days: leadingBlanks));

    return Container(
      margin: const EdgeInsets.symmetric(horizontal: 20),
      padding: const EdgeInsets.fromLTRB(12, 10, 12, 12),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(20),
        boxShadow: const [
          BoxShadow(
            color: AppColors.cardShadow,
            blurRadius: 10,
            offset: Offset(0, 3),
          ),
        ],
      ),
      child: Column(
        children: [
          // En-tête mois + navigation (chevrons symétriques, même style)
          Row(
            children: [
              _NavButton(
                icon: Icons.chevron_left_rounded,
                onTap: () => _shiftMonth(-1),
              ),
              Expanded(
                child: Text(
                  '${_monthNames[_visibleMonth.month - 1]} ${_visibleMonth.year}',
                  textAlign: TextAlign.center,
                  style: const TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w700,
                    color: AppColors.textDark,
                  ),
                ),
              ),
              _NavButton(
                icon: Icons.chevron_right_rounded,
                onTap: () => _shiftMonth(1),
              ),
            ],
          ),
          const SizedBox(height: 8),
          // Libellés des jours
          Row(
            children: _weekdayLabels
                .map((d) => Expanded(
                      child: Center(
                        child: Text(
                          d,
                          style: const TextStyle(
                            fontSize: 11,
                            fontWeight: FontWeight.w600,
                            color: AppColors.textLight,
                          ),
                        ),
                      ),
                    ))
                .toList(),
          ),
          const SizedBox(height: 4),
          // Grille 6×7
          ...List.generate(6, (week) {
            return Row(
              children: List.generate(7, (dow) {
                final day = gridStart.add(Duration(days: week * 7 + dow));
                final inMonth = day.month == _visibleMonth.month;
                final isToday = _sameDay(day, now);
                final marker = inMonth ? markerFor(day) : null;

                return Expanded(
                  child: GestureDetector(
                    behavior: HitTestBehavior.opaque,
                    onTap: inMonth
                        ? () => _showDaySheet(context, day, eventsOn(day))
                        : null,
                    child: Container(
                      height: 40,
                      margin: const EdgeInsets.all(2),
                      decoration: BoxDecoration(
                        color: isToday
                            ? AppColors.primary.withValues(alpha: 0.10)
                            : Colors.transparent,
                        borderRadius: BorderRadius.circular(10),
                        border: isToday
                            ? Border.all(
                                color: AppColors.primary.withValues(alpha: 0.5))
                            : null,
                      ),
                      child: Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Text(
                            '${day.day}',
                            style: TextStyle(
                              fontSize: 13,
                              fontWeight:
                                  isToday ? FontWeight.w800 : FontWeight.w500,
                              color: !inMonth
                                  ? AppColors.textLight.withValues(alpha: 0.4)
                                  : isToday
                                      ? AppColors.primary
                                      : AppColors.textDark,
                            ),
                          ),
                          const SizedBox(height: 3),
                          Container(
                            width: 6,
                            height: 6,
                            decoration: BoxDecoration(
                              shape: BoxShape.circle,
                              color: marker ?? Colors.transparent,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ),
                );
              }),
            );
          }),
          const SizedBox(height: 8),
          // Légende
          const Wrap(
            alignment: WrapAlignment.center,
            spacing: 14,
            runSpacing: 4,
            children: [
              _LegendDot(color: _markerGreen, label: 'Inscrit'),
              _LegendDot(color: _markerBlue, label: 'Événement'),
              _LegendDot(color: _markerRed, label: 'Manqué'),
            ],
          ),
        ],
      ),
    );
  }

  void _showDaySheet(
      BuildContext context, DateTime day, List<Activity> events) {
    // `events` = TOUS les événements du jour (pas seulement les inscriptions).
    final now = DateTime.now();
    final startToday = DateTime(now.year, now.month, now.day);
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
                    ? 'Aucun événement ce jour'
                    : '${events.length} événement${events.length > 1 ? 's' : ''}',
                style: const TextStyle(fontSize: 13, color: AppColors.textLight),
              ),
              const SizedBox(height: 16),
              if (events.isEmpty)
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 16),
                  child: Row(
                    children: [
                      Icon(Iconsax.calendar_remove,
                          color: AppColors.textLight, size: 20),
                      SizedBox(width: 10),
                      Expanded(
                        child: Text(
                          'Aucun événement programmé ce jour-là.',
                          style: TextStyle(color: AppColors.textMedium),
                        ),
                      ),
                    ],
                  ),
                )
              else
                ...events.map(
                  (e) => _DayEventTile(
                    activity: e,
                    statusColor: _statusColor(e, startToday),
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

class _NavButton extends StatelessWidget {
  final IconData icon;
  final VoidCallback onTap;
  const _NavButton({required this.icon, required this.onTap});

  @override
  Widget build(BuildContext context) {
    return GestureDetector(
      onTap: onTap,
      behavior: HitTestBehavior.opaque,
      child: Container(
        width: 32,
        height: 32,
        margin: const EdgeInsets.all(2),
        decoration: BoxDecoration(
          color: AppColors.background,
          borderRadius: BorderRadius.circular(10),
          border: Border.all(color: AppColors.divider),
        ),
        child: Icon(icon, size: 20, color: AppColors.textMedium),
      ),
    );
  }
}

class _LegendDot extends StatelessWidget {
  final Color color;
  final String label;
  const _LegendDot({required this.color, required this.label});

  @override
  Widget build(BuildContext context) {
    return Row(
      mainAxisSize: MainAxisSize.min,
      children: [
        Container(
          width: 7,
          height: 7,
          decoration: BoxDecoration(shape: BoxShape.circle, color: color),
        ),
        const SizedBox(width: 5),
        Text(
          label,
          style: const TextStyle(fontSize: 11, color: AppColors.textMedium),
        ),
      ],
    );
  }
}

class _DayEventTile extends StatelessWidget {
  final Activity activity;

  /// Couleur du marqueur de cet événement (verte/rouge/bleue), cohérente avec
  /// la légende du calendrier.
  final Color statusColor;
  final VoidCallback onTap;

  const _DayEventTile({
    required this.activity,
    required this.statusColor,
    required this.onTap,
  });

  /// Badge de statut d'inscription (si l'utilisateur est inscrit à cet événement).
  (String, Color)? _statusBadge() {
    switch (activity.myStatus) {
      case 'attended':
        return ('Présent', _markerGreen);
      case 'noshow':
        return ('Absent', _markerRed);
      case 'registered':
        return ('Inscrit', AppColors.primary);
      case 'waitlisted':
        return ("Liste d'attente", _markerBlue);
      default:
        return null;
    }
  }

  @override
  Widget build(BuildContext context) {
    final badge = _statusBadge();
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
            // Pastille de statut : même code couleur que les marqueurs du
            // calendrier (vert inscrit, rouge manqué, bleu sans inscription).
            Container(
              width: 42,
              height: 42,
              decoration: BoxDecoration(
                color: statusColor.withValues(alpha: 0.12),
                borderRadius: BorderRadius.circular(10),
                border: Border.all(
                    color: statusColor.withValues(alpha: 0.5)),
              ),
              child: Icon(Iconsax.calendar_tick,
                  color: statusColor, size: 20),
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
                            fontSize: 14,
                            fontWeight: FontWeight.w700,
                            color: AppColors.textDark,
                          ),
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                      if (badge != null) ...[
                        const SizedBox(width: 8),
                        Container(
                          padding: const EdgeInsets.symmetric(
                              horizontal: 8, vertical: 3),
                          decoration: BoxDecoration(
                            color: badge.$2.withValues(alpha: 0.12),
                            borderRadius: BorderRadius.circular(8),
                          ),
                          child: Text(
                            badge.$1,
                            style: TextStyle(
                              fontSize: 11,
                              fontWeight: FontWeight.w700,
                              color: badge.$2,
                            ),
                          ),
                        ),
                      ],
                    ],
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
