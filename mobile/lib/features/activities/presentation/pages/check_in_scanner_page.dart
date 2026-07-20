import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import 'package:mobile_scanner/mobile_scanner.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/platform_icons.dart';
import '../../../profile/presentation/providers/profile_provider.dart';
import '../providers/activity_provider.dart';

/// Scanner d'émargement : l'étudiant scanne le QR affiché par l'organisateur
/// (payload `uqtrsante://checkin?a=<activityId>&k=<token>`). Le serveur valide
/// (inscrit + fenêtre horaire + jeton) puis confirme la présence et crédite les
/// cœurs. Un seul scan est traité à la fois ; le résultat s'affiche en overlay.
class CheckInScannerPage extends ConsumerStatefulWidget {
  const CheckInScannerPage({super.key});

  @override
  ConsumerState<CheckInScannerPage> createState() =>
      _CheckInScannerPageState();
}

class _CheckInScannerPageState extends ConsumerState<CheckInScannerPage> {
  final MobileScannerController _controller = MobileScannerController(
    formats: const [BarcodeFormat.qrCode],
  );
  bool _processing = false;

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  Future<void> _onDetect(BarcodeCapture capture) async {
    if (_processing) return;
    final raw = capture.barcodes.firstOrNull?.rawValue;
    if (raw == null) return;

    final uri = Uri.tryParse(raw);
    final isCheckIn = uri != null &&
        uri.scheme == 'uqtrsante' &&
        uri.host == 'checkin' &&
        (uri.queryParameters['a']?.isNotEmpty ?? false) &&
        (uri.queryParameters['k']?.isNotEmpty ?? false);
    if (!isCheckIn) {
      _showResult(
        icon: Iconsax.warning_2,
        color: AppColors.secondary,
        title: 'QR non reconnu',
        message: 'Ce code n\'est pas un QR d\'émargement UQTR en santé.',
        retry: true,
      );
      return;
    }

    setState(() => _processing = true);
    await _controller.stop();

    try {
      final result = await ref
          .read(activityRemoteDataSourceProvider)
          .checkIn(uri.queryParameters['a']!, uri.queryParameters['k']!);

      switch (result['status'] as String?) {
        case 'ok':
          final hearts = (result['heartsAwarded'] ?? 0) as int;
          final already = (result['alreadyCheckedIn'] ?? false) as bool;
          // La présence (statut) et le solde de cœurs ont changé côté serveur.
          ref.invalidate(myRegistrationsProvider);
          ref.invalidate(currentUserProvider);
          _showResult(
            icon: Iconsax.tick_circle,
            color: AppColors.primary,
            title: already ? 'Déjà pointé !' : 'Présence confirmée !',
            message: already
                ? 'Votre présence avait déjà été enregistrée.'
                : hearts > 0
                    ? 'Vous gagnez +$hearts cœurs santé. Bravo !'
                    : 'Votre présence est enregistrée.',
          );
        case 'invalidToken':
          _showResult(
            icon: Iconsax.close_circle,
            color: AppColors.heart,
            title: 'QR invalide',
            message:
                'Ce code ne correspond pas à cet événement. Demandez le QR du jour à l\'organisateur.',
            retry: true,
          );
        case 'notRegistered':
          _showResult(
            icon: Iconsax.user_remove,
            color: AppColors.heart,
            title: 'Non inscrit(e)',
            message:
                'Vous n\'êtes pas inscrit(e) à cet événement. Inscrivez-vous d\'abord depuis sa fiche.',
          );
        case 'outsideWindow':
          _showResult(
            icon: Iconsax.clock,
            color: AppColors.secondary,
            title: 'Émargement fermé',
            message:
                'Le pointage n\'est ouvert qu\'autour de l\'horaire de l\'événement.',
          );
        default:
          _showResult(
            icon: Iconsax.warning_2,
            color: AppColors.secondary,
            title: 'Réponse inattendue',
            message: 'Réessayez dans un instant.',
            retry: true,
          );
      }
    } catch (e) {
      _showResult(
        icon: Iconsax.wifi_square,
        color: AppColors.secondary,
        title: 'Échec du pointage',
        message: '$e',
        retry: true,
      );
    }
  }

  void _showResult({
    required IconData icon,
    required Color color,
    required String title,
    required String message,
    bool retry = false,
  }) {
    if (!mounted) return;
    showModalBottomSheet<void>(
      context: context,
      isDismissible: false,
      enableDrag: false,
      backgroundColor: AppColors.surface,
      shape: const RoundedRectangleBorder(
        borderRadius: BorderRadius.vertical(top: Radius.circular(24)),
      ),
      builder: (sheetContext) => SafeArea(
        child: Padding(
          padding: const EdgeInsets.fromLTRB(24, 24, 24, 24),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Container(
                width: 84,
                height: 84,
                decoration: BoxDecoration(
                  color: color.withValues(alpha: 0.12),
                  shape: BoxShape.circle,
                ),
                child: Icon(icon, color: color, size: 44),
              ),
              const SizedBox(height: 16),
              Text(
                title,
                style: const TextStyle(
                  fontSize: 20,
                  fontWeight: FontWeight.w700,
                  color: AppColors.textDark,
                ),
              ),
              const SizedBox(height: 8),
              Text(
                message,
                textAlign: TextAlign.center,
                style: const TextStyle(
                    fontSize: 14, color: AppColors.textMedium, height: 1.4),
              ),
              const SizedBox(height: 20),
              SizedBox(
                width: double.infinity,
                height: 52,
                child: ElevatedButton(
                  onPressed: () {
                    Navigator.of(sheetContext).pop();
                    if (retry) {
                      setState(() => _processing = false);
                      _controller.start();
                    } else {
                      context.pop();
                    }
                  },
                  style: ElevatedButton.styleFrom(
                    backgroundColor: color,
                    foregroundColor: Colors.white,
                    elevation: 0,
                    shape: RoundedRectangleBorder(
                        borderRadius: BorderRadius.circular(14)),
                  ),
                  child: Text(retry ? 'Rescanner' : 'Terminer',
                      style: const TextStyle(
                          fontSize: 16, fontWeight: FontWeight.w600)),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.black,
      appBar: AppBar(
        backgroundColor: Colors.black,
        foregroundColor: Colors.white,
        leading: IconButton(
          icon: Icon(PlatformIcons.back, color: Colors.white),
          onPressed: () => context.pop(),
        ),
        title: const Text('Scanner ma présence',
            style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600)),
      ),
      body: Stack(
        children: [
          MobileScanner(controller: _controller, onDetect: _onDetect),
          // Cadre de visée.
          Center(
            child: Container(
              width: 250,
              height: 250,
              decoration: BoxDecoration(
                border: Border.all(color: Colors.white70, width: 3),
                borderRadius: BorderRadius.circular(24),
              ),
            ),
          ),
          Positioned(
            left: 32,
            right: 32,
            bottom: 48,
            child: Text(
              'Visez le QR affiché par l\'organisateur pour confirmer votre présence.',
              textAlign: TextAlign.center,
              style: TextStyle(
                color: Colors.white.withValues(alpha: 0.85),
                fontSize: 14,
                height: 1.4,
              ),
            ),
          ),
          if (_processing)
            const Center(
                child: CircularProgressIndicator(color: Colors.white)),
        ],
      ),
    );
  }
}
