import '../../../../core/utils/media_url.dart';
import '../../domain/entities/post.dart';

class PostModel extends Post {
  const PostModel({
    required super.id,
    required super.authorName,
    required super.authorAvatarUrl,
    required super.imageUrl,
    required super.caption,
    required super.activityName,
    required super.createdAt,
    required super.likesCount,
    super.isLikedByMe,
    super.comments,
  });

  factory PostModel.fromJson(Map<String, dynamic> json) {
    return PostModel(
      id: json['id'] as String,
      authorName: (json['authorName'] ?? '') as String,
      authorAvatarUrl: resolveMediaUrl(json['authorAvatarUrl'] as String?),
      imageUrl: resolveMediaUrl(json['imageUrl'] as String?),
      caption: json['caption'] as String,
      activityName: (json['activityName'] ?? '') as String,
      createdAt: DateTime.parse(json['createdAt'] as String).toLocal(),
      likesCount: (json['likesCount'] ?? 0) as int,
      isLikedByMe: (json['isLikedByMe'] ?? false) as bool,
      comments: (json['comments'] as List<dynamic>? ?? [])
          .map((c) => _commentFromJson(c as Map<String, dynamic>))
          .toList(),
    );
  }

  static PostComment _commentFromJson(Map<String, dynamic> json) {
    return PostComment(
      id: (json['id'] ?? '') as String,
      authorName: json['authorName'] as String,
      text: json['text'] as String,
      createdAt: DateTime.parse(json['createdAt'] as String).toLocal(),
    );
  }
}
