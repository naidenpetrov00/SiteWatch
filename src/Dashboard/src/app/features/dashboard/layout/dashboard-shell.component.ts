import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import {
  IsActiveMatchOptions,
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';

import { CdkMenuModule } from '@angular/cdk/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';

import { IdentityAuthService } from '../../identity/services/identity-auth.service';

@Component({
  selector: 'app-dashboard-shell',
  imports: [
    CdkMenuModule,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
    MatButtonModule,
    MatIconModule,
    MatMenuModule,
    MatToolbarModule
  ],
  templateUrl: './dashboard-shell.component.html',
  styleUrl: './dashboard-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardShellComponent {
  private readonly exactMatchOptions: IsActiveMatchOptions = {
    paths: 'exact',
    matrixParams: 'ignored',
    queryParams: 'ignored',
    fragment: 'ignored'
  };

  private readonly router = inject(Router);
  private readonly identityAuthService = inject(IdentityAuthService);

  async logOut(): Promise<void> {
    this.identityAuthService.logOut();
    await this.router.navigateByUrl('/sign-in');
  }

  isInvoiceManagementActive(): boolean {
    return (
      this.router.isActive('/invoices', this.exactMatchOptions) ||
      this.router.isActive('/scan-invoice', this.exactMatchOptions)
    );
  }

  isAdministrationActive(): boolean {
    return this.router.isActive('/manage-users', this.exactMatchOptions);
  }
}
