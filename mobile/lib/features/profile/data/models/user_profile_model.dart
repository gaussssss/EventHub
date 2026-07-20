import '../../../../core/utils/media_url.dart';
import '../../domain/entities/user_profile.dart';

class UserProfileModel extends UserProfile {
  const UserProfileModel({
    required super.id,
    required super.name,
    required super.email,
    required super.avatarUrl,
    required super.totalHearts,
    required super.completedActivityIds,
    required super.heartHistory,
  });

  /// Depuis `GET /me`.
  factory UserProfileModel.fromJson(Map<String, dynamic> json) {
    return UserProfileModel(
      id: json['id'] as String,
      name: json['name'] as String,
      email: json['email'] as String,
      avatarUrl: resolveMediaUrl(json['avatarUrl'] as String?),
      totalHearts: (json['totalHearts'] ?? 0) as int,
      completedActivityIds:
          (json['completedActivityIds'] as List<dynamic>? ?? [])
              .map((e) => e as String)
              .toList(),
      heartHistory: (json['heartHistory'] as List<dynamic>? ?? [])
          .map((e) => _historyFromJson(e as Map<String, dynamic>))
          .toList(),
    );
  }

  static HeartHistory _historyFromJson(Map<String, dynamic> json) {
    return HeartHistory(
      activityTitle: json['activityTitle'] as String,
      hearts: json['hearts'] as int,
      date: DateTime.parse(json['date'] as String).toLocal(),
    );
  }
}
