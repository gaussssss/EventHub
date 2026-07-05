import { UserStates } from './userStates';
import { UserDto } from '../../../models/userDto';

function user(id: string, role = 'student'): UserDto {
  return { id, name: `U${id}`, email: `${id}@x`, role, status: 'active', totalHearts: 0 };
}

describe('UserStates', () => {
  let states: UserStates;

  beforeEach(() => (states = new UserStates()));

  it('remplace la liste avec setUsers', () => {
    states.setUsers([user('1'), user('2')]);
    expect(states.users().length).toBe(2);
  });

  it('met à jour un utilisateur par id (upsert)', () => {
    states.setUsers([user('1'), user('2')]);
    states.upsert(user('2', 'admin'));
    expect(states.users().find((u) => u.id === '2')?.role).toBe('admin');
    expect(states.users().length).toBe(2);
  });

  it('réinitialise la liste', () => {
    states.setUsers([user('1')]);
    states.reset();
    expect(states.users()).toEqual([]);
  });
});
