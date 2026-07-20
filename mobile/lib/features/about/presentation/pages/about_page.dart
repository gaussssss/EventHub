import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/platform_icons.dart';
import '../../../../core/widgets/brand_logo.dart';
import '../../domain/entities/contributor.dart';
import '../providers/about_provider.dart';

/// Page « À propos » : identité de l'app + contributeurs (gérés au back office).
class AboutPage extends ConsumerWidget {
  const AboutPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final contributorsAsync = ref.watch(contributorsProvider);

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.surface,
        leading: IconButton(
          icon: Icon(PlatformIcons.back),
          onPressed: () => context.pop(),
        ),
        title: const Text('À propos'),
      ),
      body: RefreshIndicator(
        color: AppColors.primary,
        onRefresh: () async {
          ref.invalidate(contributorsProvider);
          await ref.read(contributorsProvider.future);
        },
        child: ListView(
          physics: const AlwaysScrollableScrollPhysics(),
          padding: const EdgeInsets.fromLTRB(20, 28, 20, 40),
          children: [
            const Center(child: BrandLogo(height: 96)),
            const SizedBox(height: 16),
            const Text(
              'UQTR en santé',
              textAlign: TextAlign.center,
              style: TextStyle(
                fontSize: 22,
                fontWeight: FontWeight.w800,
                color: AppColors.textDark,
                letterSpacing: -0.4,
              ),
            ),
            const SizedBox(height: 8),
            const Text(
              'Bougez, participez, gagnez des cœurs santé.\n'
              'L\'app des activités sportives et socioculturelles de l\'UQTR.',
              textAlign: TextAlign.center,
              style: TextStyle(
                  fontSize: 14, color: AppColors.textMedium, height: 1.5),
            ),
            const SizedBox(height: 32),
            const Text(
              'Contributeurs',
              style: TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.w700,
                color: AppColors.textDark,
                letterSpacing: -0.3,
              ),
            ),
            const SizedBox(height: 12),
            contributorsAsync.when(
              loading: () => const Padding(
                padding: EdgeInsets.symmetric(vertical: 32),
                child: Center(child: CircularProgressIndicator()),
              ),
              error: (_, _) => const Padding(
                padding: EdgeInsets.symmetric(vertical: 16),
                child: Text(
                  'Contributeurs indisponibles pour le moment.',
                  style: TextStyle(color: AppColors.textLight),
                ),
              ),
              data: (contributors) => contributors.isEmpty
                  ? const Padding(
                      padding: EdgeInsets.symmetric(vertical: 16),
                      child: Text(
                        'Aucun contributeur renseigné.',
                        style: TextStyle(color: AppColors.textLight),
                      ),
                    )
                  : Column(
                      children: contributors
                          .map((c) => _ContributorTile(contributor: c))
                          .toList(),
                    ),
            ),
            const SizedBox(height: 32),
            const Center(
              child: Text(
                'UQTR, Université du Québec à Trois-Rivières',
                style: TextStyle(fontSize: 12, color: AppColors.textLight),
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _ContributorTile extends StatelessWidget {
  final Contributor contributor;
  const _ContributorTile({required this.contributor});

  @override
  Widget build(BuildContext context) {
    final avatar = contributor.avatarUrl;
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      padding: const EdgeInsets.all(14),
      decoration: BoxDecoration(
        color: AppColors.surface,
        borderRadius: BorderRadius.circular(14),
        boxShadow: const [
          BoxShadow(
            color: AppColors.cardShadow,
            blurRadius: 8,
            offset: Offset(0, 2),
          ),
        ],
      ),
      child: Row(
        children: [
          CircleAvatar(
            radius: 22,
            backgroundColor: AppColors.primary.withValues(alpha: 0.1),
            backgroundImage:
                avatar != null ? CachedNetworkImageProvider(avatar) : null,
            child: avatar == null
                ? const Icon(Iconsax.profile_circle,
                    color: AppColors.primary, size: 22)
                : null,
          ),
          const SizedBox(width: 14),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  contributor.name,
                  style: const TextStyle(
                    fontSize: 15,
                    fontWeight: FontWeight.w700,
                    color: AppColors.textDark,
                  ),
                ),
                const SizedBox(height: 2),
                Text(
                  contributor.role,
                  style: const TextStyle(
                      fontSize: 13, color: AppColors.textMedium),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
