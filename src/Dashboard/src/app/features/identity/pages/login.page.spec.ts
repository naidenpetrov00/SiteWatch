import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';

import { LoginPage } from './login.page';

describe('LoginPage', () => {
  let fixture: ComponentFixture<LoginPage>;
  let component: LoginPage;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [LoginPage, NoopAnimationsModule],
      providers: [provideHttpClient(), provideRouter([])]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginPage);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('renders the sign-in form', () => {
    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain(
      'Sign in'
    );
    expect(fixture.nativeElement.querySelector('input[type="email"]')).toBeTruthy();
    expect(fixture.nativeElement.querySelector('input[type="password"]')).toBeTruthy();
    expect(
      fixture.nativeElement.querySelector('button[type="submit"]')?.textContent
    ).toContain('Sign in');
  });

  it('keeps submit disabled while the form is invalid', () => {
    const button = fixture.nativeElement.querySelector(
      'button[type="submit"]'
    ) as HTMLButtonElement;

    expect(component.loginForm.invalid).toBeTrue();
    expect(button.disabled).toBeTrue();
  });

  it('shows validation messages after the form is touched', () => {
    component.loginForm.patchValue({
      email: 'invalid-email',
      password: ''
    });
    component.loginForm.markAllAsTouched();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent as string;

    expect(text).toContain('Enter a valid email address.');
    expect(text).toContain('Password is required.');
  });
});
