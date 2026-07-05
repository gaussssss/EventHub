import '../entities/activity.dart';

/// Contrat du dépôt d'activités.
///
/// Asynchrone : aujourd'hui satisfait par une source locale, demain par l'API
/// (`GET /activities`, `GET /activities/:id`) sans changer la présentation.
/// Le filtrage par catégorie/dates/disponibilité se fait côté présentation
/// (voir `filteredActivitiesProvider`).
abstract class ActivityRepository {
  Future<List<Activity>> getAllActivities();
  Future<Activity?> getActivityById(String id);
}
