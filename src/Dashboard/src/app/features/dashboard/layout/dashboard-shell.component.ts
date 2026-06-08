import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatToolbarModule } from '@angular/material/toolbar';

import { DashboardDropdownComponent } from './dashboard-dropdown.component';
import { IdentityAuthService } from '../../identity/services/identity-auth.service';

@Component({
  selector: 'app-dashboard-shell',
  imports: [
    DashboardDropdownComponent,
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
  private readonly router = inject(Router);
  private readonly identityAuthService = inject(IdentityAuthService);

  async logOut(): Promise<void> {
    this.identityAuthService.logOut();
    await this.router.navigateByUrl('/sign-in');
  }
}
