import { ThemeService } from './theme';

describe('ThemeService', () => {
  afterEach(() => {
    localStorage.removeItem('eventhub-theme');
    document.documentElement.removeAttribute('data-theme');
  });

  it('restaure le thème sombre depuis localStorage', () => {
    localStorage.setItem('eventhub-theme', 'dark');
    const svc = new ThemeService();
    expect(svc.isDark()).toBeTrue();
    expect(document.documentElement.getAttribute('data-theme')).toBe('azure-dark');
  });

  it('applique le thème clair par défaut (préférence light)', () => {
    localStorage.setItem('eventhub-theme', 'light');
    const svc = new ThemeService();
    expect(svc.isDark()).toBeFalse();
    expect(document.documentElement.getAttribute('data-theme')).toBe('azure');
  });

  it('bascule le thème, applique data-theme et persiste le choix', () => {
    localStorage.setItem('eventhub-theme', 'light');
    const svc = new ThemeService();

    svc.toggle();

    expect(svc.isDark()).toBeTrue();
    expect(document.documentElement.getAttribute('data-theme')).toBe('azure-dark');
    expect(localStorage.getItem('eventhub-theme')).toBe('dark');

    svc.toggle();

    expect(svc.isDark()).toBeFalse();
    expect(document.documentElement.getAttribute('data-theme')).toBe('azure');
    expect(localStorage.getItem('eventhub-theme')).toBe('light');
  });
});
