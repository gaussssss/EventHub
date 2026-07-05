/// Erreur métier normalisée, présentable à l'utilisateur.
///
/// Les `RemoteDataSource` traduiront les erreurs réseau/HTTP en [Failure]
/// (ex: 401 → [Failure.unauthorized], pas de réseau → [Failure.network]).
class Failure implements Exception {
  final String message;
  final FailureType type;

  const Failure(this.message, {this.type = FailureType.unknown});

  const Failure.network()
      : message = 'Connexion impossible. Vérifiez votre réseau.',
        type = FailureType.network;

  const Failure.unauthorized()
      : message = 'Session expirée. Veuillez vous reconnecter.',
        type = FailureType.unauthorized;

  const Failure.notFound()
      : message = 'Ressource introuvable.',
        type = FailureType.notFound;

  const Failure.server()
      : message = 'Une erreur est survenue. Réessayez plus tard.',
        type = FailureType.server;

  @override
  String toString() => message;
}

enum FailureType { network, unauthorized, notFound, server, unknown }
