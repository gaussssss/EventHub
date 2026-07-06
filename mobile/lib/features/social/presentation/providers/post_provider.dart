import 'package:flutter/foundation.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/network/network_providers.dart';
import '../../domain/entities/post.dart';
import '../../domain/repositories/post_repository.dart';
import '../../data/datasources/post_local_datasource.dart';
import '../../data/datasources/post_remote_datasource.dart';
import '../../data/datasources/report_remote_datasource.dart';
import '../../data/datasources/upload_remote_datasource.dart';
import '../../data/repositories/post_repository_impl.dart';

final postRemoteDataSourceProvider = Provider<PostRemoteDataSource>((ref) {
  return PostRemoteDataSource(ref.watch(apiClientProvider));
});

final uploadRemoteDataSourceProvider = Provider<UploadRemoteDataSource>((ref) {
  return UploadRemoteDataSource(ref.watch(apiClientProvider));
});

final reportRemoteDataSourceProvider = Provider<ReportRemoteDataSource>((ref) {
  return ReportRemoteDataSource(ref.watch(apiClientProvider));
});

final postRepositoryProvider = Provider<PostRepository>((ref) {
  return PostRepositoryImpl(
    local: PostLocalDataSource(),
    remote: ref.watch(postRemoteDataSourceProvider),
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

/// Surcouche optimiste des « j'aime » : `postId → état souhaité`. On part de
/// l'état serveur (`post.isLikedByMe`) et on stocke uniquement une bascule locale
/// non encore reflétée par le serveur, ce qui évite le double comptage (le
/// compteur = `likesCount` serveur ± le delta local). En **live**, la bascule
/// appelle l'API puis invalide le fil pour se réaligner sur la vérité serveur.
class LikeOverrideNotifier extends Notifier<Map<String, bool>> {
  @override
  Map<String, bool> build() => {};

  /// [serverLiked] = `post.isLikedByMe` renvoyé par l'API pour ce post.
  void toggle(String id, bool serverLiked) {
    final current = state[id] ?? serverLiked;
    final desired = !current;
    state = {...state, id: desired};
    if (!AppConfig.useMockData) _sync(id, desired);
  }

  Future<void> _sync(String id, bool liked) async {
    try {
      final ds = ref.read(postRemoteDataSourceProvider);
      liked ? await ds.like(id) : await ds.unlike(id);
      ref.invalidate(allPostsProvider); // réaligne likesCount + isLikedByMe
    } catch (e) {
      debugPrint('[Posts] échec like/unlike $id : $e');
    }
  }
}

final likeOverrideProvider =
    NotifierProvider<LikeOverrideNotifier, Map<String, bool>>(
        LikeOverrideNotifier.new);

/// État d'affichage d'un like (résout serveur + override) : `(liked, count)`.
({bool liked, int count}) likeDisplay(
    Map<String, bool> overrides, Post post) {
  final override = overrides[post.id];
  final liked = override ?? post.isLikedByMe;
  final delta = (override != null && override != post.isLikedByMe)
      ? (override ? 1 : -1)
      : 0;
  final count = (post.likesCount + delta).clamp(0, 1 << 31);
  return (liked: liked, count: count);
}
