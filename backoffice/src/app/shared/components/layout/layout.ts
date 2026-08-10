import { Component, inject } from '@angular/core';
import { RouterModule, RouterOutlet } from '@angular/router';
import { Navbar } from '../navbar/navbar';
import { ToastList } from '../toast-list/toast-list';
import { UiStates } from '../../services/infrastructure/states/uiStates';

/**
 * Coquille de l'admin : barre supérieure (navbar) + rail de navigation gauche +
 * zone de contenu (`router-outlet`), façon portail Azure. Les toasts sont rendus
 * globalement ici.
 */
@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterModule, RouterOutlet, Navbar, ToastList],
  templateUrl: './layout.html',
})
export class Layout {
  readonly ui = inject(UiStates);

  /** Entrées du rail. `icon` = classe Iconify Fluent. `enabled: false` = à venir. */
  readonly nav = [
    { label: 'Tableau de bord', path: '/dashboard', icon: 'icon-[fluent--board-24-regular]', enabled: true },
    { label: 'Utilisateurs', path: '/users', icon: 'icon-[fluent--people-24-regular]', enabled: true },
    { label: 'Classement', path: '/leaderboard', icon: 'icon-[fluent--trophy-24-regular]', enabled: true },
    { label: 'Activités', path: '/activities', icon: 'icon-[fluent--calendar-ltr-24-regular]', enabled: true },
    { label: 'Modération', path: '/moderation', icon: 'icon-[fluent--shield-24-regular]', enabled: true },
    { label: 'Catégories', path: '/categories', icon: 'icon-[fluent--tag-24-regular]', enabled: true },
    { label: 'Organisateurs', path: '/organizers', icon: 'icon-[fluent--people-team-24-regular]', enabled: true },
    // Notifications : section masquée à la demande du client (la route et
    // l'API existent toujours ; ré-ajouter l'entrée ici pour la réactiver).
    { label: 'Paramètres', path: '/settings', icon: 'icon-[fluent--settings-24-regular]', enabled: true },
  ];
}
