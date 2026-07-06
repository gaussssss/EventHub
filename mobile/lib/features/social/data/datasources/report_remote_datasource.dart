import '../../../../core/network/api_client.dart';

/// Signalement de contenu (`POST /api/reports`). Cible un post ou un commentaire.
class ReportRemoteDataSource {
  final ApiClient _client;

  ReportRemoteDataSource(this._client);

  /// `targetType` = `post` | `comment` ; `targetId` = Guid de la cible.
  Future<void> report({
    required String targetType,
    required String targetId,
    required String reason,
  }) =>
      _client.post('/api/reports', data: {
        'targetType': targetType,
        'targetId': targetId,
        'reason': reason,
      });
}
