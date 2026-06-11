import { Component, inject } from '@angular/core';
import { TranslocoModule } from '@jsverse/transloco';
import { EventsStore } from '@features/events/events-store';

@Component({
  selector: 'app-create-event',
  imports: [TranslocoModule],
  templateUrl: './create-event.component.html',
})
export class CreateEventComponent {
  protected readonly store = inject(EventsStore);
}
