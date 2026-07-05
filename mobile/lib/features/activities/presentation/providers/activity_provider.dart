import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/network_providers.dart';
import '../../../stats/presentation/providers/stats_provider.dart';
import '../../domain/entities/activity.dart';
import '../../domain/entities/activity_filter.dart';
import '../../domain/repositories/activity_repository.dart';
import '../../data/datasources/activity_local_datasource.dart';
import '../../data/datasources/activity_remote_datasource.dart';
import '../../data/repositories/activity_repository_impl.dart';

// ---------------------------------------------------------------------------
// Sources de données / dépôt
// ---------------------------------------------------------------------------

final activityRepositoryProvider = Provider<ActivityRepository>((ref) {
  return ActivityRepositoryImpl(
    local: ActivityLocalDataSource(),
    remote: ActivityRemoteDataSource(ref.watch(apiClientProvider)),
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

  void setCategory(ActivityCategory? category) =>
      state = state.copyWith(category: category);

  void toggleCategory(ActivityCategory category) => state = state.copyWith(
        category: state.category == category ? null : category,
      );

  void setAvailableOnly(bool value) =>
      state = state.copyWith(availableOnly: value);

  void setDateRange(DateTimeRange? range) =>
      state = state.copyWith(dateRange: range);

  void reset() => state = const ActivityFilter();
}

final activityFilterProvider =
    NotifierProvider<ActivityFilterNotifier, ActivityFilter>(
        ActivityFilterNotifier.new);

/// Vrai dès qu'un filtre est actif (pour le badge du bouton filtre).
final hasActiveFiltersProvider =
    Provider<bool>((ref) => ref.watch(activityFilterProvider).isActive);

/// Liste filtrée (catégorie + disponibilité + dates), dérivée de la source
/// asynchrone. Le filtrage se fait en mémoire sur les données chargées.
final filteredActivitiesProvider =
    Provider<AsyncValue<List<Activity>>>((ref) {
  final filter = ref.watch(activityFilterProvider);
  return ref.watch(allActivitiesProvider).whenData((all) {
    var list = all;

    if (filter.category != null) {
      list = list.where((a) => a.category == filter.category).toList();
    }
    if (filter.availableOnly) {
      list = list
          .where((a) => a.currentParticipants < a.maxParticipants)
          .toList();
    }
    final range = filter.dateRange;
    if (range != null) {
      final start = DateUtils.dateOnly(range.start);
      final end = DateUtils.dateOnly(range.end);
      list = list.where((a) {
        final d = DateUtils.dateOnly(a.date);
        return !d.isBefore(start) && !d.isAfter(end);
      }).toList();
    }
    return list;
  });
});

// ---------------------------------------------------------------------------
// Inscriptions de l'utilisateur
// ---------------------------------------------------------------------------

/// Gère l'ensemble des activités auxquelles l'utilisateur est inscrit.
class RegisteredActivitiesNotifier extends Notifier<Set<String>> {
  @override
  Set<String> build() => {'act002', 'act006'};

  bool isRegistered(String id) => state.contains(id);

  void register(String id) => state = {...state, id};

  void unregister(String id) => state = {...state}..remove(id);

  void toggle(String id) =>
      isRegistered(id) ? unregister(id) : register(id);
}

final registeredActivitiesProvider =
    NotifierProvider<RegisteredActivitiesNotifier, Set<String>>(
        RegisteredActivitiesNotifier.new);
