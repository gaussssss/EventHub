import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'core/router/app_router.dart';
import 'core/theme/app_theme.dart';
import 'features/auth/domain/entities/user.dart';
import 'features/auth/presentation/providers/auth_provider.dart';

void main() {
  runApp(const ProviderScope(child: EventHubApp()));
}

class EventHubApp extends ConsumerWidget {
  const EventHubApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(routerProvider);

    // Déconnexion globale : dès que la session passe de « connecté » à « null »
    // (déconnexion volontaire ou jeton expiré → 401), on ramène au login. On ne
    // réagit qu'à cette transition précise pour ne pas court-circuiter le splash
    // au démarrage (état initial loading/null, sans utilisateur précédent).
    ref.listen<AsyncValue<User?>>(authControllerProvider, (prev, next) {
      if (prev?.valueOrNull != null && next.valueOrNull == null) {
        router.go('/login');
      }
    });

    return MaterialApp.router(
      title: 'UQTR en santé',
      debugShowCheckedModeBanner: false,
      theme: AppTheme.light,
      routerConfig: router,
    );
  }
}
