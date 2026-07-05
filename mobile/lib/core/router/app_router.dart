import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import '../../features/auth/presentation/pages/login_page.dart';
import '../../features/auth/presentation/pages/splash_page.dart';
import '../../features/activities/presentation/pages/activity_detail_page.dart';
import '../../features/activities/presentation/pages/catalogue_page.dart';
import '../../features/activities/presentation/pages/registration_confirmation_page.dart';
import '../../features/activities/presentation/pages/registration_webview_page.dart';
import '../../features/home/presentation/pages/home_page.dart';
import '../../features/social/presentation/pages/post_detail_page.dart';
import '../../features/social/presentation/pages/publish_photo_page.dart';
import '../../features/profile/presentation/pages/profile_page.dart';
import '../../features/profile/presentation/pages/hearts_page.dart';
import '../widgets/main_scaffold.dart';

final routerProvider = Provider<GoRouter>((ref) {
  return GoRouter(
    initialLocation: '/splash',
    routes: [
      GoRoute(
        path: '/splash',
        builder: (context, state) => const SplashPage(),
      ),
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginPage(),
      ),
      GoRoute(
        path: '/activity/:id',
        builder: (context, state) => ActivityDetailPage(
          activityId: state.pathParameters['id']!,
        ),
      ),
      GoRoute(
        path: '/activity/:id/confirmation',
        builder: (context, state) => RegistrationConfirmationPage(
          activityId: state.pathParameters['id']!,
        ),
      ),
      GoRoute(
        path: '/activity/:id/register',
        builder: (context, state) => RegistrationWebViewPage(
          activityId: state.pathParameters['id']!,
          url: Uri.decodeComponent(
              state.uri.queryParameters['url'] ?? ''),
        ),
      ),
      GoRoute(
        path: '/post/:id',
        builder: (context, state) => PostDetailPage(
          postId: state.pathParameters['id']!,
        ),
      ),
      GoRoute(
        path: '/create-post',
        builder: (context, state) => const PublishPhotoPage(),
      ),
      GoRoute(
        path: '/hearts',
        builder: (context, state) => const HeartsPage(),
      ),
      StatefulShellRoute.indexedStack(
        builder: (context, state, shell) =>
            MainScaffold(shell: shell),
        branches: [
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/home',
                builder: (context, state) => const HomePage(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/catalogue',
                builder: (context, state) => const CataloguePage(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: '/profile',
                builder: (context, state) => const ProfilePage(),
              ),
            ],
          ),
        ],
      ),
    ],
  );
});
