import { Routes } from '@angular/router';

import { authGuard } from './features/identity/guards/auth.guard';
import { guestGuard } from './features/identity/guards/guest.guard';

export const routes: Routes = [
  {
    path: 'sign-in',
    title: 'Sign In',
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
        title: 'Home Page',
        loadComponent: () =>
          import('./features/home/pages/home.page').then((m) => m.HomePage)
      },
      {
        path: 'invoices',
        title: 'Invoices',
        loadComponent: () =>
          import('./features/invoices/pages/invoices.page').then(
            (m) => m.InvoicesPage
          )
      },
      {
        path: 'scan-invoice',
        title: 'Scan Invoice',
        loadComponent: () =>
          import('./features/invoices/pages/scan-invoice.page').then(
            (m) => m.ScanInvoicePage
          )
      },
      {
        path: 'manage-sites',
        title: 'Manage Sites',
        loadComponent: () =>
          import('./features/sites/pages/manage-sites.page').then(
            (m) => m.ManageSitesPage
          )
      },
      {
        path: 'manage-issues',
        title: 'Manage Issues',
        loadComponent: () =>
          import('./features/issues/pages/manage-issues.page').then(
            (m) => m.ManageIssuesPage
          )
      },
      {
        path: 'manage-users',
        title: 'Manage Users',
        loadComponent: () =>
          import('./features/users/pages/manage-users.page').then(
            (m) => m.ManageUsersPage
          )
      },
      {
        path: 'manage-persons',
        title: 'Manage Persons',
        loadComponent: () =>
          import('./features/persons/pages/manage-persons.page').then(
            (m) => m.ManagePersonsPage
          )
      },
      {
        path: 'manage-products',
        title: 'Manage Products',
        loadComponent: () =>
          import('./features/products/pages/manage-products.page').then(
            (m) => m.ManageProductsPage
          )
      },
      {
        path: 'manage-retailers',
        title: 'Manage Retailers',
        loadComponent: () =>
          import('./features/retailers/pages/manage-retailers.page').then(
            (m) => m.ManageRetailersPage
          )
      },
      {
        path: 'manage-activities',
        title: 'Manage Activities',
        loadComponent: () =>
          import('./features/activities/pages/manage-activities.page').then(
            (m) => m.ManageActivitiesPage
          )
      },
      {
        path: 'manage-cameras',
        title: 'Manage Cameras',
        loadComponent: () =>
          import('./features/cameras/pages/manage-cameras.page').then(
            (m) => m.ManageCamerasPage
          )
      }
    ]
  }
];
