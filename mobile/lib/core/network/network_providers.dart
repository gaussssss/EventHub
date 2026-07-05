import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../storage/token_storage.dart';
import 'api_client.dart';

/// Stockage sécurisé des jetons (source unique pour toute l'app).
final tokenStorageProvider = Provider<TokenStorage>((ref) => TokenStorage());

/// Client HTTP partagé (injecte le Bearer, mappe les erreurs).
final apiClientProvider = Provider<ApiClient>((ref) {
  return ApiClient(ref.watch(tokenStorageProvider));
});
