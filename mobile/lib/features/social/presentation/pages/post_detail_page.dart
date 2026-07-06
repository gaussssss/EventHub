import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/error/failure.dart';
import '../../../../core/utils/date_formatter.dart';
import '../../../../core/widgets/async_value_widget.dart';
import '../../domain/entities/post.dart';
import '../providers/post_provider.dart';
import '../widgets/report_sheet.dart';

class PostDetailPage extends ConsumerStatefulWidget {
  final String postId;

  const PostDetailPage({super.key, required this.postId});

  @override
  ConsumerState<PostDetailPage> createState() => _PostDetailPageState();
}

class _PostDetailPageState extends ConsumerState<PostDetailPage> {
  final _commentController = TextEditingController();
  bool _sending = false;

  @override
  void dispose() {
    _commentController.dispose();
    super.dispose();
  }

  Future<void> _submitComment(String postId) async {
    final text = _commentController.text.trim();
    if (text.isEmpty || _sending) return;
    setState(() => _sending = true);
    try {
      if (AppConfig.useMockData) {
        _toast('Commentaire envoyé (démo).');
      } else {
        await ref.read(postRemoteDataSourceProvider).addComment(postId, text);
        ref.invalidate(allPostsProvider); // le commentaire apparaît au rechargement
      }
      _commentController.clear();
      if (mounted) FocusScope.of(context).unfocus();
    } on Failure catch (f) {
      _toast(f.message, error: true);
    } catch (e) {
      _toast('Échec de l\'envoi : $e', error: true);
    } finally {
      if (mounted) setState(() => _sending = false);
    }
  }

