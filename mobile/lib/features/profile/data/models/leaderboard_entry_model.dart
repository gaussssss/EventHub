import '../../domain/entities/leaderboard_entry.dart';

class LeaderboardEntryModel extends LeaderboardEntry {
  const LeaderboardEntryModel({
    required super.rank,
    required super.name,
    required super.hearts,
    super.avatarUrl,
    super.isMe,
  });

  /// Depuis une ligne de `GET /api/leaderboard` (`LeaderboardRow`).
  factory LeaderboardEntryModel.fromJson(Map<String, dynamic> json) {
    return LeaderboardEntryModel(
      rank: (json['rank'] ?? 0) as int,
      name: (json['name'] ?? 'Anonyme') as String,
      avatarUrl: json['avatarUrl'] as String?,
      hearts: (json['hearts'] ?? 0) as int,
      isMe: (json['isMe'] ?? false) as bool,
    );
  }
}
