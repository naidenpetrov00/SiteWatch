import { Routes } from '@angular/router';

import { authGuard } from './features/identity/guards/auth.guard';
import { guestGuard } from './features/identity/guards/guest.guard';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    canMatch: [authGuard],
    loadComponent: () =>
      import('./features/home/pages/home.page').then((m) => m.HomePage)
  },
  {
    path: 'invoices',
    canMatch: [authGuard],
    loadComponent: () =>
      import('./features/invoices/pages/invoices.page').then(
        (m) => m.InvoicesPage
      )
  },
  {
    path: 'sign-in',
    canMatch: [guestGuard],
    loadComponent: () =>
      import('./features/identity/pages/login.page').then(
        (m) => m.LoginPage
      )
  }
];
