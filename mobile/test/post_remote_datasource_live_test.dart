@Tags(['live'])
library;

import 'package:eventhub/core/network/api_client.dart';
import 'package:eventhub/core/storage/token_storage.dart';
import 'package:eventhub/features/social/data/datasources/post_remote_datasource.dart';
import 'package:flutter_test/flutter_test.dart';

/// Test live : API en marche (http://localhost:5199) avec au moins un post publié.
///   flutter test --run-skipped --tags live test/post_remote_datasource_live_test.dart
class _NoTokenStorage extends TokenStorage {
  @override
  Future<String?> readAccessToken() async => null;
}

void main() {
  test('PostRemoteDataSource récupère et parse le fil live', () async {
    final dataSource = PostRemoteDataSource(ApiClient(_NoTokenStorage()));

    final posts = await dataSource.getPosts();
    expect(posts, isNotEmpty, reason: 'seed attendu côté API');

    final first = posts.first;
    expect(first.id, isNotEmpty);
    expect(first.imageUrl, isNotEmpty);
    expect(first.caption, isNotEmpty);

    final detail = await dataSource.getPostById(first.id);
    expect(detail, isNotNull);
    expect(detail!.id, first.id);

    final missing = await dataSource.getPostById(
      '00000000-0000-0000-0000-000000000000',
    );
    expect(missing, isNull);
  });
}
