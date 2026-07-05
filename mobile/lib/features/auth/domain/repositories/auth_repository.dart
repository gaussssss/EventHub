import '../entities/user.dart';

/// Contrat d'authentification.
///
/// Implémentation actuelle : **mock** (renvoie un utilisateur fictif).
/// Cible : connexion **Microsoft Entra ID** (OAuth2/PKCE) → le backend
/// échange l'`idToken` Microsoft contre les JWT EventHub
/// (voir docs/BACKEND_MANIFEST.md §1).
abstract class AuthRepository {
  /// Lance le flux Microsoft et ouvre une session.
  Future<User> signInWithMicrosoft();

  /// Restaure la session si un jeton valide est stocké, sinon `null`.
  Future<User?> restoreSession();

  /// Ferme la session et efface les jetons.
  Future<void> signOut();
}
