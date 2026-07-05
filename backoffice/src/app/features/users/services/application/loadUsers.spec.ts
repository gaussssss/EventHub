import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { ToastStates } from '../../../../shared/services/infrastructure/states/toastStates';
import { UserService } from '../infrastructure/repository/user';
import { UserStates } from '../infrastructure/states/userStates';
import { LoadUsers } from './loadUsers';

function setup(serviceStub: Partial<UserService>) {
  TestBed.configureTestingModule({
    providers: [{ provide: UserService, useValue: serviceStub }],
  });
  return {
    load: TestBed.inject(LoadUsers),
    states: TestBed.inject(UserStates),
    toasts: TestBed.inject(ToastStates),
  };
}

describe('LoadUsers', () => {
  const sample = [
    { id: '1', name: 'A', email: 'a@x', role: 'student', status: 'active', totalHearts: 5 },
  ];

  it('charge les utilisateurs et coupe le chargement', () => {
    const search = jasmine.createSpy('search').and.returnValue(of(sample));
    const { load, states } = setup({ search });

    load.handler('alice');

    expect(search).toHaveBeenCalledWith('alice');
    expect(load.query()).toBe('alice');
    expect(load.isLoading()).toBeFalse();
    expect(states.users()).toEqual(sample);
  });

  it('signale une erreur via un toast et laisse la liste vide', () => {
    const { load, states, toasts } = setup({
      search: () => throwError(() => new Error('boom')),
    });

    load.handler();

    expect(load.isLoading()).toBeFalse();
    expect(states.users()).toEqual([]);
    expect(toasts.messages().length).toBe(1);
    expect(toasts.messages()[0].messageType).toBe('error');
  });
});
