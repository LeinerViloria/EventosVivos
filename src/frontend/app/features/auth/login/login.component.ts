import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { SignalFormControl } from '@angular/forms/signals/compat';
import { email, required } from '@angular/forms/signals';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { MessageModule } from 'primeng/message';
import { ToastModule } from 'primeng/toast';
import { FluidModule } from 'primeng/fluid';
import { MessageService } from 'primeng/api';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { AuthStore } from '@core/auth/auth-store';
import { AppError } from '@shared/models/app-error';

@Component({
  selector: 'app-login',
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    InputTextModule,
    PasswordModule,
    MessageModule,
    FluidModule,
    ToastModule,
    TranslocoModule,
  ],
  providers: [MessageService],
  templateUrl: './login.component.html',
})
export class LoginComponent {
  private readonly auth = inject(AuthStore);
  private readonly router = inject(Router);
  private readonly messages = inject(MessageService);
  private readonly transloco = inject(TranslocoService);

  protected readonly submitted = signal(false);
  protected readonly submitting = signal(false);

  protected readonly form = new FormGroup({
    email: new SignalFormControl<string>('', (path) => {
      required(path);
      email(path);
    }),
    password: new SignalFormControl<string>('', (path) => required(path)),
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

    const { email: emailValue, password } = this.form.getRawValue();
    this.submitting.set(true);
    this.auth.login(emailValue, password).subscribe({
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
