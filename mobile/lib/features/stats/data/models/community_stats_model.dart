import '../../domain/entities/community_stats.dart';

class CommunityStatsModel extends CommunityStats {
  const CommunityStatsModel({
    required super.totalRegisteredUsers,
    required super.totalUqtrHearts,
  });

  factory CommunityStatsModel.fromJson(Map<String, dynamic> json) {
    return CommunityStatsModel(
      totalRegisteredUsers: (json['totalRegisteredUsers'] as num?)?.toInt() ?? 0,
      totalUqtrHearts: (json['totalUqtrHearts'] as num?)?.toInt() ?? 0,
    );
  }
}
