import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { SignalFormControl } from '@angular/forms/signals/compat';
import { email, required } from '@angular/forms/signals';
import { AuthStore } from '@core/auth/auth-store';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { FluidModule } from 'primeng/fluid';
import { MessageService } from 'primeng/api';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { EventsStore } from '@features/events/events-store';
import { FieldComponent } from '@shared/components/field/field.component';
import { EventListItem } from '@shared/models/event';
import { AppError } from '@shared/models/app-error';

@Component({
  selector: 'app-reserve-dialog',
  imports: [
    ReactiveFormsModule,
    DialogModule,
    InputTextModule,
    InputNumberModule,
    ButtonModule,
    ToastModule,
    FluidModule,
    TranslocoModule,
    FieldComponent,
  ],
  providers: [MessageService],
  templateUrl: './reserve-dialog.component.html',
})
export class ReserveDialogComponent {
  private readonly store = inject(EventsStore);
  private readonly messages = inject(MessageService);
  private readonly transloco = inject(TranslocoService);
  private readonly auth = inject(AuthStore);

  readonly event = input<EventListItem | null>(null);
  readonly closed = output<void>();
  readonly reserved = output<void>();

  protected readonly submitted = signal(false);
  protected readonly submitting = signal(false);

  protected readonly form = new FormGroup({
    buyerName: new SignalFormControl<string>('', (path) => required(path)),
    buyerEmail: new SignalFormControl<string>('', (path) => {
      required(path);
      email(path);
    }),
    quantity: new SignalFormControl<number>(1, (path) => required(path)),
  });

  constructor() {
    // When the dialog opens, default the buyer to the signed-in user (still editable, so the
    // user can reserve for someone else).
    effect(() => {
      if (this.event() === null) {
        return;
      }

      const user = this.auth.user();
      if (user) {
        this.form.patchValue({ buyerName: user.name, buyerEmail: user.email });
      }
    });
  }

  protected showError(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && (control.touched || this.submitted());
  }

  protected onHide(): void {
    this.resetForm();
    this.closed.emit();
  }

  protected submit(): void {
    const event = this.event();
    if (!event) {
      return;
    }

    this.submitted.set(true);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { buyerName, buyerEmail, quantity } = this.form.getRawValue();
    this.submitting.set(true);
    this.store.createReservation({ eventId: event.id, buyerName, buyerEmail, quantity }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.messages.add({
          severity: 'success',
          detail: this.transloco.translate('labels.reserve.success'),
        });
        this.reserved.emit();
        this.resetForm();
      },
      error: (error: AppError) => {
        this.submitting.set(false);
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

  private resetForm(): void {
    this.submitted.set(false);
    this.form.setValue({ buyerName: '', buyerEmail: '', quantity: 1 });
    this.form.markAsPristine();
    this.form.markAsUntouched();
  }
}
