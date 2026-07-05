enum ActivityCategory { sport, socioculturel }

class Activity {
  final String id;
  final String title;
  final String description;
  final ActivityCategory category;
  final DateTime date;
  final String location;
  final String organizer;
  final String imageUrl;
  final int hearts;
  final int maxParticipants;
  final int currentParticipants;
  final String? registrationUrl;
  final DateTime? registrationDeadline;

  const Activity({
    required this.id,
    required this.title,
    required this.description,
    required this.category,
    required this.date,
    required this.location,
    required this.organizer,
    required this.imageUrl,
    required this.hearts,
    required this.maxParticipants,
    required this.currentParticipants,
    this.registrationUrl,
    this.registrationDeadline,
  });

  String get categoryLabel =>
      category == ActivityCategory.sport ? 'Sport' : 'Socioculturel';
}
