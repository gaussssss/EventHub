@Tags(['live'])
library;

import 'package:eventhub/core/network/api_client.dart';
import 'package:eventhub/core/storage/token_storage.dart';
import 'package:eventhub/features/activities/data/datasources/activity_remote_datasource.dart';
import 'package:flutter_test/flutter_test.dart';

/// Test d'intégration « live » : nécessite l'API EventHub en marche
/// (http://localhost:5199) avec au moins une activité publiée.
///   dart run  → API en mode dev, puis :
///   flutter test test/activity_remote_datasource_live_test.dart
///
/// TokenStorage neutralisé (endpoints publics, pas de canal de plateforme en test).
class _NoTokenStorage extends TokenStorage {
  @override
  Future<String?> readAccessToken() async => null;
}

void main() {
  test('ActivityRemoteDataSource récupère et parse le catalogue live', () async {
    final dataSource = ActivityRemoteDataSource(ApiClient(_NoTokenStorage()));

    final activities = await dataSource.getActivities();
    expect(activities, isNotEmpty, reason: 'seed attendu côté API');

    final first = activities.first;
    expect(first.id, isNotEmpty);
    expect(first.title, isNotEmpty);
    expect(first.maxParticipants, greaterThan(0));

    final detail = await dataSource.getActivityById(first.id);
    expect(detail, isNotNull);
    expect(detail!.id, first.id);

    final missing = await dataSource.getActivityById(
      '00000000-0000-0000-0000-000000000000',
    );
    expect(missing, isNull, reason: '404 → null');
  });
}
