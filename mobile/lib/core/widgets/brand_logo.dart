import 'package:flutter/material.dart';

/// Logo « UQTR en santé » fourni par le client, adapté au thème **de l'appareil** :
/// wordmark couleur en clair, variante monochrome en sombre. On lit
/// `platformBrightness` (et non le thème de l'app, aujourd'hui figé en clair) afin
/// que la bascule fonctionne dès qu'un vrai thème sombre sera ajouté.
class BrandLogo extends StatelessWidget {
  final double height;

  const BrandLogo({super.key, this.height = 96});

  @override
  Widget build(BuildContext context) {
    final isDark =
        MediaQuery.platformBrightnessOf(context) == Brightness.dark;
    return Image.asset(
      isDark ? 'assets/logo/logo_dark.png' : 'assets/logo/logo_light.png',
      height: height,
      fit: BoxFit.contain,
      // Sécurité d'affichage si un asset venait à manquer.
      errorBuilder: (_, _, _) => const SizedBox.shrink(),
    );
  }
}
