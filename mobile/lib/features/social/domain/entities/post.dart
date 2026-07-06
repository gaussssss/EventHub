class PostComment {
  final String id;
  final String authorName;
  final String text;
  final DateTime createdAt;

  const PostComment({
    this.id = '',
    required this.authorName,
    required this.text,
    required this.createdAt,
  });
}

class Post {
  final String id;
  final String authorName;
  final String authorAvatarUrl;
  final String imageUrl;
  final String caption;
  final String activityName;
  final DateTime createdAt;
  final int likesCount;

  /// Vrai si l'utilisateur connecté a aimé ce post (renvoyé par l'API).
  final bool isLikedByMe;

  final List<PostComment> comments;

  const Post({
    required this.id,
    required this.authorName,
    required this.authorAvatarUrl,
    required this.imageUrl,
    required this.caption,
    required this.activityName,
    required this.createdAt,
    required this.likesCount,
    this.isLikedByMe = false,
    this.comments = const [],
  });
}
