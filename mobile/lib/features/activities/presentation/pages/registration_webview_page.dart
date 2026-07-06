import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import 'package:webview_flutter/webview_flutter.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/utils/platform_icons.dart';
import '../providers/activity_provider.dart';

class RegistrationWebViewPage extends ConsumerStatefulWidget {
  final String activityId;
  final String url;

  const RegistrationWebViewPage({
    super.key,
    required this.activityId,
    required this.url,
  });

  @override
  ConsumerState<RegistrationWebViewPage> createState() =>
      _RegistrationWebViewPageState();
}

class _RegistrationWebViewPageState
    extends ConsumerState<RegistrationWebViewPage> {
  late final WebViewController _controller;
  bool _isLoading = true;
  bool _formSubmitted = false;

  @override
  void initState() {
    super.initState();
    _controller = WebViewController()
      ..setJavaScriptMode(JavaScriptMode.unrestricted)
      ..setNavigationDelegate(
        NavigationDelegate(
          onPageStarted: (_) => setState(() => _isLoading = true),
          onPageFinished: (url) {
            setState(() => _isLoading = false);
            // Google Forms shows "formResponse" in the URL after submission
            if (url.contains('formResponse') ||
                url.contains('viewform?embedded=true&usp=pp_url')) {
              setState(() => _formSubmitted = true);
            }
          },
          onUrlChange: (change) {
            final url = change.url ?? '';
            if (url.contains('formResponse')) {
              setState(() => _formSubmitted = true);
            }
          },
        ),
      )
      ..loadRequest(Uri.parse(widget.url));
  }

  void _confirmRegistration() {
    ref.read(registeredActivitiesProvider.notifier).register(widget.activityId);
    context.pushReplacement(
        '/activity/${widget.activityId}/confirmation');
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.surface,
        elevation: 0,
        leading: IconButton(
          icon: Icon(PlatformIcons.back, color: AppColors.textDark),
          onPressed: () => context.pop(),
        ),
        title: const Text(
          'Formulaire d\'inscription',
          style: TextStyle(fontSize: 16, fontWeight: FontWeight.w600),
        ),
        actions: [
          if (_isLoading)
            const Padding(
              padding: EdgeInsets.all(14),
              child: SizedBox(
                width: 18,
                height: 18,
                child: CircularProgressIndicator(
                  strokeWidth: 2,
                  color: AppColors.primary,
                ),
              ),
            ),
          const SizedBox(width: 4),
        ],
      ),
      body: Column(
        children: [
          if (_formSubmitted)
            Container(
              width: double.infinity,
              padding:
                  const EdgeInsets.symmetric(horizontal: 16, vertical: 10),
              color: AppColors.primary.withValues(alpha: 0.1),
              child: Row(
                children: [
                  const Icon(Iconsax.tick_circle,
                      color: AppColors.primary, size: 18),
                  const SizedBox(width: 8),
                  const Expanded(
                    child: Text(
                      'Formulaire soumis ! Confirmez votre inscription ci-dessous.',
                      style: TextStyle(
                        fontSize: 13,
                        color: AppColors.primary,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          Expanded(child: WebViewWidget(controller: _controller)),
          Container(
            padding: const EdgeInsets.fromLTRB(20, 12, 20, 28),
            decoration: const BoxDecoration(
              color: AppColors.surface,
              boxShadow: [
                BoxShadow(
                  color: Color(0x1A000000),
                  blurRadius: 16,
                  offset: Offset(0, -4),
                ),
              ],
            ),
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                SizedBox(
                  width: double.infinity,
                  height: 54,
                  child: ElevatedButton(
                    onPressed: _confirmRegistration,
                    style: ElevatedButton.styleFrom(
                      backgroundColor: _formSubmitted
                          ? AppColors.primary
                          : AppColors.primary.withValues(alpha: 0.5),
                      foregroundColor: Colors.white,
                      elevation: 0,
                      shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(14)),
                    ),
                    child: const Text(
                      'J\'ai soumis le formulaire',
                      style: TextStyle(
                          fontSize: 16, fontWeight: FontWeight.w600),
                    ),
                  ),
                ),
                const SizedBox(height: 8),
                TextButton(
                  onPressed: () => context.pop(),
                  child: const Text(
                    'Annuler',
                    style: TextStyle(
                        color: AppColors.textLight, fontSize: 14),
                  ),
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}
