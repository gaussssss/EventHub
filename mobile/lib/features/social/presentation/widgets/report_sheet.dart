import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/error/failure.dart';
import '../providers/post_provider.dart';

/// Feuille de signalement de contenu (publication ou commentaire).
/// Propose des motifs prédéfinis puis envoie `POST /api/reports`.
class ReportSheet {
  static const _reasons = [
    'Contenu inapproprié',
    'Spam ou publicité',
    'Harcèlement',
    'Fausse information',
    'Autre',
  ];

  static Future<void> show(
    BuildContext context,
    WidgetRef ref, {
    required String targetType,
    required String targetId,
    required String targetLabel,
  }) {
    return showModalBottomSheet<void>(
      context: context,
      backgroundColor: AppColors.surface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(20)),
      ),
      builder: (sheetContext) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Padding(
              padding: const EdgeInsets.fromLTRB(20, 20, 20, 8),
              child: Text(
                'Signaler $targetLabel',
                style: const TextStyle(
                  fontSize: 17,
                  fontWeight: FontWeight.w700,
                  color: AppColors.textDark,
                ),
              ),
            ),
            const Padding(
              padding: EdgeInsets.symmetric(horizontal: 20),
              child: Text(
                'Choisissez un motif. Notre équipe de modération examinera le contenu.',
                style: TextStyle(fontSize: 13, color: AppColors.textLight),
              ),
            ),
            const SizedBox(height: 8),
            ..._reasons.map(
              (reason) => ListTile(
                leading: const Icon(Iconsax.flag, color: AppColors.heart),
                title: Text(reason),
                onTap: () => _submit(
                  sheetContext,
                  ref,
                  targetType: targetType,
                  targetId: targetId,
                  reason: reason,
                ),
              ),
            ),
            const SizedBox(height: 8),
          ],
        ),
      ),
    );
  }

  static Future<void> _submit(
    BuildContext sheetContext,
    WidgetRef ref, {
    required String targetType,
    required String targetId,
    required String reason,
  }) async {
    final messenger = ScaffoldMessenger.of(sheetContext);
    Navigator.of(sheetContext).pop();
    try {
      if (!AppConfig.useMockData) {
        await ref.read(reportRemoteDataSourceProvider).report(
              targetType: targetType,
              targetId: targetId,
              reason: reason,
            );
      }
      messenger.showSnackBar(
        const SnackBar(
          content: Text('Merci, votre signalement a été transmis.'),
          backgroundColor: AppColors.primary,
        ),
      );
    } on Failure catch (f) {
      messenger.showSnackBar(
        SnackBar(content: Text(f.message), backgroundColor: AppColors.heart),
      );
    } catch (e) {
      messenger.showSnackBar(
        SnackBar(
            content: Text('Échec du signalement : $e'),
            backgroundColor: AppColors.heart),
      );
    }
  }
}
