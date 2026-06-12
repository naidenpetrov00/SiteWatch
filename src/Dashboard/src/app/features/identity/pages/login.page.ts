import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { IdentityAuthService } from '../services/identity-auth.service';

@Component({
  selector: 'app-login-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './login.page.html',
  styleUrl: './login.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoginPage {
  private readonly formBuilder = inject(FormBuilder);
  private readonly identityAuthService = inject(IdentityAuthService);
  private readonly router = inject(Router);

  readonly loginForm = this.formBuilder.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  readonly emailControl = this.loginForm.controls.email;
  readonly passwordControl = this.loginForm.controls.password;
  readonly serverErrorMessage = signal<string | null>(null);
  readonly isSubmitting = this.identityAuthService.signInMutation.isPending;

  async onSubmit(): Promise<void> {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.serverErrorMessage.set(null);
    try {
      const result = await this.identityAuthService.signIn(
        this.loginForm.controls.email.value,
        this.loginForm.controls.password.value
      );

      if (result.succeeded) {
        await this.router.navigateByUrl('/');
        return;
      }

      this.serverErrorMessage.set(result.errors[0] ?? 'Unable to sign in.');
    } catch {
      this.serverErrorMessage.set('Unable to sign in.');
    }
  }
}
