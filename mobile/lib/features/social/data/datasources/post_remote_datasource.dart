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

  /// `POST /api/posts` → crée une publication, renvoie son id.
  Future<String> createPost({
    required String imageUrl,
    required String caption,
    String? activityId,
  }) async {
    final data = await _client.post('/api/posts', data: {
      'imageUrl': imageUrl,
      'caption': caption,
      'activityId': activityId,
    });
    return (data as Map<String, dynamic>)['id'] as String;
  }

  /// `POST /api/posts/{id}/like`.
  Future<void> like(String id) => _client.post('/api/posts/$id/like');

  /// `DELETE /api/posts/{id}/like`.
  Future<void> unlike(String id) => _client.delete('/api/posts/$id/like');

  /// `POST /api/posts/{id}/comments` → ajoute un commentaire.
  Future<void> addComment(String id, String text) =>
      _client.post('/api/posts/$id/comments', data: {'text': text});
}
