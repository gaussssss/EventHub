import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/services/theme';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet />`,
})
export class App {
  // Instancié dès le démarrage pour appliquer le thème (clair/sombre) avant tout écran.
  private readonly theme = inject(ThemeService);
}
