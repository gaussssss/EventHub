import { ModalStates } from './modalStates';

describe('ModalStates', () => {
  let modals: ModalStates;

  beforeEach(() => (modals = new ModalStates()));

  it('ouvre une modale et expose ses données typées', () => {
    modals.open('organizer-form', { id: null, name: 'AGE', contactEmail: 'age@x' });
    expect(modals.isOpen('organizer-form')).toBeTrue();
    expect(modals.getData('organizer-form')?.name).toBe('AGE');
  });

  it('ferme une modale', () => {
    modals.open('activity-form', { id: '1' });
    modals.close('activity-form');
    expect(modals.isOpen('activity-form')).toBeFalse();
    expect(modals.getData('activity-form')).toBeUndefined();
  });

  it('gère plusieurs modales indépendamment', () => {
    modals.open('activity-form', { id: '1' });
    modals.open('award-hearts', { id: 'u1', name: 'X', totalHearts: 10 });
    expect(modals.isOpen('activity-form')).toBeTrue();
    expect(modals.isOpen('award-hearts')).toBeTrue();
    modals.close('activity-form');
    expect(modals.isOpen('award-hearts')).toBeTrue();
  });
});
