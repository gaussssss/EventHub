import '../../../../core/utils/media_url.dart';
import '../../domain/entities/activity.dart';

class ActivityModel extends Activity {
  const ActivityModel({
    required super.id,
    required super.title,
    required super.description,
    required super.categorySlug,
    required super.categoryLabel,
    required super.date,
    super.endDate,
    required super.location,
    required super.organizer,
    required super.imageUrl,
    required super.hearts,
    required super.maxParticipants,
    required super.currentParticipants,
    super.registrationUrl,
    super.registrationDeadline,
    super.myStatus,
    super.participationCost,
  });

  /// Construit le modèle depuis la réponse JSON de l'API
  /// (`GET /activities`, `GET /activities/:id`).
  factory ActivityModel.fromJson(Map<String, dynamic> json) {
    return ActivityModel(
      id: json['id'] as String,
      title: json['title'] as String,
      description: json['description'] as String,
      categorySlug: (json['category'] ?? '') as String,
      categoryLabel: _labelFromSlug(json['category'] as String?),
      date: DateTime.parse(json['startsAt'] as String).toLocal(),
      endDate: json['endsAt'] == null
          ? null
          : DateTime.parse(json['endsAt'] as String).toLocal(),
      location: json['location'] as String,
      organizer: (json['organizer'] ?? '') as String,
      imageUrl: resolveMediaUrl(json['imageUrl'] as String?),
      hearts: (json['heartsReward'] ?? json['hearts'] ?? 0) as int,
      maxParticipants: json['maxParticipants'] as int,
      currentParticipants: (json['currentParticipants'] ?? 0) as int,
      registrationUrl: json['registrationUrl'] as String?,
      registrationDeadline: json['registrationDeadline'] == null
          ? null
          : DateTime.parse(json['registrationDeadline'] as String).toLocal(),
      myStatus: json['myStatus'] as String?,
      participationCost:
          ((json['participationCost'] ?? 0) as num).toDouble(),
    );
  }

  Map<String, dynamic> toJson() => {
        'id': id,
        'title': title,
        'description': description,
        'category': categorySlug,
        'startsAt': date.toUtc().toIso8601String(),
        'endsAt': endDate?.toUtc().toIso8601String(),
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

  /// Libellé lisible dérivé du slug (repli quand l'API n'en fournit pas) :
  /// « seed-culture » → « Culture », « sport » → « Sport ».
  static String _labelFromSlug(String? slug) {
    if (slug == null || slug.isEmpty) return 'Autre';
    final cleaned = slug.startsWith('seed-') ? slug.substring(5) : slug;
    return cleaned
        .split('-')
        .where((w) => w.isNotEmpty)
        .map((w) => w[0].toUpperCase() + w.substring(1))
        .join(' ');
  }
}
