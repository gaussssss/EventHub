import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/network/network_providers.dart';
import '../../data/repositories/auth_repository_entra.dart';
import '../../data/repositories/auth_repository_mock.dart';
import '../../domain/entities/user.dart';
import '../../domain/repositories/auth_repository.dart';

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  final tokens = ref.watch(tokenStorageProvider);
  // Config Entra présente → login Microsoft réel ; sinon repli mock (démo).
  return AppConfig.useRealAuth
      ? AuthRepositoryEntra(tokens)
      : AuthRepositoryMock(tokens);
});

/// Contrôleur de session : `null` = déconnecté, `User` = connecté.
/// L'état initial restaure une éventuelle session persistée.
class AuthController extends AsyncNotifier<User?> {
  @override
  Future<User?> build() {
    return ref.read(authRepositoryProvider).restoreSession();
  }

  Future<void> signInWithMicrosoft() async {
    state = const AsyncValue.loading();
    state = await AsyncValue.guard(
      () => ref.read(authRepositoryProvider).signInWithMicrosoft(),
    );
  }

  Future<void> signOut() async {
    await ref.read(authRepositoryProvider).signOut();
    state = const AsyncValue.data(null);
  }
}

final authControllerProvider =
    AsyncNotifierProvider<AuthController, User?>(AuthController.new);

/// Vrai si une session est ouverte (pratique pour les redirections).
final isAuthenticatedProvider = Provider<bool>((ref) {
  return ref.watch(authControllerProvider).valueOrNull != null;
});
