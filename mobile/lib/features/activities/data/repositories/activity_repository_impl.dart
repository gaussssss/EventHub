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
  Future<List<Activity>> getAllActivities() async {
    if (AppConfig.useMockData) {
      await Future.delayed(AppConfig.mockLatency);
      return _local.activities;
    }
    return _remote.getActivities();
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
