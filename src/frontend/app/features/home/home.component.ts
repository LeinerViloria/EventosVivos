import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoModule } from '@jsverse/transloco';
import { AuthStore } from '@core/auth/auth-store';

@Component({
  selector: 'app-home',
  imports: [RouterLink, TranslocoModule],
  templateUrl: './home.component.html',
})
export class HomeComponent {
  protected readonly auth = inject(AuthStore);
}
