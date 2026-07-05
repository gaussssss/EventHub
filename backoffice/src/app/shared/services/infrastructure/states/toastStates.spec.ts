import { ToastStates } from './toastStates';

describe('ToastStates', () => {
  let toasts: ToastStates;

  beforeEach(() => {
    toasts = new ToastStates();
    jasmine.clock().install();
  });

  afterEach(() => jasmine.clock().uninstall());

  it('incrémente les ids des toasts', () => {
    toasts.success('un');
    toasts.error('deux');
    const ids = toasts.messages().map((t) => t.id);
    expect(ids).toEqual([1, 2]);
    expect(toasts.messages()[1].messageType).toBe('error');
  });

  it('retire un toast par id', () => {
    toasts.info('a');
    toasts.info('b');
    const [first] = toasts.messages();
    toasts.removeToast(first.id);
    expect(toasts.messages().length).toBe(1);
    expect(toasts.messages()[0].message).toBe('b');
  });

  it('auto-supprime le toast après le délai', () => {
    toasts.warning('éphémère');
    expect(toasts.messages().length).toBe(1);
    jasmine.clock().tick(5001);
    expect(toasts.messages().length).toBe(0);
  });
});
