/// Catégorie d'activité (chip de filtre), issue de `GET /api/categories`.
class Category {
  final String slug;
  final String label;

  const Category({required this.slug, required this.label});
}
