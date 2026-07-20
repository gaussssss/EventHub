import 'package:flutter/material.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';

class HeartStatCard extends StatelessWidget {
  final int userHearts;
  final int uqtrHearts;
  final String level;
  final int previousThreshold;
  final int nextThreshold;
  final VoidCallback onTap;

  const HeartStatCard({
    super.key,
    required this.userHearts,
    required this.uqtrHearts,
    required this.level,
    required this.previousThreshold,
    required this.nextThreshold,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    final progress = (userHearts - previousThreshold) /
        (nextThreshold - previousThreshold);

    return GestureDetector(
      onTap: onTap,
      child: Container(
        padding: const EdgeInsets.all(20),
        decoration: BoxDecoration(
          gradient: const LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [Color(0xFF006534), Color(0xFF1A7A4A)],
          ),
          borderRadius: BorderRadius.circular(20),
          boxShadow: [
            BoxShadow(
              color: AppColors.primary.withValues(alpha: 0.3),
              blurRadius: 16,
              offset: const Offset(0, 6),
            ),
          ],
        ),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              mainAxisAlignment: MainAxisAlignment.spaceBetween,
              children: [
                const Text(
                  'Mes cœurs santé',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 16,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(
                      horizontal: 10, vertical: 4),
                  decoration: BoxDecoration(
                    color: Colors.white.withValues(alpha: 0.2),
                    borderRadius: BorderRadius.circular(12),
                  ),
                  child: Row(
                    children: [
                      Icon(
                        _levelIcon(level),
                        color: Colors.white,
                        size: 14,
                      ),
                      const SizedBox(width: 5),
                      Text(
                        level,
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 13,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            Row(
              crossAxisAlignment: CrossAxisAlignment.end,
              children: [
                const Icon(Iconsax.heart_copy, color: Colors.white, size: 34),
                const SizedBox(width: 8),
                Text(
                  '$userHearts',
                  style: const TextStyle(
                    color: Colors.white,
                    fontSize: 42,
                    fontWeight: FontWeight.bold,
                    height: 1,
                  ),
                ),
                const SizedBox(width: 4),
                const Padding(
                  padding: EdgeInsets.only(bottom: 6),
                  child: Text(
                    'cœurs',
                    style: TextStyle(
                        color: Colors.white70, fontSize: 16),
                  ),
                ),
              ],
            ),
            const SizedBox(height: 16),
            ClipRRect(
              borderRadius: BorderRadius.circular(4),
              child: LinearProgressIndicator(
                value: progress.clamp(0.0, 1.0),
                backgroundColor: Colors.white.withValues(alpha: 0.25),
                valueColor: const AlwaysStoppedAnimation<Color>(
                    Colors.white),
                minHeight: 8,
              ),
            ),
            const SizedBox(height: 8),
            Text(
              '$userHearts / $nextThreshold pour niveau suivant',
              style: const TextStyle(color: Colors.white70, fontSize: 12),
            ),
            const SizedBox(height: 16),
            const Divider(
                color: Colors.white24, height: 1),
            const SizedBox(height: 12),
            Row(
              children: [
                const Icon(Iconsax.people,
                    color: Colors.white70, size: 16),
                const SizedBox(width: 6),
                Expanded(
                  child: Text(
                    'Total UQTR : $uqtrHearts cœurs',
                    style: const TextStyle(
                        color: Colors.white70, fontSize: 13),
                  ),
                ),
                // Indice d'affordance : signale que la carte est cliquable
                // (mène au détail des cœurs / classement).
                const Text(
                  'Détails',
                  style: TextStyle(
                    color: Colors.white,
                    fontSize: 13,
                    fontWeight: FontWeight.w600,
                  ),
                ),
                const Icon(Icons.chevron_right,
                    color: Colors.white, size: 20),
              ],
            ),
          ],
        ),
      ),
    );
  }

  IconData _levelIcon(String level) {
    switch (level) {
      case 'Or':
        return Iconsax.cup;
      case 'Argent':
        return Iconsax.medal_star;
      default:
        return Iconsax.award;
    }
  }
}
