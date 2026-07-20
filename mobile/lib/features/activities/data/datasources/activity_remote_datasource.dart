import '../../../../core/error/failure.dart';
import '../../../../core/network/api_client.dart';
import '../../domain/entities/activity.dart';
import '../models/activity_model.dart';

/// Accès distant au catalogue d'activités (API REST EventHub).
class ActivityRemoteDataSource {
  final ApiClient _client;

  ActivityRemoteDataSource(this._client);

  /// `GET /api/activities` → catalogue publié, filtré côté serveur.
  Future<List<Activity>> getActivities({
    String? categorySlug,
    bool availableOnly = false,
    DateTime? from,
    DateTime? to,
  }) async {
    final query = <String, dynamic>{};
    if (categorySlug != null) query['category'] = categorySlug;
    if (availableOnly) query['availableOnly'] = true;
    if (from != null) query['from'] = from.toUtc().toIso8601String();
    if (to != null) query['to'] = to.toUtc().toIso8601String();

    final data = await _client.get('/api/activities', query: query);
    return (data as List)
        .map((json) => ActivityModel.fromJson(json as Map<String, dynamic>))
        .toList();
  }

  /// `GET /api/activities/featured` → activités marquées « à la une ».
  Future<List<Activity>> getFeatured() async {
    final data = await _client.get('/api/activities/featured');
    return (data as List)
        .map((json) => ActivityModel.fromJson(json as Map<String, dynamic>))
        .toList();
  }

  /// `GET /api/me/registrations` → activités auxquelles l'utilisateur courant
  /// est inscrit (route protégée : `Bearer` joint automatiquement).
  Future<List<Activity>> getMyRegistrations() async {
    final data = await _client.get('/api/me/registrations');
    return (data as List)
        .map((json) => ActivityModel.fromJson(json as Map<String, dynamic>))
        .toList();
  }

  /// `POST /api/activities/{id}/check-in` — auto-émargement : envoie le jeton
  /// scanné dans le QR de l'événement ; le serveur confirme la présence et
  /// crédite les cœurs. Renvoie `{status, heartsAwarded, alreadyCheckedIn}`.
  Future<Map<String, dynamic>> checkIn(String activityId, String token) async {
    final data = await _client
        .post('/api/activities/$activityId/check-in', data: {'token': token});
    return data as Map<String, dynamic>;
  }

  /// `POST /api/activities/{id}/register` — déclare l'inscription au serveur.
  /// (Le webhook Google Form étant différé, c'est l'app qui la déclare, en
  /// transmettant l'`formResponseId` s'il est connu.)
  Future<void> register(String activityId, {String? formResponseId}) async {
    await _client.post('/api/activities/$activityId/register',
        data: {'formResponseId': formResponseId});
  }

  /// `POST /api/activities/{id}/cancel` — annule l'inscription.
  Future<void> cancel(String activityId) async {
    await _client.post('/api/activities/$activityId/cancel');
  }

  /// `GET /api/activities/{id}` → détail, ou `null` si 404.
  Future<Activity?> getActivityById(String id) async {
    try {
      final data = await _client.get('/api/activities/$id');
      return ActivityModel.fromJson(data as Map<String, dynamic>);
    } on Failure catch (failure) {
      if (failure.type == FailureType.notFound) return null;
      rethrow;
    }
  }
}
