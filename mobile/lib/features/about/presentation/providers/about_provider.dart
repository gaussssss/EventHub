import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/network/network_providers.dart';
import '../../../../core/utils/media_url.dart';
import '../../domain/entities/contributor.dart';

/// Contributeurs de la page « À propos » (`GET /api/about/contributors`),
/// gérés depuis le back office. Jeu statique en mode démo (mock).
final contributorsProvider = FutureProvider<List<Contributor>>((ref) async {
  if (AppConfig.useMockData) {
    return const [
      Contributor(name: 'Équipe UQTR en santé', role: 'Conception & développement'),
    ];
  }
  final data = await ref.watch(apiClientProvider).get('/api/about/contributors');
  return (data as List)
      .map((json) => Contributor(
            name: (json['name'] ?? '') as String,
            role: (json['role'] ?? '') as String,
            avatarUrl: (json['avatarUrl'] as String?)?.isEmpty ?? true
                ? null
                : resolveMediaUrl(json['avatarUrl'] as String?),
          ))
      .toList();
});
