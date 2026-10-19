import { Routes } from '@angular/router';

export const ASSET_TRACKING_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/asset-tracking-shell.page').then((m) => m.AssetTrackingShellPage),
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'assets' },
      { path: 'catalog/pending', title: 'Pending Catalog Reviews', data: { pending: true }, loadComponent: () => import('./pages/catalog.page').then((m) => m.CatalogPage) },
      { path: 'catalog/:id', title: 'Catalog Details', loadComponent: () => import('./pages/catalog-detail.page').then((m) => m.CatalogDetailPage) },
      { path: 'catalog', title: 'Asset Catalog', loadComponent: () => import('./pages/catalog.page').then((m) => m.CatalogPage) },
      { path: 'assets/pending', title: 'Pending Registrations', data: { pending: true }, loadComponent: () => import('./pages/assets.page').then((m) => m.AssetsPage) },
      { path: 'register', title: 'Register Assets', loadComponent: () => import('./pages/register-assets.page').then((m) => m.RegisterAssetsPage) },
      { path: 'assets/:id/label', title: 'Print Asset Label', loadComponent: () => import('./pages/asset-label.page').then((m) => m.AssetLabelPage) },
      { path: 'assets/:id', title: 'Asset Details', loadComponent: () => import('./pages/asset-detail.page').then((m) => m.AssetDetailPage) },
      { path: 'assets', title: 'Assets', loadComponent: () => import('./pages/assets.page').then((m) => m.AssetsPage) }
    ]
  }
];
