class HeartHistory {
  final String activityTitle;
  final int hearts;
  final DateTime date;

  const HeartHistory({
    required this.activityTitle,
    required this.hearts,
    required this.date,
  });
}

class UserProfile {
  final String id;
  final String name;
  final String email;
  final String avatarUrl;
  final int totalHearts;
  final List<String> completedActivityIds;
  final List<HeartHistory> heartHistory;

  const UserProfile({
    required this.id,
    required this.name,
    required this.email,
    required this.avatarUrl,
    required this.totalHearts,
    required this.completedActivityIds,
    required this.heartHistory,
  });

  String get level {
    if (totalHearts >= 500) return 'Or';
    if (totalHearts >= 200) return 'Argent';
    return 'Bronze';
  }

  int get nextLevelThreshold {
    if (totalHearts >= 500) return 1000;
    if (totalHearts >= 200) return 500;
    return 200;
  }

  int get previousLevelThreshold {
    if (totalHearts >= 500) return 500;
    if (totalHearts >= 200) return 200;
    return 0;
  }
}
