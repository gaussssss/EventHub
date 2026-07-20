import '../config/app_config.dart';

/// Résout une URL média renvoyée par l'API.
///
/// Les fichiers uploadés sont stockés en **chemin relatif** (`/uploads/…`, sans
/// nom de domaine) afin que la base reste portable si l'hôte change (dev → prod).
/// On les préfixe donc avec la base API configurée du client. Les URLs déjà
/// absolues (http/https — images externes seed, avatars pravatar, etc.) passent
/// telles quelles. Renvoie `''` si l'entrée est nulle/vide.
String resolveMediaUrl(String? url) {
  if (url == null || url.isEmpty) return '';
  final base = AppConfig.apiBaseUrl.replaceAll(RegExp(r'/+$'), '');

  // Fichier hébergé par l'API : on ré-ancre le chemin à partir de « /uploads/ »
  // sur la base API courante — répare aussi les anciennes URLs absolues dont
  // l'hôte est périmé (ex. IP LAN `http://192.168.x.x:5199` gravée en base).
  final uploadsIdx = url.indexOf('/uploads/');
  if (uploadsIdx >= 0) return '$base${url.substring(uploadsIdx)}';

  // URL déjà absolue et externe (picsum, unsplash, pravatar…) → inchangée.
  if (url.startsWith('http://') || url.startsWith('https://')) return url;

  // Chemin relatif quelconque → préfixé par la base API.
  return url.startsWith('/') ? '$base$url' : '$base/$url';
}
