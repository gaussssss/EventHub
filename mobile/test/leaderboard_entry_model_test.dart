import 'package:eventhub/features/profile/data/models/leaderboard_entry_model.dart';
import 'package:flutter_test/flutter_test.dart';

void main() {
  group('LeaderboardEntryModel.fromJson', () {
    test('parse une ligne complète (LeaderboardRow de l\'API)', () {
      final entry = LeaderboardEntryModel.fromJson({
        'rank': 3,
        'name': 'Tiya Florian',
        'avatarUrl': 'https://example.com/a.png',
        'hearts': 340,
        'isMe': true,
      });

      expect(entry.rank, 3);
      expect(entry.name, 'Tiya Florian');
      expect(entry.avatarUrl, 'https://example.com/a.png');
      expect(entry.hearts, 340);
      expect(entry.isMe, isTrue);
    });

    test('tolère avatar null et isMe absent (valeurs par défaut)', () {
      final entry = LeaderboardEntryModel.fromJson({
        'rank': 1,
        'name': 'Sophie',
        'avatarUrl': null,
        'hearts': 520,
      });

      expect(entry.avatarUrl, isNull);
      expect(entry.isMe, isFalse);
      expect(entry.hearts, 520);
    });
  });
}
