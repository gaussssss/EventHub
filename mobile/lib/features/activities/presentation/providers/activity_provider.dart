import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/network/network_providers.dart';
import '../../../stats/presentation/providers/stats_provider.dart';
import '../../domain/entities/activity.dart';
import '../../domain/entities/activity_filter.dart';
import '../../domain/entities/category.dart';
import '../../domain/repositories/activity_repository.dart';
import '../../data/datasources/activity_local_datasource.dart';
import '../../data/datasources/activity_remote_datasource.dart';
import '../../data/datasources/category_remote_datasource.dart';
import '../../data/repositories/activity_repository_impl.dart';

// ---------------------------------------------------------------------------
// Sources de données / dépôt
// ---------------------------------------------------------------------------

final activityRemoteDataSourceProvider =
    Provider<ActivityRemoteDataSource>((ref) {
  return ActivityRemoteDataSource(ref.watch(apiClientProvider));
});

final activityRepositoryProvider = Provider<ActivityRepository>((ref) {
  return ActivityRepositoryImpl(
    local: ActivityLocalDataSource(),
    remote: ref.watch(activityRemoteDataSourceProvider),
  );
});

/// Source de vérité : la liste complète, chargée de façon asynchrone.
final allActivitiesProvider = FutureProvider<List<Activity>>((ref) {
  return ref.watch(activityRepositoryProvider).getAllActivities();
});

/// Détail d'une activité, dérivé de [allActivitiesProvider] (pas de second appel
/// en mode mock ; côté API on pourra brancher `GET /activities/:id`).
final activityByIdProvider =
    Provider.family<AsyncValue<Activity?>, String>((ref, id) {
  return ref.watch(allActivitiesProvider).whenData(
        (list) => list.where((a) => a.id == id).firstOrNull,
      );
});

/// Nombre d'inscrits (badge accueil), dérivé des stats communautaires
/// (`GET /api/stats/community`) avec repli sur une valeur d'affichage.
final totalAppUsersProvider = Provider<int>((ref) {
  return ref.watch(communityStatsProvider).valueOrNull?.totalRegisteredUsers ??
      1_243;
});

// ---------------------------------------------------------------------------
// État de filtrage (catégorie + disponibilité + intervalle de dates)
// ---------------------------------------------------------------------------

/// Notifier centralisant toute la logique de mutation des filtres.
/// Les widgets appellent des méthodes nommées plutôt que de muter l'état.
class ActivityFilterNotifier extends Notifier<ActivityFilter> {
  @override
  ActivityFilter build() => const ActivityFilter();

  void setCategory(String? slug) =>
      state = state.copyWith(categorySlug: slug);

  void toggleCategory(String slug) => state = state.copyWith(
        categorySlug: state.categorySlug == slug ? null : slug,
      );

  void setAvailableOnly(bool value) =>
      state = state.copyWith(availableOnly: value);

  void setDateRange(DateTimeRange? range) =>
      state = state.copyWith(dateRange: range);

  void setIncludePast(bool value) =>
      state = state.copyWith(includePast: value);

  void setRegisteredOnly(bool value) =>
      state = state.copyWith(registeredOnly: value);

  void reset() => state = const ActivityFilter();
}

final activityFilterProvider =
    NotifierProvider<ActivityFilterNotifier, ActivityFilter>(
        ActivityFilterNotifier.new);

/// Vrai dès qu'un filtre est actif (pour le badge du bouton filtre).
final hasActiveFiltersProvider =
    Provider<bool>((ref) => ref.watch(activityFilterProvider).isActive);

// ---------------------------------------------------------------------------
// Catégories (chips dynamiques)
// ---------------------------------------------------------------------------

final categoryRemoteDataSourceProvider =
    Provider<CategoryRemoteDataSource>((ref) {
  return CategoryRemoteDataSource(ref.watch(apiClientProvider));
});

/// Catégories pour les chips de filtre. Live → `GET /api/categories` ; mock →
/// les catégories du jeu de démonstration.
final categoriesProvider = FutureProvider<List<Category>>((ref) async {
  if (AppConfig.useMockData) {
    return const [
      Category(slug: 'sport', label: 'Sport'),
      Category(slug: 'socioculturel', label: 'Socioculturel'),
    ];
  }
  return ref.watch(categoryRemoteDataSourceProvider).getCategories();
});

/// Liste filtrée, poussée au **backend** (catégorie/disponibilité/dates). Par
/// défaut, on masque les activités déjà passées (`from = début du jour`) sauf si
/// le filtre « voir les anciennes » est actif.
final filteredActivitiesProvider = FutureProvider<List<Activity>>((ref) {
  final filter = ref.watch(activityFilterProvider);

  DateTime? from = filter.dateRange?.start;
  final DateTime? to = filter.dateRange?.end;
  if (from == null && !filter.includePast) {
    final now = DateTime.now();
    from = DateTime(now.year, now.month, now.day); // début du jour courant
  }

  return _loadFiltered(ref, filter, from, to);
});

