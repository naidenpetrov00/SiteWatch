import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-asset-tracking-shell',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <section class="asset-shell">
      <header><p>Administration</p><h1>Asset Tracking</h1></header>
      <nav aria-label="Asset tracking navigation">
        <a routerLink="assets" routerLinkActive="active">Assets</a>
        <a routerLink="assets/pending" routerLinkActive="active">Pending Registrations</a>
        <a routerLink="register" routerLinkActive="active">Register Assets</a>
      </nav>
      <router-outlet />
    </section>`,
  styles: `
    :host{display:block}.asset-shell>header p{margin:0;color:var(--mat-sys-primary);font-weight:700;text-transform:uppercase;letter-spacing:.08em}.asset-shell>header h1{margin:.25rem 0 1rem}.asset-shell>nav{display:flex;flex-wrap:wrap;gap:.5rem;margin-bottom:1.5rem}.asset-shell>nav a{padding:.65rem .9rem;border-radius:999px;text-decoration:none;color:var(--mat-sys-on-surface);background:var(--mat-sys-surface-container)}.asset-shell>nav a.active{background:var(--mat-sys-primary);color:var(--mat-sys-on-primary)}`,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetTrackingShellPage {}
