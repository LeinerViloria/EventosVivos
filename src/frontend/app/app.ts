import { Component, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { TranslocoModule } from '@jsverse/transloco';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ButtonModule, TranslocoModule],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {
  private readonly router = inject(Router);

  protected goToCreateEvent(): void {
    this.router.navigate(['/events/create']);
  }
}