/// Charge le catalogue filtré (backend) puis applique le filtre client
/// « inscrits seulement » si demandé (intersection avec mes inscriptions).
Future<List<Activity>> _loadFiltered(
    Ref ref, ActivityFilter filter, DateTime? from, DateTime? to) async {
  final list = await ref.watch(activityRepositoryProvider).getActivities(
        categorySlug: filter.categorySlug,
        availableOnly: filter.availableOnly,
        from: from,
        to: to,
      );
  if (!filter.registeredOnly) return list;
  final registered = await ref.watch(myRegistrationsProvider.future);
  final ids = registered.map((a) => a.id).toSet();
  return list.where((a) => ids.contains(a.id)).toList();
}

/// Tous les événements d'un mois donné (passés **inclus**), pour le calendrier
/// mensuel de l'accueil. La clé de famille est normalisée au 1er du mois.
final monthActivitiesProvider =
    FutureProvider.family<List<Activity>, DateTime>((ref, month) {
  final from = DateTime(month.year, month.month, 1);
  // Dernier instant du dernier jour du mois (jour 0 du mois suivant = dernier jour).
  final to = DateTime(month.year, month.month + 1, 0, 23, 59, 59);
  return ref
      .watch(activityRepositoryProvider)
      .getActivities(from: from, to: to);
});

/// Activités à venir (à partir d'aujourd'hui), **indépendantes du filtre** —
/// utilisées par la section « Activités à venir » de l'accueil, qui ne doit pas
/// refléter les filtres du catalogue.
final upcomingActivitiesProvider = FutureProvider<List<Activity>>((ref) {
  final now = DateTime.now();
  return ref
      .watch(activityRepositoryProvider)
      .getActivities(from: DateTime(now.year, now.month, now.day));
});

/// Activités « à la une » : **toutes** celles explicitement marquées
/// `IsFeatured` (le carrousel les fait défiler) ; si aucune n'est marquée, on
/// retombe sur les 3 prochaines activités à venir.
final featuredActivitiesProvider = FutureProvider<List<Activity>>((ref) async {
  final repo = ref.watch(activityRepositoryProvider);
  if (!AppConfig.useMockData) {
    final featured =
        await ref.watch(activityRemoteDataSourceProvider).getFeatured();
    if (featured.isNotEmpty) return featured;
  }
  final now = DateTime.now();
  final upcoming =
      await repo.getActivities(from: DateTime(now.year, now.month, now.day));
  return upcoming.take(3).toList();
});

// ---------------------------------------------------------------------------
// Inscriptions de l'utilisateur
// ---------------------------------------------------------------------------

/// Gère l'ensemble des activités auxquelles l'utilisateur est inscrit.
///
/// En **mock** : jeu d'inscriptions de démonstration, mutations purement locales.
/// En **live** : on part des vraies inscriptions serveur (`myRegistrationsProvider`)
/// et chaque register/unregister est répercuté sur l'API (`POST …/register|cancel`),
/// avec mise à jour optimiste immédiate.
class RegisteredActivitiesNotifier extends Notifier<Set<String>> {
  @override
  Set<String> build() {
    if (AppConfig.useMockData) return {'act002', 'act006'};

    // Fusionne les inscriptions serveur à chaque rafraîchissement, sans écraser
    // un ajout optimiste local encore non reflété côté serveur.
    ref.listen(myRegistrationsProvider, (_, next) {
      final ids = next.valueOrNull?.map((a) => a.id) ?? const <String>[];
      state = {...state, ...ids};
    });
    final initial = ref.read(myRegistrationsProvider).valueOrNull;
    return initial == null ? <String>{} : initial.map((a) => a.id).toSet();
  }

  bool isRegistered(String id) => state.contains(id);

  void register(String id) {
    state = {...state, id};
    if (!AppConfig.useMockData) _syncRegister(id);
  }

  void unregister(String id) {
    state = {...state}..remove(id);
    if (!AppConfig.useMockData) _syncCancel(id);
  }

  void toggle(String id) => isRegistered(id) ? unregister(id) : register(id);

  Future<void> _syncRegister(String id) async {
    try {
      await ref.read(activityRemoteDataSourceProvider).register(id);
    } catch (e) {
      debugPrint('[Registrations] échec register $id : $e');
    }
    // Réconcilie avec la vérité serveur (statut réel : inscrit / liste d'attente).
    ref.invalidate(myRegistrationsProvider);
  }

  Future<void> _syncCancel(String id) async {
    try {
      await ref.read(activityRemoteDataSourceProvider).cancel(id);
    } catch (e) {
      debugPrint('[Registrations] échec cancel $id : $e');
    }
    ref.invalidate(myRegistrationsProvider);
  }
}

final registeredActivitiesProvider =
    NotifierProvider<RegisteredActivitiesNotifier, Set<String>>(
        RegisteredActivitiesNotifier.new);

/// Activités auxquelles l'utilisateur courant est inscrit (« Mes activités »).
///
/// Sous **auth réelle Entra**, on lit le vrai serveur (`GET /api/me/registrations`
/// renvoie les activités complètes — aucun besoin de recouper avec le catalogue
/// local) ; sinon on dérive du set local d'inscriptions ∩ catalogue mock.
final myRegistrationsProvider = FutureProvider<List<Activity>>((ref) async {
  if (AppConfig.useRealAuth) {
    return ref.watch(activityRemoteDataSourceProvider).getMyRegistrations();
  }
  final all = await ref.watch(allActivitiesProvider.future);
  final registeredIds = ref.watch(registeredActivitiesProvider);
  return all.where((a) => registeredIds.contains(a.id)).toList();
});
