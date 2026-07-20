import '../../domain/entities/activity.dart';
import '../models/activity_model.dart';

class ActivityLocalDataSource {
  List<Activity> get activities => _activities;

  static final List<ActivityModel> _activities = [
    ActivityModel(
      id: 'act001',
      title: 'Course en forêt',
      description:
          'Rejoignez-nous pour une course matinale dans les sentiers autour du campus. Parfait pour tous les niveaux, du débutant au coureur confirmé. Eau fournie au départ.',
      categorySlug: 'sport',
      categoryLabel: 'Sport',
      date: DateTime(2026, 7, 6, 8, 0),
      registrationDeadline: DateTime(2026, 7, 4, 23, 59),
      location: 'Parc portuaire, Trois-Rivières',
      organizer: 'Club de course UQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1552674605-db6ffd4facb5?w=800&fit=crop&q=80',
      hearts: 30,
      maxParticipants: 40,
      currentParticipants: 18,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample001/viewform',
    ),
    ActivityModel(
      id: 'act002',
      title: 'Yoga matinal',
      description:
          'Commencez votre journée en douceur avec une session de yoga guidée. Réduisez le stress et améliorez votre flexibilité. Tapis fournis ou apportez le vôtre.',
      categorySlug: 'sport',
      categoryLabel: 'Sport',
      date: DateTime(2026, 7, 7, 7, 30),
      registrationDeadline: DateTime(2026, 7, 5, 23, 59),
      location: 'Gymnase principal UQTR',
      organizer: 'Service des sports UQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1544367567-0f2fcb009e0b?w=800&fit=crop&q=80',
      hearts: 20,
      maxParticipants: 25,
      currentParticipants: 12,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample002/viewform',
    ),
    ActivityModel(
      id: 'act003',
      title: 'Tournoi de basketball',
      description:
          'Tournoi interéquipes en format 3 contre 3. Inscrivez votre équipe ou venez seul et on vous trouvera une équipe. Trophées pour les trois premières équipes.',
      categorySlug: 'sport',
      categoryLabel: 'Sport',
      date: DateTime(2026, 7, 11, 14, 0),
      registrationDeadline: DateTime(2026, 7, 8, 23, 59),
      location: 'Complexe sportif Gilles-Côté',
      organizer: 'Association sportive UQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1546519638-68e109498ffc?w=800&fit=crop&q=80',
      hearts: 50,
      maxParticipants: 60,
      currentParticipants: 42,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample003/viewform',
    ),
    ActivityModel(
      id: 'act004',
      title: 'Tournoi de volleyball',
      description:
          'Championnat annuel de volleyball du campus. Matchs en double élimination. Ambiance garantie et collations offertes entre les matchs.',
      categorySlug: 'sport',
      categoryLabel: 'Sport',
      date: DateTime(2026, 7, 14, 10, 0),
      registrationDeadline: DateTime(2026, 7, 11, 23, 59),
      location: 'Gymnase secondaire UQTR',
      organizer: 'Service des sports UQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1612872087720-bb876e2e67d1?w=800&fit=crop&q=80',
      hearts: 40,
      maxParticipants: 48,
      currentParticipants: 24,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample004/viewform',
    ),
    ActivityModel(
      id: 'act005',
      title: 'Méditation guidée',
      description:
          'Session de méditation pleine conscience pour réduire le stress et améliorer la concentration. Aucune expérience requise. Coussin fourni.',
      categorySlug: 'sport',
      categoryLabel: 'Sport',
      date: DateTime(2026, 7, 18, 12, 0),
      registrationDeadline: DateTime(2026, 7, 15, 23, 59),
      location: 'Centre de bien-être UQTR',
      organizer: 'Service de santé UQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1506126613408-eca07ce68773?w=800&fit=crop&q=80',
      hearts: 25,
      maxParticipants: 20,
      currentParticipants: 8,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample005/viewform',
    ),
    ActivityModel(
      id: 'act006',
      title: 'Atelier de poterie',
      description:
          'Découvrez l\'art de la poterie avec notre artiste résidente. Apprenez les techniques de base du tournage et du modelage. Tablier fourni, vous repartez avec votre création.',
      categorySlug: 'socioculturel',
      categoryLabel: 'Socioculturel',
      date: DateTime(2026, 7, 9, 13, 0),
      registrationDeadline: DateTime(2026, 7, 7, 23, 59),
      location: 'Pavillon des arts UQTR',
      organizer: 'Département des arts UQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1565193566173-7a0ee3dbe261?w=800&fit=crop&q=80',
      hearts: 35,
      maxParticipants: 15,
      currentParticipants: 10,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample006/viewform',
    ),
    ActivityModel(
      id: 'act007',
      title: 'Conférence : Santé mentale',
      description:
          'Conférence animée par des professionnels sur la gestion du stress et l\'équilibre travail-vie personnelle. Panel de discussion ouvert au public UQTR.',
      categorySlug: 'socioculturel',
      categoryLabel: 'Socioculturel',
      date: DateTime(2026, 7, 13, 16, 0),
      registrationDeadline: DateTime(2026, 7, 11, 23, 59),
      location: 'Amphithéâtre Cogeco UQTR',
      organizer: 'Service des étudiants UQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=800&fit=crop&q=80',
      hearts: 30,
      maxParticipants: 200,
      currentParticipants: 87,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample007/viewform',
    ),
    ActivityModel(
      id: 'act008',
      title: 'Cinéma en plein air',
      description:
          'Soirée cinéma sous les étoiles sur l\'esplanade du campus. Film surprise ! Apportez votre couverture et votre bonne humeur. Maïs soufflé offert.',
      categorySlug: 'socioculturel',
      categoryLabel: 'Socioculturel',
      date: DateTime(2026, 7, 17, 21, 0),
      registrationDeadline: DateTime(2026, 7, 14, 23, 59),
      location: 'Esplanade du campus UQTR',
      organizer: 'AEUQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1489599849927-2ee91cede3ba?w=800&fit=crop&q=80',
      hearts: 25,
      maxParticipants: 150,
      currentParticipants: 63,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample008/viewform',
    ),
    ActivityModel(
      id: 'act009',
      title: 'Atelier de peinture',
      description:
          'Initiez-vous à la peinture acrylique avec notre artiste invitée. Vous repartirez avec votre œuvre originale encadrée. Tout le matériel est fourni.',
      categorySlug: 'socioculturel',
      categoryLabel: 'Socioculturel',
      date: DateTime(2026, 7, 21, 14, 0),
      registrationDeadline: DateTime(2026, 7, 18, 23, 59),
      location: 'Studio des arts UQTR',
      organizer: 'Club des arts UQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1513364776144-60967b0f800f?w=800&fit=crop&q=80',
      hearts: 35,
      maxParticipants: 20,
      currentParticipants: 15,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample009/viewform',
    ),
    ActivityModel(
      id: 'act010',
      title: 'Festival culturel UQTR',
      description:
          'Grande célébration de la diversité culturelle du campus. Nourriture, musique, danses et activités du monde entier. Venez partager votre culture !',
      categorySlug: 'socioculturel',
      categoryLabel: 'Socioculturel',
      date: DateTime(2026, 7, 25, 11, 0),
      registrationDeadline: DateTime(2026, 7, 22, 23, 59),
      location: 'Campus UQTR, Aire commune',
      organizer: 'Bureau international UQTR',
      imageUrl:
          'https://images.unsplash.com/photo-1533174072545-7a4b6ad7a6c3?w=800&fit=crop&q=80',
      hearts: 60,
      maxParticipants: 500,
      currentParticipants: 234,
      registrationUrl: 'https://docs.google.com/forms/d/e/1FAIpQLSexample010/viewform',
    ),
  ];
}
