import '../../../../core/network/api_client.dart';
import '../../domain/entities/category.dart';

/// Accès distant aux catégories (`GET /api/categories`).
class CategoryRemoteDataSource {
  final ApiClient _client;

  CategoryRemoteDataSource(this._client);

  Future<List<Category>> getCategories() async {
    final data = await _client.get('/api/categories');
    return (data as List)
        .map((json) => Category(
              slug: (json['slug'] ?? '') as String,
              label: (json['label'] ?? '') as String,
            ))
        .toList();
  }
}
