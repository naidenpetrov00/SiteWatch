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
          { path: 'scan-invoice', component: DummyComponent }
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
    expect(compiled.querySelector('button[aria-label="Account menu"]')).toBeTruthy();
  });

  it('marks invoice management and the current menu item as active on child routes', async () => {
    const fixture = TestBed.createComponent(DashboardShellComponent);
    const router = TestBed.inject(Router);

    await router.navigateByUrl('/invoices');
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    const nav = compiled.querySelector('.dashboard-shell__nav');
    expect(nav?.classList.contains('dashboard-shell__nav--active')).toBeTrue();

    const trigger = compiled.querySelector(
      'button[aria-label="Invoice management menu"]'
    ) as HTMLButtonElement;
    trigger.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const activeMenuItem = document.body.querySelector(
      '.dashboard-shell__menu-item--active'
    );
    expect(activeMenuItem?.textContent).toContain('Invoices');

    trigger.click();
    fixture.detectChanges();
    await fixture.whenStable();
  });

  it('marks the scan invoice item as active when that route is selected', async () => {
    const fixture = TestBed.createComponent(DashboardShellComponent);
    const router = TestBed.inject(Router);

    await router.navigateByUrl('/scan-invoice');
    fixture.detectChanges();
    await fixture.whenStable();

    const compiled = fixture.nativeElement as HTMLElement;
    const nav = compiled.querySelector('.dashboard-shell__nav');
    expect(nav?.classList.contains('dashboard-shell__nav--active')).toBeTrue();

    const trigger = compiled.querySelector(
      'button[aria-label="Invoice management menu"]'
    ) as HTMLButtonElement;
    trigger.click();
    fixture.detectChanges();
    await fixture.whenStable();

    const activeMenuItem = document.body.querySelector(
      '.dashboard-shell__menu-item--active'
    );
    expect(activeMenuItem?.textContent).toContain('Scan Invoice');

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
