import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  signal,
  untracked
} from '@angular/core';

import { DashboardUser } from '../models/dashboard-user.model';
import { DashboardUsersService } from '../services/dashboard-users.service';
import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import {
  DataTableColumn,
  DataTableState
} from '../../../shared/data-table/data-table.types';
import { filterRows } from '../../../shared/data-table/data-table.utils';
import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';

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
  imports: [ActionButtonComponent, DataTableComponent],
  templateUrl: './manage-users.page.html',
  styleUrl: './manage-users.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageUsersPage {
  private readonly dashboardUsersService = inject(DashboardUsersService);

  readonly allUsers = signal<readonly DashboardUser[]>([]);
  readonly users = signal<readonly DashboardUser[]>([]);
  readonly tableState = signal<DataTableState<DashboardUser> | null>(null);
  readonly columns = USER_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500] as const;

  constructor() {
    effect(() => {
      const dashboardUsers = this.dashboardUsersService.dashboardUsersQuery.data() ?? [];
      const appliedFilters = untracked(
        () => this.tableState()?.appliedFilters ?? {}
      );

      this.allUsers.set(dashboardUsers);
      this.users.set(filterRows(dashboardUsers, this.columns, appliedFilters));
    });
  }

  onTableStateChange(state: DataTableState<DashboardUser>): void {
    this.tableState.set(state);
  }

  onSearchRequested(state: DataTableState<DashboardUser>): void {
    this.users.set(filterRows(this.allUsers(), this.columns, state.appliedFilters));
  }
}
