import 'dart:io';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:iconsax_flutter/iconsax_flutter.dart';
import 'package:image_picker/image_picker.dart';
import '../../../../core/config/app_config.dart';
import '../../../../core/constants/app_colors.dart';
import '../../../../core/error/failure.dart';
import '../../../activities/presentation/providers/activity_provider.dart';
import '../providers/post_provider.dart';

class PublishPhotoPage extends ConsumerStatefulWidget {
  const PublishPhotoPage({super.key});

  @override
  ConsumerState<PublishPhotoPage> createState() => _PublishPhotoPageState();
}

class _PublishPhotoPageState extends ConsumerState<PublishPhotoPage> {
  final _captionController = TextEditingController();
  String? _selectedActivityId;
  XFile? _pickedImage;
  bool _submitting = false;

  @override
  void dispose() {
    _captionController.dispose();
    super.dispose();
  }

  bool get _canPublish =>
      _pickedImage != null &&
      _captionController.text.trim().isNotEmpty &&
      !_submitting;

  Future<void> _pickImage() async {
    try {
      final picked = await ImagePicker().pickImage(
        source: ImageSource.gallery,
        imageQuality: 85,
        maxWidth: 1600,
      );
      if (picked != null) setState(() => _pickedImage = picked);
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text('Impossible d\'ouvrir la galerie : $e'),
            backgroundColor: AppColors.heart),
      );
    }
  }

  Future<void> _publish() async {
    final image = _pickedImage;
    if (image == null) return;
    setState(() => _submitting = true);
    try {
      if (!AppConfig.useMockData) {
        // 1) upload de l'image choisie → URL servie par l'API.
        final url =
            await ref.read(uploadRemoteDataSourceProvider).uploadImage(image.path);
        // 2) création du post avec cette URL.
        await ref.read(postRemoteDataSourceProvider).createPost(
              imageUrl: url,
              caption: _captionController.text.trim(),
              activityId: _selectedActivityId,
            );
        ref.invalidate(allPostsProvider); // le fil se recharge → le post apparaît
      }
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Publication partagée avec la communauté !'),
          backgroundColor: AppColors.primary,
        ),
      );
      context.pop();
    } on Failure catch (f) {
      if (!mounted) return;
      setState(() => _submitting = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(f.message), backgroundColor: AppColors.heart),
      );
    } catch (e) {
      if (!mounted) return;
      setState(() => _submitting = false);
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
            content: Text('Échec de la publication : $e'),
            backgroundColor: AppColors.heart),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    // Liste pour le menu déroulant ; vide tant que le chargement n'est pas fini.
    final activities =
        ref.watch(allActivitiesProvider).valueOrNull ?? const [];

    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        backgroundColor: AppColors.surface,
        elevation: 0,
        leading: IconButton(
          icon: const Icon(Iconsax.close_circle),
          onPressed: () => context.pop(),
        ),
        title: const Text(
          'Nouvelle publication',
          style: TextStyle(fontWeight: FontWeight.w600),
        ),
        actions: [
          Padding(
            padding: const EdgeInsets.fromLTRB(0, 8, 12, 8),
            child: FilledButton.icon(
              onPressed: _canPublish ? _publish : null,
              icon: _submitting
                  ? const SizedBox(
                      width: 16,
                      height: 16,
                      child: CircularProgressIndicator(
                          strokeWidth: 2, color: Colors.white),
                    )
                  : const Icon(Iconsax.send_1, size: 18),
              label: Text(_submitting ? 'Publication…' : 'Publier'),
              style: FilledButton.styleFrom(
                backgroundColor: AppColors.primary,
                foregroundColor: Colors.white,
                disabledBackgroundColor: AppColors.divider,
                disabledForegroundColor: AppColors.textLight,
                elevation: 0,
                padding:
                    const EdgeInsets.symmetric(horizontal: 18, vertical: 12),
                textStyle: const TextStyle(
                    fontWeight: FontWeight.w700, fontSize: 14),
                shape: RoundedRectangleBorder(
                    borderRadius: BorderRadius.circular(24)),
              ),
            ),
          ),
        ],
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            GestureDetector(
              onTap: _pickImage,
              child: Container(
                width: double.infinity,
                height: 220,
                decoration: BoxDecoration(
                  color: AppColors.surface,
                  borderRadius: BorderRadius.circular(18),
                  border: Border.all(
                    color: _pickedImage != null
                        ? AppColors.primary
                        : AppColors.divider,
                    width: _pickedImage != null ? 2 : 1,
                  ),
                  boxShadow: const [
                    BoxShadow(
                      color: AppColors.cardShadow,
                      blurRadius: 8,
                      offset: Offset(0, 2),
                    ),
                  ],
                ),
                child: _pickedImage != null
                    ? Stack(
                        fit: StackFit.expand,
                        children: [
                          ClipRRect(
                            borderRadius: BorderRadius.circular(16),
                            child: Image.file(
                              File(_pickedImage!.path),
                              fit: BoxFit.cover,
                            ),
                          ),
                          Positioned(
                            top: 8,
                            right: 8,
                            child: GestureDetector(
                              onTap: () =>
                                  setState(() => _pickedImage = null),
                              child: Container(
                                padding: const EdgeInsets.all(4),
                                decoration: BoxDecoration(
                                  color:
                                      Colors.black.withValues(alpha: 0.5),
                                  shape: BoxShape.circle,
                                ),
                                child: const Icon(Iconsax.close_circle,
                                    color: Colors.white, size: 18),
                              ),
                            ),
                          ),
                        ],
                      )
                    : Column(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: [
                          Icon(
                            Iconsax.gallery_add,
                            size: 48,
                            color: AppColors.textLight,
                          ),
                          const SizedBox(height: 12),
                          const Text(
                            'Appuyer pour choisir une photo',
                            style: TextStyle(
                                color: AppColors.textLight, fontSize: 14),
                          ),
                        ],
                      ),
              ),
            ),
            const SizedBox(height: 16),
            Container(
              decoration: BoxDecoration(
                color: AppColors.surface,
                borderRadius: BorderRadius.circular(16),
                boxShadow: const [
                  BoxShadow(
                    color: AppColors.cardShadow,
                    blurRadius: 8,
                    offset: Offset(0, 2),
                  ),
                ],
              ),
              child: TextField(
                controller: _captionController,
                maxLines: 4,
                onChanged: (_) => setState(() {}),
                decoration: const InputDecoration(
                  hintText:
                      'Décrivez votre expérience, encouragez la communauté...',
                  border: InputBorder.none,
                  contentPadding: EdgeInsets.all(16),
                ),
              ),
            ),
            const SizedBox(height: 16),
            Container(
              decoration: BoxDecoration(
                color: AppColors.surface,
                borderRadius: BorderRadius.circular(16),
                boxShadow: const [
                  BoxShadow(
                    color: AppColors.cardShadow,
                    blurRadius: 8,
                    offset: Offset(0, 2),
                  ),
                ],
              ),
              padding: const EdgeInsets.symmetric(horizontal: 16),
              child: DropdownButtonHideUnderline(
                child: DropdownButton<String>(
                  value: _selectedActivityId,
                  isExpanded: true,
                  hint: const Text(
                    'Associer à une activité',
                    style: TextStyle(color: AppColors.textLight),
                  ),
                  icon: const Icon(Iconsax.arrow_down_1,
                      color: AppColors.textLight),
                  items: activities
                      .map(
                        (a) => DropdownMenuItem(
                          value: a.id,
                          child: Text(a.title,
                              style: const TextStyle(fontSize: 14)),
                        ),
                      )
                      .toList(),
                  onChanged: (v) =>
                      setState(() => _selectedActivityId = v),
                ),
              ),
            ),
          ],
        ),
      ),
    );
  }
}
