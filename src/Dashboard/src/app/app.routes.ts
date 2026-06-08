import { Routes } from '@angular/router';

import { authGuard } from './features/identity/guards/auth.guard';
import { guestGuard } from './features/identity/guards/guest.guard';

export const routes: Routes = [
  {
    path: 'sign-in',
    canMatch: [guestGuard],
    loadComponent: () =>
      import('./features/identity/pages/login.page').then(
        (m) => m.LoginPage
      )
  },
  {
    canMatch: [authGuard],
    path: '',
    loadComponent: () =>
      import('./features/dashboard/layout/dashboard-shell.component').then(
        (m) => m.DashboardShellComponent
      ),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/home/pages/home.page').then((m) => m.HomePage)
      },
      {
        path: 'invoices',
        loadComponent: () =>
          import('./features/invoices/pages/invoices.page').then(
            (m) => m.InvoicesPage
          )
      },
      {
        path: 'scan-invoice',
        loadComponent: () =>
          import('./features/invoices/pages/scan-invoice.page').then(
            (m) => m.ScanInvoicePage
          )
      },
      {
        path: 'manage-users',
        loadComponent: () =>
          import('./features/users/pages/manage-users.page').then(
            (m) => m.ManageUsersPage
          )
      }
    ]
  }
];
