/** Ligne de la liste des utilisateurs (GET /api/admin/users). */
export interface UserDto {
  id: string;
  name: string;
  email: string;
  role: string;
  status: string;
  totalHearts: number;
}

/** Rôles applicatifs valides côté API (UserRoles.Normalize). */
export const USER_ROLES = ['student', 'organizer', 'moderator', 'admin'] as const;

/** Statuts de compte valides côté API (UpdateUserHandler.AllowedStatuses). */
export const USER_STATUSES = ['active', 'suspended', 'deleted'] as const;
