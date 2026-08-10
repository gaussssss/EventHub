import 'package:flutter/material.dart';

/// Logo « UQTR en santé » fourni par le client. L'app s'affiche **uniquement en
/// thème clair** : on sert toujours la version light, sans bascule selon le
/// mode clair/sombre de l'appareil (choix produit).
class BrandLogo extends StatelessWidget {
  final double height;

  const BrandLogo({super.key, this.height = 96});

  @override
  Widget build(BuildContext context) {
    return Image.asset(
      'assets/logo/logo_light.png',
      height: height,
      fit: BoxFit.contain,
      // Sécurité d'affichage si l'asset venait à manquer.
      errorBuilder: (_, _, _) => const SizedBox.shrink(),
    );
  }
}
