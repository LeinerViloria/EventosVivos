import { Component, inject, signal } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { SignalFormControl } from '@angular/forms/signals/compat';
import { maxLength, minLength, required } from '@angular/forms/signals';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TextareaModule } from 'primeng/textarea';
import { InputNumberModule } from 'primeng/inputnumber';
import { SelectModule } from 'primeng/select';
import { DatePickerModule } from 'primeng/datepicker';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { MessageService } from 'primeng/api';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { EventsStore } from '@features/events/events-store';
import { EventType } from '@shared/enums/event-type';
import { AppError } from '@shared/models/app-error';
import { CreateEventRequest } from '@shared/models/event';

@Component({
  selector: 'app-create-event',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    InputNumberModule,
    SelectModule,
    DatePickerModule,
    MessageModule,
    ToastModule,
    TranslocoModule,
  ],
  providers: [MessageService],
  templateUrl: './create-event.component.html',
})
export class CreateEventComponent {
  private readonly store = inject(EventsStore);
  private readonly messages = inject(MessageService);
  private readonly transloco = inject(TranslocoService);

  protected readonly venues = this.store.venues;
  protected readonly typeOptions = [
    EventType.Conference,
    EventType.Workshop,
    EventType.Concert,
  ].map((value) => ({ value, label: this.transloco.translate(`enums.eventType.${value}`) }));

  protected readonly submitted = signal(false);
  protected readonly submitting = signal(false);

  protected readonly form = new FormGroup({
    title: new SignalFormControl<string>('', (path) => {
      required(path);
      minLength(path, 5);
      maxLength(path, 100);
    }),
    description: new SignalFormControl<string>('', (path) => {
      required(path);
      minLength(path, 10);
      maxLength(path, 500);
    }),
    venueId: new SignalFormControl<string | null>(null, (path) => required(path)),
    maxCapacity: new SignalFormControl<number | null>(null, (path) => required(path)),
    startsAt: new SignalFormControl<Date | null>(null, (path) => required(path)),
    endsAt: new SignalFormControl<Date | null>(null, (path) => required(path)),
    price: new SignalFormControl<number | null>(null, (path) => required(path)),
    eventType: new SignalFormControl<EventType | null>(null, (path) => required(path)),
  });

  protected submit(): void {
    this.submitted.set(true);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: CreateEventRequest = {
      title: value.title.trim(),
      description: value.description.trim(),
      venueId: value.venueId!,
      maxCapacity: value.maxCapacity!,
      startsAt: this.toOffsetIso(value.startsAt!),
      endsAt: this.toOffsetIso(value.endsAt!),
      price: value.price!,
      type: value.eventType!,
    };

    this.submitting.set(true);
    this.store.createEvent(request).subscribe({
      next: () => {
        this.submitting.set(false);
        this.submitted.set(false);
        this.form.reset();
        this.messages.add({
          severity: 'success',
          detail: this.transloco.translate('labels.event.created'),
        });
      },
      error: (error: AppError) => {
        this.submitting.set(false);
        this.messages.add({
          severity: 'error',
          detail: this.transloco.translate(`errors.${error.errorCode}`, error.params ?? undefined),
        });
      },
    });
  }

  /** Converts a local Date to an ISO 8601 string keeping the client's offset. */
  private toOffsetIso(date: Date): string {
    const offsetMinutes = -date.getTimezoneOffset();
    const sign = offsetMinutes >= 0 ? '+' : '-';
    const abs = Math.abs(offsetMinutes);
    const pad = (n: number) => String(n).padStart(2, '0');

    return (
      `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}` +
      `T${pad(date.getHours())}:${pad(date.getMinutes())}:00` +
      `${sign}${pad(Math.floor(abs / 60))}:${pad(abs % 60)}`
    );
  }
}
