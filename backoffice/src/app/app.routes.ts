import { Routes } from '@angular/router';
import { auth } from './core/guards/auth';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'dashboard',
    pathMatch: 'full',
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/components/login/login').then((m) => m.Login),
  },
  {
    path: '',
    canActivate: [auth],
    loadComponent: () =>
      import('./shared/components/layout/layout').then((m) => m.Layout),
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/components/dashboard/dashboard').then((m) => m.Dashboard),
      },
      {
        path: 'activities',
        loadComponent: () =>
          import('./features/activities/components/activity-list/activity-list').then(
            (m) => m.ActivityList,
          ),
      },
      {
        path: 'activities/:id/registrations',
        loadComponent: () =>
          import(
            './features/activities/components/activity-registrations/activity-registrations'
          ).then((m) => m.ActivityRegistrations),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/users/components/user-list/user-list').then((m) => m.UserList),
      },
      {
        path: 'leaderboard',
        loadComponent: () =>
          import('./features/leaderboard/components/leaderboard/leaderboard').then(
            (m) => m.Leaderboard,
          ),
      },
      {
        path: 'moderation',
        loadComponent: () =>
          import('./features/moderation/components/report-list/report-list').then(
            (m) => m.ReportList,
          ),
      },
      {
        path: 'categories',
        loadComponent: () =>
          import('./features/categories/components/category-list/category-list').then(
            (m) => m.CategoryList,
          ),
      },
      {
        path: 'organizers',
        loadComponent: () =>
          import('./features/organizers/components/organizer-list/organizer-list').then(
            (m) => m.OrganizerList,
          ),
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import(
            './features/notifications/components/notification-broadcast/notification-broadcast'
          ).then((m) => m.NotificationBroadcast),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/components/settings/settings').then((m) => m.Settings),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'dashboard',
  },
];
