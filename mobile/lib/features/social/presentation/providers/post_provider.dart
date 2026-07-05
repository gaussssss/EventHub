import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/network/network_providers.dart';
import '../../domain/entities/post.dart';
import '../../domain/repositories/post_repository.dart';
import '../../data/datasources/post_local_datasource.dart';
import '../../data/datasources/post_remote_datasource.dart';
import '../../data/repositories/post_repository_impl.dart';

final postRepositoryProvider = Provider<PostRepository>((ref) {
  return PostRepositoryImpl(
    local: PostLocalDataSource(),
    remote: PostRemoteDataSource(ref.watch(apiClientProvider)),
  );
});

final allPostsProvider = FutureProvider<List<Post>>((ref) {
  return ref.watch(postRepositoryProvider).getAllPosts();
});

/// Détail d'un post, dérivé de [allPostsProvider].
final postByIdProvider =
    Provider.family<AsyncValue<Post?>, String>((ref, id) {
  return ref.watch(allPostsProvider).whenData(
        (list) => list.where((p) => p.id == id).firstOrNull,
      );
});

/// Gère les publications « aimées » par l'utilisateur.
class LikedPostsNotifier extends Notifier<Set<String>> {
  @override
  Set<String> build() => {};

  bool isLiked(String id) => state.contains(id);

  void toggle(String id) =>
      state = isLiked(id) ? ({...state}..remove(id)) : {...state, id};
}

final likedPostsProvider =
    NotifierProvider<LikedPostsNotifier, Set<String>>(LikedPostsNotifier.new);
