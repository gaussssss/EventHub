import { Component, inject, OnInit } from '@angular/core';
import { ModalStates } from '../../../../shared/services/infrastructure/states/modalStates';
import { DeleteOrganizer } from '../../services/application/deleteOrganizer';
import { LoadOrganizers } from '../../services/application/loadOrganizers';
import { OrganizerStates } from '../../services/infrastructure/states/organizerStates';
import { OrganizerDto } from '../../models/organizerDto';
import { OrganizerFormModal } from '../modals/organizer-form/organizer-form';

/** Écran « Organisateurs » : liste + création / édition / suppression. */
@Component({
  selector: 'app-organizer-list',
  standalone: true,
  imports: [OrganizerFormModal],
  templateUrl: './organizer-list.html',
})
export class OrganizerList implements OnInit {
  readonly states = inject(OrganizerStates);
  readonly load = inject(LoadOrganizers);
  readonly del = inject(DeleteOrganizer);
  readonly modals = inject(ModalStates);

  ngOnInit(): void {
    this.load.handler();
  }

  openCreate(): void {
    this.modals.open('organizer-form', { id: null, name: '', contactEmail: '' });
  }

  openEdit(organizer: OrganizerDto): void {
    this.modals.open('organizer-form', {
      id: organizer.id,
      name: organizer.name,
      contactEmail: organizer.contactEmail ?? '',
    });
  }

  confirmDelete(organizer: OrganizerDto): void {
    if (window.confirm(`Supprimer l'organisateur « ${organizer.name} » ?`)) {
      this.del.handler(organizer.id);
    }
  }
}
