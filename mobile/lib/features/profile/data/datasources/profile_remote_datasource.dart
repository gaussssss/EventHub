import '../../../../core/network/api_client.dart';
import '../../domain/entities/user_profile.dart';
import '../models/user_profile_model.dart';

/// Accès distant au profil de l'utilisateur courant (API REST EventHub).
/// Toutes ces routes sont protégées : l'`ApiClient` joint le `Bearer` Entra.
class ProfileRemoteDataSource {
  final ApiClient _client;

  ProfileRemoteDataSource(this._client);

  /// `GET /api/me` → profil de l'utilisateur authentifié.
  Future<UserProfile> getMyProfile() async {
    final data = await _client.get('/api/me');
    return UserProfileModel.fromJson(data as Map<String, dynamic>);
  }
}
