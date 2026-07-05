import '../../domain/entities/post.dart';

class PostLocalDataSource {
  List<Post> get posts => _posts;

  static final List<Post> _posts = [
    Post(
      id: 'post001',
      authorName: 'Marie Tremblay',
      authorAvatarUrl:
          'https://images.unsplash.com/photo-1494790108755-2616b612b74c?w=150&fit=crop&q=80',
      imageUrl:
          'https://images.unsplash.com/photo-1552674605-db6ffd4facb5?w=800&fit=crop&q=80',
      caption:
          'Super session de course ce matin ! Merci à tous pour l\'énergie. On recommence la semaine prochaine !',
      activityName: 'Course en forêt',
      createdAt: DateTime(2026, 6, 3, 9, 15),
      likesCount: 24,
      comments: [
        PostComment(
          authorName: 'Jean Lapointe',
          text: 'C\'était génial ! J\'ai battu mon record personnel.',
          createdAt: DateTime(2026, 6, 3, 10, 0),
        ),
        PostComment(
          authorName: 'Sophie Martin',
          text: 'Hâte à la prochaine édition !',
          createdAt: DateTime(2026, 6, 3, 11, 30),
        ),
      ],
    ),
    Post(
      id: 'post002',
      authorName: 'Jean Lapointe',
      authorAvatarUrl:
          'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&fit=crop&q=80',
      imageUrl:
          'https://images.unsplash.com/photo-1544367567-0f2fcb009e0b?w=800&fit=crop&q=80',
      caption:
          'Yoga matinal au lever du soleil. Rien de mieux pour démarrer la journée avec sérénité.',
      activityName: 'Yoga matinal',
      createdAt: DateTime(2026, 6, 2, 8, 0),
      likesCount: 31,
      comments: [
        PostComment(
          authorName: 'Alex Tremblay',
          text: 'La vue depuis le gymnase était magnifique ce matin !',
          createdAt: DateTime(2026, 6, 2, 8, 45),
        ),
      ],
    ),
    Post(
      id: 'post003',
      authorName: 'Sophie Martin',
      authorAvatarUrl:
          'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150&fit=crop&q=80',
      imageUrl:
          'https://images.unsplash.com/photo-1546519638-68e109498ffc?w=800&fit=crop&q=80',
      caption:
          'Notre équipe en finale du tournoi ! On s\'est battus jusqu\'au bout. Fiers de notre parcours !',
      activityName: 'Tournoi de basketball',
      createdAt: DateTime(2026, 6, 1, 17, 30),
      likesCount: 47,
      comments: [
        PostComment(
          authorName: 'Marie Tremblay',
          text: 'Bravo à toute l\'équipe, vous avez assuré !',
          createdAt: DateTime(2026, 6, 1, 18, 0),
        ),
        PostComment(
          authorName: 'Camille Roy',
          text: 'Quelle ambiance incroyable dans le gym !',
          createdAt: DateTime(2026, 6, 1, 19, 15),
        ),
      ],
    ),
    Post(
      id: 'post004',
      authorName: 'Alex Tremblay',
      authorAvatarUrl:
          'https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&fit=crop&q=80',
      imageUrl:
          'https://images.unsplash.com/photo-1565193566173-7a0ee3dbe261?w=800&fit=crop&q=80',
      caption:
          'Ma première création en poterie ! Je suis tellement fier de ce bol. Merci à l\'instructrice pour sa patience.',
      activityName: 'Atelier de poterie',
      createdAt: DateTime(2026, 5, 29, 15, 0),
      likesCount: 38,
      comments: [
        PostComment(
          authorName: 'Jean Lapointe',
          text: 'C\'est magnifique ! Tu as du talent.',
          createdAt: DateTime(2026, 5, 29, 15, 45),
        ),
      ],
    ),
    Post(
      id: 'post005',
      authorName: 'Camille Roy',
      authorAvatarUrl:
          'https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&fit=crop&q=80',
      imageUrl:
          'https://images.unsplash.com/photo-1513364776144-60967b0f800f?w=800&fit=crop&q=80',
      caption:
          'Mon tableau terminé ! J\'ai adoré l\'atelier de peinture. C\'est thérapeutique de se retrouver face à une toile blanche.',
      activityName: 'Atelier de peinture',
      createdAt: DateTime(2026, 5, 27, 16, 30),
      likesCount: 52,
      comments: [
        PostComment(
          authorName: 'Sophie Martin',
          text: 'Les couleurs sont superbes ! Tu l\'accroches où ?',
          createdAt: DateTime(2026, 5, 27, 17, 0),
        ),
        PostComment(
          authorName: 'Marie Tremblay',
          text: 'C\'est vraiment beau, tu devrais participer à l\'expo des arts !',
          createdAt: DateTime(2026, 5, 27, 18, 30),
        ),
      ],
    ),
    Post(
      id: 'post006',
      authorName: 'Sophie Martin',
      authorAvatarUrl:
          'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=150&fit=crop&q=80',
      imageUrl:
          'https://images.unsplash.com/photo-1540575467063-178a50c2df87?w=800&fit=crop&q=80',
      caption:
          'Conférence santé mentale très enrichissante. Des conseils pratiques que j\'applique déjà au quotidien.',
      activityName: 'Conférence : Santé mentale',
      createdAt: DateTime(2026, 5, 25, 18, 0),
      likesCount: 29,
      comments: [],
    ),
  ];
}
