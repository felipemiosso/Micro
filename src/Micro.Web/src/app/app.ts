import { Component, signal, inject } from '@angular/core';
import { RouterOutlet, RouterModule, Router } from '@angular/router';
import { AuthService } from './core/auth/auth';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('Micro.Web');
  public authService = inject(AuthService);
  private router = inject(Router);
  public userMenuOpen = signal(false);

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  toggleUserMenu() {
    this.userMenuOpen.update(v => !v);
  }
}
