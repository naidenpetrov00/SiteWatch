import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  signal
} from '@angular/core';
import {
  DataTableColumn,
  DataTableState
} from '../../../shared/data-table/data-table.types';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { DashboardUser } from '../models/dashboard-user.model';
import { DashboardUsersService } from '../services/dashboard-users.service';
import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DialogShellComponent } from '../../../shared/ui/dialog-shell/dialog-shell.component';

const USER_COLUMNS: readonly DataTableColumn<DashboardUser>[] = [
  {
    key: 'id',
    label: 'Id',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Id' }
  },
  {
    key: 'username',
    label: 'Username',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Username' }
  },
  {
    key: 'email',
    label: 'Email',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Email' }
  },
  {
    key: 'phoneNumber',
    label: 'Phone Number',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Phone Number' }
  },
  {
    key: 'isEmailConfirmed',
    label: 'Is Email Confirmed',
    sortable: true,
    align: 'center',
    filter: { kind: 'boolean', placeholder: 'Email Confirmed' }
  },
  {
    key: 'isPhoneNumberConfirmed',
    label: 'Is Phone Number Confirmed',
    sortable: true,
    align: 'center',
    filter: { kind: 'boolean', placeholder: 'Phone Confirmed' }
  },
  {
    key: 'lastLoginAt',
    label: 'Last Login At',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Last Login' },
    displayFormatter: (value) => {
      if (!value) {
        return 'Never';
      }

      return new Date(String(value)).toLocaleString();
    }
  }
] as const;

@Component({
  selector: 'app-manage-users-page',
  imports: [ActionButtonComponent, DataTableComponent, MatDialogModule],
  templateUrl: './manage-users.page.html',
  styleUrl: './manage-users.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageUsersPage {
  private readonly dashboardUsersService = inject(DashboardUsersService);
  private readonly dialog = inject(MatDialog);

  readonly users = signal<readonly DashboardUser[]>([]);
  readonly usersFilteredCount = signal(0);
  readonly usersTotalCount = signal(0);
  readonly tableState = signal<DataTableState<DashboardUser> | null>(null);
  readonly columns = USER_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;

  constructor() {
    effect(() => {
      const tableState = this.tableState();

      if (!tableState) {
        return;
      }

      this.dashboardUsersService.setTableState(tableState);
    });

    effect(() => {
      const dashboardUsers = this.dashboardUsersService.dashboardUsersQuery.data();

      if (!dashboardUsers) {
        return;
      }

      this.users.set(dashboardUsers.items);
      this.usersFilteredCount.set(dashboardUsers.filteredCount);
      this.usersTotalCount.set(dashboardUsers.totalCount);
    });
  }

  onTableStateChange(state: DataTableState<DashboardUser>): void {
    this.tableState.set(state);
  }

  openAddUserDialog(): void {
    this.dialog.open(DialogShellComponent);
  }
}
