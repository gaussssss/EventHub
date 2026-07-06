import 'dart:ui';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../constants/app_colors.dart';

class MainScaffold extends StatelessWidget {
  final StatefulNavigationShell shell;

  const MainScaffold({super.key, required this.shell});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      // Le contenu passe sous la barre flottante pour l'effet « verre dépoli ».
      extendBody: true,
      // Transition douce (fondu + léger glissement) au changement d'onglet,
      // plutôt qu'un remplacement sec. La clé = index de branche déclenche l'anim.
      body: AnimatedSwitcher(
        duration: const Duration(milliseconds: 300),
        switchInCurve: Curves.easeOutCubic,
        switchOutCurve: Curves.easeInCubic,
        transitionBuilder: (child, animation) => FadeTransition(
          opacity: animation,
          child: SlideTransition(
            position: Tween<Offset>(
              begin: const Offset(0, 0.015),
              end: Offset.zero,
            ).animate(animation),
            child: child,
          ),
        ),
        child: KeyedSubtree(
          key: ValueKey<int>(shell.currentIndex),
          child: shell,
        ),
      ),
      bottomNavigationBar: _FloatingNavBar(shell: shell),
    );
  }
}

class _FloatingNavBar extends StatelessWidget {
  final StatefulNavigationShell shell;
  const _FloatingNavBar({required this.shell});

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      top: false,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(20, 0, 20, 12),
        child: ClipRRect(
          borderRadius: BorderRadius.circular(26),
          child: BackdropFilter(
            filter: ImageFilter.blur(sigmaX: 18, sigmaY: 18),
            child: Container(
              height: 64,
              decoration: BoxDecoration(
                color: AppColors.surface.withValues(alpha: 0.82),
                borderRadius: BorderRadius.circular(26),
                border: Border.all(
                  color: Colors.black.withValues(alpha: 0.08),
                  width: 1,
                ),
                boxShadow: [
                  BoxShadow(
                    color: Colors.black.withValues(alpha: 0.10),
                    blurRadius: 24,
                    offset: const Offset(0, 8),
                  ),
                ],
              ),
              child: Row(
                children: [
                  _NavItem(
                    icon: Iconsax.home_2,
                    activeIcon: Iconsax.home_copy,
                    label: 'Accueil',
                    isActive: shell.currentIndex == 0,
                    onTap: () => shell.goBranch(0),
                  ),
                  _NavItem(
                    icon: Iconsax.category,
                    activeIcon: Iconsax.category_copy,
                    label: 'Catalogue',
                    isActive: shell.currentIndex == 1,
                    onTap: () => shell.goBranch(1),
                  ),
                  _NavItem(
                    icon: Iconsax.profile_circle,
                    activeIcon: Iconsax.profile_circle_copy,
                    label: 'Profil',
                    isActive: shell.currentIndex == 2,
                    onTap: () => shell.goBranch(2),
                  ),
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}

class _NavItem extends StatelessWidget {
  final IconData icon;
  final IconData activeIcon;
  final String label;
  final bool isActive;
  final VoidCallback onTap;

  const _NavItem({
    required this.icon,
    required this.activeIcon,
    required this.label,
    required this.isActive,
    required this.onTap,
  });

  @override
  Widget build(BuildContext context) {
    // L'onglet actif reçoit plus d'espace pour accueillir son libellé.
    return Expanded(
      flex: isActive ? 3 : 2,
      child: GestureDetector(
        onTap: onTap,
        behavior: HitTestBehavior.opaque,
        child: Center(
          child: AnimatedContainer(
            duration: const Duration(milliseconds: 220),
            curve: Curves.easeOut,
            padding: EdgeInsets.symmetric(
              horizontal: isActive ? 14 : 10,
              vertical: 8,
            ),
            decoration: BoxDecoration(
              color: isActive
                  ? AppColors.primary.withValues(alpha: 0.12)
                  : Colors.transparent,
              borderRadius: BorderRadius.circular(18),
            ),
            // FittedBox garantit que le contenu ne déborde jamais de son créneau.
            child: FittedBox(
              fit: BoxFit.scaleDown,
              child: Row(
                mainAxisSize: MainAxisSize.min,
                children: [
                  AnimatedScale(
                    scale: isActive ? 1.15 : 1.0,
                    duration: const Duration(milliseconds: 220),
                    curve: Curves.easeOutBack,
                    child: Icon(
                      isActive ? activeIcon : icon,
                      color:
                          isActive ? AppColors.primary : AppColors.textLight,
                      size: 24,
                    ),
                  ),
                  // Le libellé n'apparaît que sur l'onglet actif (style Apple Music).
                  if (isActive) ...[
                    const SizedBox(width: 8),
                    Text(
                      label,
                      style: const TextStyle(
                        fontSize: 13,
                        fontWeight: FontWeight.w700,
                        color: AppColors.primary,
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
    );
  }
}
