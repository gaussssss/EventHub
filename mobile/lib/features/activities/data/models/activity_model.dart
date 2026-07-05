import '../../domain/entities/activity.dart';

class ActivityModel extends Activity {
  const ActivityModel({
    required super.id,
    required super.title,
    required super.description,
    required super.category,
    required super.date,
    required super.location,
    required super.organizer,
    required super.imageUrl,
    required super.hearts,
    required super.maxParticipants,
    required super.currentParticipants,
    super.registrationUrl,
    super.registrationDeadline,
  });

  /// Construit le modèle depuis la réponse JSON de l'API
  /// (`GET /activities`, `GET /activities/:id`).
  factory ActivityModel.fromJson(Map<String, dynamic> json) {
    return ActivityModel(
      id: json['id'] as String,
      title: json['title'] as String,
      description: json['description'] as String,
      category: _categoryFromSlug(json['category'] as String?),
      date: DateTime.parse(json['startsAt'] as String).toLocal(),
      location: json['location'] as String,
      organizer: (json['organizer'] ?? '') as String,
      imageUrl: json['imageUrl'] as String,
      hearts: (json['heartsReward'] ?? json['hearts'] ?? 0) as int,
      maxParticipants: json['maxParticipants'] as int,
      currentParticipants: (json['currentParticipants'] ?? 0) as int,
      registrationUrl: json['registrationUrl'] as String?,
      registrationDeadline: json['registrationDeadline'] == null
          ? null
          : DateTime.parse(json['registrationDeadline'] as String).toLocal(),
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'title': title,
        'description': description,
        'category': category.name,
        'startsAt': date.toUtc().toIso8601String(),
        'location': location,
        'organizer': organizer,
        'imageUrl': imageUrl,
        'heartsReward': hearts,
        'maxParticipants': maxParticipants,
        'currentParticipants': currentParticipants,
        'registrationUrl': registrationUrl,
        'registrationDeadline':
            registrationDeadline?.toUtc().toIso8601String(),
      };

  static ActivityCategory _categoryFromSlug(String? slug) {
    switch (slug) {
      case 'sport':
        return ActivityCategory.sport;
      case 'socioculturel':
      default:
        return ActivityCategory.socioculturel;
    }
  }
}
