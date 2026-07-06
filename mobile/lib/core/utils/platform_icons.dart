import 'dart:io';
import 'package:flutter/material.dart';

/// Icônes adaptées aux conventions de la plateforme (iOS ↔ Android).
/// Centralise le choix pour rester cohérent partout dans l'app.
class PlatformIcons {
  const PlatformIcons._();

  /// Retour : chevron sur iOS, flèche pleine sur Android.
  static IconData get back =>
      Platform.isIOS ? Icons.arrow_back_ios_new : Icons.arrow_back;

  /// Partage : icône « box + flèche » sur iOS, nœud de partage sur Android.
  static IconData get share => Platform.isIOS ? Icons.ios_share : Icons.share;
}
