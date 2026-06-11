import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslocoModule } from '@jsverse/transloco';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, ButtonModule, TranslocoModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly router = inject(Router);

  protected goHome(): void {
    this.router.navigate(['/']);
  }
}
