import '../../../../core/config/app_config.dart';
import '../../../../core/storage/token_storage.dart';
import '../../domain/entities/user.dart';
import '../../domain/repositories/auth_repository.dart';

/// Implémentation **mock** de [AuthRepository] tant que l'enregistrement
/// Microsoft Entra n'est pas prêt. La session est néanmoins persistée via
/// [TokenStorage], donc le comportement (rester connecté) est déjà réaliste.
///
/// Le jour J : remplacer par `AuthRepositoryEntra` qui lance MSAL/AppAuth,
/// récupère l'`idToken`, appelle `POST /auth/microsoft`, et stocke les JWT.
class AuthRepositoryMock implements AuthRepository {
  final TokenStorage _tokens;

  AuthRepositoryMock(this._tokens);

  static const _mockUser = User(
    id: 'user001',
    name: 'Alex Tremblay',
    email: 'alex.tremblay@uqtr.ca',
  );

  @override
  Future<User> signInWithMicrosoft() async {
    await Future.delayed(AppConfig.mockLatency);
    await _tokens.save(access: 'mock-access-token', refresh: 'mock-refresh');
    return _mockUser;
  }

  @override
  Future<User?> restoreSession() async {
    if (await _tokens.hasSession()) return _mockUser;
    return null;
  }

  @override
  Future<void> signOut() => _tokens.clear();
}
