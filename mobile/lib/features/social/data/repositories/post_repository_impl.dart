import '../../../../core/config/app_config.dart';
import '../../domain/entities/post.dart';
import '../../domain/repositories/post_repository.dart';
import '../datasources/post_local_datasource.dart';
import '../datasources/post_remote_datasource.dart';

/// Aiguille entre source locale (mock) et API distante selon
/// [AppConfig.useMockData].
class PostRepositoryImpl implements PostRepository {
  final PostLocalDataSource _local;
  final PostRemoteDataSource _remote;

  PostRepositoryImpl({
    required PostLocalDataSource local,
    required PostRemoteDataSource remote,
  })  : _local = local,
        _remote = remote;

  @override
  Future<List<Post>> getAllPosts() async {
    if (AppConfig.useMockData) {
      await Future.delayed(AppConfig.mockLatency);
      return _local.posts;
    }
    return _remote.getPosts();
  }

  @override
  Future<Post?> getPostById(String id) async {
    if (AppConfig.useMockData) {
      await Future.delayed(AppConfig.mockLatency);
      try {
        return _local.posts.firstWhere((p) => p.id == id);
      } catch (_) {
        return null;
      }
    }
    return _remote.getPostById(id);
  }
}
