import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../features/auth/presentation/providers/auth_provider.dart';
import '../storage/token_storage.dart';
import 'api_client.dart';

/// Stockage sécurisé des jetons (source unique pour toute l'app).
final tokenStorageProvider = Provider<TokenStorage>((ref) => TokenStorage());

/// Client HTTP partagé (injecte le Bearer, mappe les erreurs). Sur `401`, il
/// tente d'abord un rafraîchissement silencieux du jeton et rejoue la requête ;
/// si le refresh échoue, il déclenche la déconnexion automatique.
final apiClientProvider = Provider<ApiClient>((ref) {
  return ApiClient(
    ref.watch(tokenStorageProvider),
    onRefreshToken: () =>
        ref.read(authRepositoryProvider).refreshAccessToken(),
    onUnauthorized: () =>
        ref.read(authControllerProvider.notifier).sessionExpired(),
  );
});
