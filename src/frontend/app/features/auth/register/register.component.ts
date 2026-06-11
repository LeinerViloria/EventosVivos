import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { SignalFormControl } from '@angular/forms/signals/compat';
import { email, minLength, required } from '@angular/forms/signals';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ToastModule } from 'primeng/toast';
import { FluidModule } from 'primeng/fluid';
import { MessageService } from 'primeng/api';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { RouterLink } from '@angular/router';
import { AuthStore } from '@core/auth/auth-store';
import { FieldComponent } from '@shared/components/field/field.component';
import { AppError } from '@shared/models/app-error';

@Component({
  selector: 'app-register',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    FluidModule,
    ToastModule,
    TranslocoModule,
    FieldComponent,
  ],
  providers: [MessageService],
  templateUrl: './register.component.html',
})
export class RegisterComponent {
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly messages = inject(MessageService);
  private readonly transloco = inject(TranslocoService);

  protected readonly submitted = signal(false);
  protected readonly submitting = signal(false);

  protected readonly form = new FormGroup({
    name: new SignalFormControl<string>('', (path) => required(path)),
    email: new SignalFormControl<string>('', (path) => {
      required(path);
      email(path);
    }),
    password: new SignalFormControl<string>('', (path) => {
      required(path);
      minLength(path, 8);
    }),
  });

  protected showError(field: string): boolean {
    const control = this.form.get(field);
    return !!control && control.invalid && (control.touched || this.submitted());
  }

  protected submit(): void {
    this.submitted.set(true);

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { name, email: emailValue, password } = this.form.getRawValue();
    this.submitting.set(true);
    this.auth.register(name, emailValue, password).subscribe({
      next: () => {
        this.submitting.set(false);
        this.router.navigate(['/']);
      },
      error: (error: AppError) => {
        this.submitting.set(false);
        this.messages.add({
          severity: 'error',
          detail: this.transloco.translate(`errors.${error.errorCode}`),
        });
      },
    });
  }
}
