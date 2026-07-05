import '../../../../core/error/failure.dart';
import '../../../../core/network/api_client.dart';
import '../../domain/entities/activity.dart';
import '../models/activity_model.dart';

/// Accès distant au catalogue d'activités (API REST EventHub).
class ActivityRemoteDataSource {
  final ApiClient _client;

  ActivityRemoteDataSource(this._client);

  /// `GET /api/activities` → catalogue publié (tableau d'activités).
  Future<List<Activity>> getActivities() async {
    final data = await _client.get('/api/activities');
    return (data as List)
        .map((json) => ActivityModel.fromJson(json as Map<String, dynamic>))
        .toList();
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
