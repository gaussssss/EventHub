/// Statistiques communautaires affichées sur l'accueil et le profil
/// (`GET /api/stats/community`).
class CommunityStats {
  final int totalRegisteredUsers;
  final int totalUqtrHearts;

  const CommunityStats({
    required this.totalRegisteredUsers,
    required this.totalUqtrHearts,
  });
}
