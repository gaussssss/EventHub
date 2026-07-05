import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ModalStates } from '../../../../shared/services/infrastructure/states/modalStates';
import { UpdateUser } from '../../services/application/updateUser';
import { LoadUsers } from '../../services/application/loadUsers';
import { UserStates } from '../../services/infrastructure/states/userStates';
import { USER_ROLES, USER_STATUSES, UserDto } from '../../models/userDto';
import { AwardHeartsModal } from '../modals/award-hearts/award-hearts';

/** Écran « Utilisateurs & rôles » : recherche + tableau éditable. */
@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [FormsModule, AwardHeartsModal],
  templateUrl: './user-list.html',
})
export class UserList implements OnInit {
  readonly states = inject(UserStates);
  readonly load = inject(LoadUsers);
  readonly update = inject(UpdateUser);
  readonly modals = inject(ModalStates);

  readonly roles = USER_ROLES;
  readonly statuses = USER_STATUSES;

  ngOnInit(): void {
    this.load.handler();
  }

  search(): void {
    this.load.handler(this.load.query());
  }

  changeRole(user: UserDto, role: string): void {
    if (role && role !== user.role) this.update.handler(user.id, { role });
  }

  changeStatus(user: UserDto, status: string): void {
    if (status && status !== user.status) this.update.handler(user.id, { status });
  }

  openHearts(user: UserDto): void {
    this.modals.open('award-hearts', {
      id: user.id,
      name: user.name,
      totalHearts: user.totalHearts,
    });
  }

  statusBadge(status: string): string {
    const map: Record<string, string> = {
      active: 'badge-success',
      suspended: 'badge-warning',
      deleted: 'badge-error',
    };
    return map[status] ?? 'badge-ghost';
  }
}
