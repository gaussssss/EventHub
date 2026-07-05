import '../../domain/entities/user_profile.dart';

class ProfileLocalDataSource {
  UserProfile get currentUser => _currentUser;

  static final UserProfile _currentUser = UserProfile(
    id: 'user001',
    name: 'Alex Tremblay',
    email: 'alex.tremblay@uqtr.ca',
    avatarUrl:
        'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&fit=crop&q=80',
    totalHearts: 340,
    completedActivityIds: ['act002', 'act006', 'act007'],
    heartHistory: [
      HeartHistory(
        activityTitle: 'Yoga matinal',
        hearts: 20,
        date: DateTime(2026, 5, 15),
      ),
      HeartHistory(
        activityTitle: 'Atelier de poterie',
        hearts: 35,
        date: DateTime(2026, 5, 22),
      ),
      HeartHistory(
        activityTitle: 'Conférence : Santé mentale',
        hearts: 30,
        date: DateTime(2026, 6, 1),
      ),
      HeartHistory(
        activityTitle: 'Course en forêt',
        hearts: 30,
        date: DateTime(2026, 5, 10),
      ),
      HeartHistory(
        activityTitle: 'Tournoi de basketball',
        hearts: 50,
        date: DateTime(2026, 4, 20),
      ),
      HeartHistory(
        activityTitle: 'Cinéma en plein air',
        hearts: 25,
        date: DateTime(2026, 4, 5),
      ),
      HeartHistory(
        activityTitle: 'Atelier de peinture',
        hearts: 35,
        date: DateTime(2026, 3, 18),
      ),
      HeartHistory(
        activityTitle: 'Méditation guidée',
        hearts: 25,
        date: DateTime(2026, 3, 5),
      ),
      HeartHistory(
        activityTitle: 'Festival culturel UQTR',
        hearts: 60,
        date: DateTime(2026, 2, 15),
      ),
      HeartHistory(
        activityTitle: 'Yoga matinal',
        hearts: 20,
        date: DateTime(2026, 2, 3),
      ),
      HeartHistory(
        activityTitle: 'Tournoi de volleyball',
        hearts: 40,
        date: DateTime(2026, 1, 22),
      ),
      HeartHistory(
        activityTitle: 'Course en forêt',
        hearts: 20,
        date: DateTime(2026, 1, 10),
      ),
    ],
  );
}
