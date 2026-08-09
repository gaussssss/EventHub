class Activity {
  final String id;
  final String title;
  final String description;

  /// Slug brut de la catégorie (sert au filtrage côté backend).
  final String categorySlug;

  /// Libellé d'affichage de la catégorie.
  final String categoryLabel;

  final DateTime date;

  /// Fin de l'activité (nullable côté API).
  final DateTime? endDate;
  final String location;
  final String organizer;
  final String imageUrl;
  final int hearts;
  final int maxParticipants;
  final int currentParticipants;
  final String? registrationUrl;
  final DateTime? registrationDeadline;

  /// Statut d'inscription de l'utilisateur courant, renseigné uniquement par
  /// « mes inscriptions » : `registered` | `attended` | `noshow` | `waitlisted`.
  /// `null` pour une activité vue hors de ce contexte.
  final String? myStatus;

  /// Coût de participation en $ (0 = gratuit). Purement informatif.
  final double participationCost;

  const Activity({
    required this.id,
    required this.title,
    required this.description,
    required this.categorySlug,
    required this.categoryLabel,
    required this.date,
    this.endDate,
    required this.location,
    required this.organizer,
    required this.imageUrl,
    required this.hearts,
    required this.maxParticipants,
    required this.currentParticipants,
    this.registrationUrl,
    this.registrationDeadline,
    this.myStatus,
    this.participationCost = 0,
  });

  // --- Fenêtre de confirmation de présence (scan du QR) --------------------
  // MIROIR des constantes serveur (SelfCheckInHandler) : ouverture 2 h avant le
  // début, fermeture 2 h après la fin (durée par défaut 4 h si pas de fin).
  // Toute modification doit être faite DES DEUX CÔTÉS.

  DateTime get checkInOpensAt => date.subtract(const Duration(hours: 2));

  DateTime get checkInClosesAt =>
      (endDate ?? date.add(const Duration(hours: 4)))
          .add(const Duration(hours: 2));

  /// Vrai si l'émargement est ouvert à l'instant [now].
  bool isCheckInOpen(DateTime now) =>
      !now.isBefore(checkInOpensAt) && !now.isAfter(checkInClosesAt);
}
