import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
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
import { FluidModule } from 'primeng/fluid';
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
    FluidModule,
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
  private readonly router = inject(Router);

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

  /** Whether a field's error should be revealed (once touched or after a submit attempt). */
  protected showError(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && (control.touched || this.submitted());
  }

  protected cancel(): void {
    this.router.navigate(['/']);
  }

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
        this.resetForm();
        this.messages.add({
          severity: 'success',
          detail: this.transloco.translate('labels.event.created'),
        });
      },
      error: (error: AppError) => {
        this.submitting.set(false);

        // A validation failure (422) carries one entry per field in `validationErrors`;
        // any other failure carries a single top-level `errorCode`. Either way the backend
        // only sends codes, and i18n turns each code (with its params) into the shown text.
        const codes = error.validationErrors?.length
          ? error.validationErrors.map((fieldError) => ({
              code: fieldError.errorCode,
              params: fieldError.params,
            }))
          : [{ code: error.errorCode, params: error.params }];

        for (const { code, params } of codes) {
          this.messages.add({
            severity: 'error',
            detail: this.transloco.translate(`errors.${code}`, params ?? undefined),
          });
        }
      },
    });
  }

  /**
   * Clears the form after a successful creation so the user can register another event.
   * `setValue` (rather than `reset`) is used because it reliably propagates the empty values
   * to the bound PrimeNG controls through the compat bridge; marking it pristine/untouched
   * then hides the validation state.
   */
  private resetForm(): void {
    this.submitted.set(false);
    this.form.setValue({
      title: '',
      description: '',
      venueId: null,
      maxCapacity: null,
      startsAt: null,
      endsAt: null,
      price: null,
      eventType: null,
    });
    this.form.markAsPristine();
    this.form.markAsUntouched();
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
