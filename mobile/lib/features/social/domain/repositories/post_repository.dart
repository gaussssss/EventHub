import '../entities/post.dart';

/// Contrat du dépôt de publications (symétrique à ActivityRepository).
abstract class PostRepository {
  Future<List<Post>> getAllPosts();
  Future<Post?> getPostById(String id);
}