  void _toast(String msg, {bool error = false}) {
    if (!mounted) return;
    ScaffoldMessenger.of(context).showSnackBar(
      SnackBar(
        content: Text(msg),
        backgroundColor: error ? AppColors.heart : AppColors.primary,
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final postAsync = ref.watch(postByIdProvider(widget.postId));
    final overrides = ref.watch(likeOverrideProvider);

    return postAsync.when(
      loading: () => const Scaffold(body: Center(child: AppLoader())),
      error: (e, _) => Scaffold(
        body: AppErrorView(
          message: '$e',
          onRetry: () => ref.invalidate(allPostsProvider),
        ),
      ),
      data: (post) {
        if (post == null) {
          return const Scaffold(
            body: Center(child: Text('Publication introuvable')),
          );
        }

        final like = likeDisplay(overrides, post);
        final isLiked = like.liked;
        final displayLikes = like.count;

        return Scaffold(
          backgroundColor: AppColors.background,
          appBar: AppBar(
            backgroundColor: AppColors.surface,
            elevation: 0,
            leading: IconButton(
              icon: const Icon(Iconsax.arrow_left),
              onPressed: () => context.pop(),
            ),
            title: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(post.authorName,
                    style: const TextStyle(
                        fontSize: 16, fontWeight: FontWeight.w600)),
                Text(
                  post.activityName,
                  style: const TextStyle(
                      fontSize: 12,
                      color: AppColors.primary,
                      fontWeight: FontWeight.normal),
                ),
              ],
            ),
            actions: [
              IconButton(
                tooltip: 'Signaler',
                icon: const Icon(Iconsax.flag, size: 20),
                onPressed: () => ReportSheet.show(
                  context,
                  ref,
                  targetType: 'post',
                  targetId: post.id,
                  targetLabel: 'cette publication',
                ),
              ),
            ],
          ),
          body: Column(
            children: [
              Expanded(
                child: SingleChildScrollView(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      CachedNetworkImage(
                        imageUrl: post.imageUrl,
                        width: double.infinity,
                        height: 320,
                        fit: BoxFit.cover,
                        placeholder: (_, _) =>
                            Container(height: 320, color: AppColors.divider),
                      ),
                      Padding(
                        padding: const EdgeInsets.all(16),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              children: [
                                CircleAvatar(
                                  radius: 22,
                                  backgroundImage: CachedNetworkImageProvider(
                                      post.authorAvatarUrl),
                                  backgroundColor: AppColors.divider,
                                ),
                                const SizedBox(width: 12),
                                Expanded(
                                  child: Column(
                                    crossAxisAlignment:
                                        CrossAxisAlignment.start,
                                    children: [
                                      Text(
                                        post.authorName,
                                        style: const TextStyle(
                                          fontWeight: FontWeight.w700,
                                          fontSize: 15,
                                          color: AppColors.textDark,
                                        ),
                                      ),
                                      Text(
                                        DateFormatter.timeAgo(post.createdAt),
                                        style: const TextStyle(
                                            fontSize: 12,
                                            color: AppColors.textLight),
                                      ),
                                    ],
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 16),
                            Text(
                              post.caption,
                              style: const TextStyle(
                                fontSize: 15,
                                color: AppColors.textDark,
                                height: 1.5,
                              ),
                            ),
                            const SizedBox(height: 16),
                            Row(
                              children: [
                                GestureDetector(
                                  onTap: () => ref
                                      .read(likeOverrideProvider.notifier)
                                      .toggle(post.id, post.isLikedByMe),
                                  child: Row(
                                    children: [
                                      Icon(
                                        isLiked
                                            ? Iconsax.heart_copy
                                            : Iconsax.heart,
                                        color: isLiked
                                            ? AppColors.heart
                                            : AppColors.textLight,
                                        size: 24,
                                      ),
                                      const SizedBox(width: 6),
                                      Text(
                                        '$displayLikes J\'aime',
                                        style: const TextStyle(
                                          fontWeight: FontWeight.w600,
                                          color: AppColors.textMedium,
                                        ),
                                      ),
                                    ],
                                  ),
                                ),
                                const SizedBox(width: 20),
                                Icon(Iconsax.message,
                                    size: 22, color: AppColors.textLight),
                                const SizedBox(width: 6),
                                Text(
                                  '${post.comments.length} commentaire${post.comments.length > 1 ? 's' : ''}',
                                  style: const TextStyle(
                                    color: AppColors.textMedium,
                                    fontWeight: FontWeight.w600,
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 20),
                            const Divider(color: AppColors.divider),
                            const SizedBox(height: 12),
                            const Text(
                              'Commentaires',
                              style: TextStyle(
                                fontSize: 16,
                                fontWeight: FontWeight.w700,
                                color: AppColors.textDark,
                              ),
                            ),
                            const SizedBox(height: 12),
                            if (post.comments.isEmpty)
                              const Padding(
                                padding: EdgeInsets.symmetric(vertical: 8),
                                child: Text(
                                  'Soyez le premier à commenter.',
                                  style: TextStyle(
                                      color: AppColors.textLight, fontSize: 14),
                                ),
                              )
                            else
                              ...post.comments
                                  .map((c) => _CommentTile(comment: c)),
                            const SizedBox(height: 12),
                          ],
                        ),
                      ),
                    ],
                  ),
                ),
              ),
              _CommentComposer(
                controller: _commentController,
                sending: _sending,
                onSend: () => _submitComment(post.id),
              ),
            ],
          ),
        );
      },
    );
  }
}

/// Une ligne de commentaire, avec appui long → signaler.
class _CommentTile extends ConsumerWidget {
  final PostComment comment;
  const _CommentTile({required this.comment});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final canReport = comment.id.isNotEmpty;
    return Padding(
      padding: const EdgeInsets.only(bottom: 14),
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          CircleAvatar(
            radius: 16,
            backgroundColor: AppColors.primary.withValues(alpha: 0.15),
            child: Text(
              comment.authorName.isEmpty
                  ? '?'
                  : comment.authorName[0].toUpperCase(),
              style: const TextStyle(
                color: AppColors.primary,
                fontWeight: FontWeight.bold,
                fontSize: 13,
              ),
            ),
          ),
          const SizedBox(width: 10),
          Expanded(
            child: Container(
              padding: const EdgeInsets.all(12),
              decoration: BoxDecoration(
                color: AppColors.surface,
                borderRadius: BorderRadius.circular(12),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    children: [
                      Expanded(
                        child: Text(
                          comment.authorName,
                          style: const TextStyle(
                            fontWeight: FontWeight.bold,
                            fontSize: 13,
                            color: AppColors.textDark,
                          ),
                        ),
                      ),
                      if (canReport)
                        GestureDetector(
                          onTap: () => ReportSheet.show(
                            context,
                            ref,
                            targetType: 'comment',
                            targetId: comment.id,
                            targetLabel: 'ce commentaire',
                          ),
                          child: const Icon(Iconsax.flag,
                              size: 15, color: AppColors.textLight),
                        ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    comment.text,
                    style: const TextStyle(
                      fontSize: 14,
                      color: AppColors.textMedium,
                      height: 1.4,
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

/// Barre de saisie de commentaire ancrée en bas, au-dessus du clavier.
class _CommentComposer extends StatelessWidget {
  final TextEditingController controller;
  final bool sending;
  final VoidCallback onSend;

  const _CommentComposer({
    required this.controller,
    required this.sending,
    required this.onSend,
  });

  @override
  Widget build(BuildContext context) {
    return SafeArea(
      top: false,
      child: Container(
        padding: const EdgeInsets.fromLTRB(16, 8, 12, 8),
        decoration: const BoxDecoration(
          color: AppColors.surface,
          border: Border(top: BorderSide(color: AppColors.divider)),
        ),
        child: Row(
          children: [
            Expanded(
              child: TextField(
                controller: controller,
                minLines: 1,
                maxLines: 4,
                textInputAction: TextInputAction.send,
                onSubmitted: (_) => onSend(),
                decoration: InputDecoration(
                  hintText: 'Ajouter un commentaire…',
                  hintStyle: const TextStyle(color: AppColors.textLight),
                  filled: true,
                  fillColor: AppColors.background,
                  contentPadding: const EdgeInsets.symmetric(
                      horizontal: 16, vertical: 10),
                  border: OutlineInputBorder(
                    borderRadius: BorderRadius.circular(24),
                    borderSide: BorderSide.none,
                  ),
                ),
              ),
            ),
            const SizedBox(width: 8),
            IconButton(
              onPressed: sending ? null : onSend,
              icon: sending
                  ? const SizedBox(
                      width: 20,
                      height: 20,
                      child: CircularProgressIndicator(strokeWidth: 2),
                    )
                  : const Icon(Iconsax.send_1, color: AppColors.primary),
            ),
          ],
        ),
      ),
    );
  }
}
