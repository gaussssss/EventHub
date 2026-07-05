import { Injectable, signal } from '@angular/core';
import { UserDto } from '../../../models/userDto';

/** Store signal de la liste des utilisateurs. Passif : signals + setters. */
@Injectable({ providedIn: 'root' })
export class UserStates {
  private readonly _users = signal<UserDto[]>([]);
  readonly users = this._users.asReadonly();

  setUsers(users: UserDto[]): void {
    this._users.set(users);
  }

  /** Remplace un utilisateur (après édition rôle/statut/cœurs). */
  upsert(user: UserDto): void {
    this._users.update((list) => list.map((u) => (u.id === user.id ? user : u)));
  }

  reset(): void {
    this._users.set([]);
  }
}
