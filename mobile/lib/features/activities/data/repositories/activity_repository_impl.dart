import '../../../../core/config/app_config.dart';
import '../../domain/entities/activity.dart';
import '../../domain/repositories/activity_repository.dart';
import '../datasources/activity_local_datasource.dart';
import '../datasources/activity_remote_datasource.dart';

/// Aiguille entre source locale (mock) et API distante selon
/// [AppConfig.useMockData], sans changer le contrat vu par la présentation.
class ActivityRepositoryImpl implements ActivityRepository {
  final ActivityLocalDataSource _local;
  final ActivityRemoteDataSource _remote;

  ActivityRepositoryImpl({
    required ActivityLocalDataSource local,
    required ActivityRemoteDataSource remote,
  })  : _local = local,
        _remote = remote;

  @override
  Future<List<Activity>> getAllActivities() => getActivities();

  @override
  Future<List<Activity>> getActivities({
    String? categorySlug,
    bool availableOnly = false,
    DateTime? from,
    DateTime? to,
  }) async {
    if (AppConfig.useMockData) {
      await Future.delayed(AppConfig.mockLatency);
      var list = _local.activities;
      if (categorySlug != null) {
        list = list.where((a) => a.categorySlug == categorySlug).toList();
      }
      if (availableOnly) {
        list = list
            .where((a) => a.currentParticipants < a.maxParticipants)
            .toList();
      }
      if (from != null) {
        list = list.where((a) => !a.date.isBefore(from)).toList();
      }
      if (to != null) {
        list = list.where((a) => !a.date.isAfter(to)).toList();
      }
      return list;
    }
    return _remote.getActivities(
      categorySlug: categorySlug,
      availableOnly: availableOnly,
      from: from,
      to: to,
    );
  }

  @override
  Future<Activity?> getActivityById(String id) async {
    if (AppConfig.useMockData) {
      await Future.delayed(AppConfig.mockLatency);
      try {
        return _local.activities.firstWhere((a) => a.id == id);
      } catch (_) {
        return null;
      }
    }
    return _remote.getActivityById(id);
  }
}
