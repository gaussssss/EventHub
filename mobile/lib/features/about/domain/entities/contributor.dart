/// Contributeur du projet, affiché sur la page « À propos »
/// (géré depuis le back office, `GET /api/about/contributors`).
class Contributor {
  final String name;
  final String role;
  final String? avatarUrl;

  const Contributor({required this.name, required this.role, this.avatarUrl});
}
