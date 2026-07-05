import '../../../../core/error/failure.dart';
import '../../../../core/network/api_client.dart';
import '../../domain/entities/post.dart';
import '../models/post_model.dart';

/// Accès distant au fil communautaire (API REST EventHub).
class PostRemoteDataSource {
  final ApiClient _client;

  PostRemoteDataSource(this._client);

  /// `GET /api/posts` → fil des publications publiées.
  Future<List<Post>> getPosts() async {
    final data = await _client.get('/api/posts');
    return (data as List)
        .map((json) => PostModel.fromJson(json as Map<String, dynamic>))
        .toList();
  }

  /// `GET /api/posts/{id}` → détail + commentaires, ou `null` si 404.
  Future<Post?> getPostById(String id) async {
    try {
      final data = await _client.get('/api/posts/$id');
      return PostModel.fromJson(data as Map<String, dynamic>);
    } on Failure catch (failure) {
      if (failure.type == FailureType.notFound) return null;
      rethrow;
    }
  }
}
