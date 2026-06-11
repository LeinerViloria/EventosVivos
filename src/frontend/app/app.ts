import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslocoModule } from '@jsverse/transloco';
import { AuthStore } from '@core/auth/auth-store';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, ButtonModule, TranslocoModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly router = inject(Router);

  protected readonly auth = inject(AuthStore);

  protected goHome(): void {
    this.router.navigate(['/']);
  }

  protected logout(): void {
    this.auth.logout();
  }
}
