import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideRouter, Router } from '@angular/router';

import { IdentityAuthService } from '../../identity/services/identity-auth.service';
import { DashboardShellComponent } from './dashboard-shell.component';

@Component({
  template: ''
})
class DummyComponent {}

describe('DashboardShellComponent', () => {
  const authService = {
    logOut: () => undefined
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DashboardShellComponent, NoopAnimationsModule],
      providers: [
        provideRouter([
          { path: 'sign-in', component: DummyComponent },
          { path: 'invoices', component: DummyComponent },
          { path: 'scan-invoice', component: DummyComponent },
          { path: 'manage-users', component: DummyComponent }
        ]),
        { provide: IdentityAuthService, useValue: authService }
      ]
    }).compileComponents();
  });

  it('renders the dashboard navigation', () => {
    const fixture = TestBed.createComponent(DashboardShellComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('SiteWatch Dashboard');
    expect(compiled.textContent).toContain('Invoice Management');
    expect(compiled.textContent).toContain('Administration');
    expect(compiled.querySelector('button[aria-label="Account menu"]')).toBeTruthy();
  });

  it('opens the invoice menu from the trigger', async () => {
    const fixture = TestBed.createComponent(DashboardShellComponent);
    const compiled = fixture.nativeElement as HTMLElement;
    const trigger = compiled.querySelector(
      'button[aria-label="Invoice management menu"]'
    ) as HTMLButtonElement;

    fixture.detectChanges();
    trigger.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const openedMenu = document.body.querySelector('.dashboard-shell__menu-panel');
    expect(openedMenu?.textContent).toContain('Invoices');
    expect(openedMenu?.textContent).toContain('Scan Invoice');

    trigger.click();
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('opens the administration menu from the trigger', async () => {
    const fixture = TestBed.createComponent(DashboardShellComponent);
    const compiled = fixture.nativeElement as HTMLElement;
    const trigger = compiled.querySelector(
      'button[aria-label="Administration menu"]'
    ) as HTMLButtonElement;

    fixture.detectChanges();
    trigger.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const openedMenu = document.body.querySelector('.dashboard-shell__menu-panel');
    expect(openedMenu?.textContent).toContain('Manage Users');

    trigger.click();
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('logs out and navigates to sign-in', async () => {
    const fixture = TestBed.createComponent(DashboardShellComponent);
    const component = fixture.componentInstance;
    const router = TestBed.inject(Router);
    const navigateSpy = spyOn(router, 'navigateByUrl').and.returnValue(
      Promise.resolve(true)
    );
    const logOutSpy = spyOn(authService, 'logOut');

    fixture.detectChanges();

    await component.logOut();

    expect(logOutSpy).toHaveBeenCalled();
    expect(navigateSpy).toHaveBeenCalledWith('/sign-in');
  });
});
