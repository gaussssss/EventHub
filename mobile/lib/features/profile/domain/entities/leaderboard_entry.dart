/// Une ligne du classement des cœurs (`GET /api/leaderboard`). Le rang est
/// 1-indexé ; [isMe] marque la ligne de l'utilisateur connecté.
class LeaderboardEntry {
  final int rank;
  final String name;
  final String? avatarUrl;
  final int hearts;
  final bool isMe;

  const LeaderboardEntry({
    required this.rank,
    required this.name,
    required this.hearts,
    this.avatarUrl,
    this.isMe = false,
  });
}
