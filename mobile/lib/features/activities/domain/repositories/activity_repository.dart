import '../entities/activity.dart';

/// Contrat du dépôt d'activités.
///
/// Le filtrage (catégorie/disponibilité/dates) est poussé au backend via
/// [getActivities] ; en mock, il est appliqué en mémoire sur la source locale.
abstract class ActivityRepository {
  /// Catalogue complet (sans filtre) — listes annexes (menu de publication,
  /// dérivation des inscriptions mock).
  Future<List<Activity>> getAllActivities();

  /// Catalogue filtré (`category`, `availableOnly`, `from`/`to`).
  Future<List<Activity>> getActivities({
    String? categorySlug,
    bool availableOnly,
    DateTime? from,
    DateTime? to,
  });

  Future<Activity?> getActivityById(String id);
}
