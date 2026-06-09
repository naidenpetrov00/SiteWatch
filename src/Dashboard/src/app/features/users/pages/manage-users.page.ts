import { ChangeDetectionStrategy, Component, computed, signal } from '@angular/core';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import {
  DataTableColumn,
  DataTableState
} from '../../../shared/data-table/data-table.types';
import { filterRows } from '../../../shared/data-table/data-table.utils';
import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';

interface ManageUsersRow {
  id: string;
  userName: string;
  normalizedUserName: string;
  email: string;
  normalizedEmail: string;
  emailConfirmed: boolean;
  passwordHash: string;
  securityStamp: string;
  concurrencyStamp: string;
  phoneNumber: string | null;
  phoneNumberConfirmed: boolean;
  twoFactorEnabled: boolean;
  lockoutEnd: string | null;
  lockoutEnabled: boolean;
  accessFailedCount: number;
}

const USER_COLUMNS: readonly DataTableColumn<ManageUsersRow>[] = [
  {
    key: 'id',
    label: 'Id',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Id' }
  },
  {
    key: 'userName',
    label: 'User Name',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter User Name' }
  },
  {
    key: 'email',
    label: 'Email',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Email' }
  },
  {
    key: 'emailConfirmed',
    label: 'Email Confirmed',
    sortable: true,
    align: 'center',
    filter: { kind: 'boolean', placeholder: 'Email Confirmed' }
  },
  {
    key: 'phoneNumber',
    label: 'Phone Number',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Phone Number' }
  },
  {
    key: 'phoneNumberConfirmed',
    label: 'Phone Confirmed',
    sortable: true,
    align: 'center',
    filter: { kind: 'boolean', placeholder: 'Phone Confirmed' }
  },
  {
    key: 'twoFactorEnabled',
    label: 'Two Factor',
    sortable: true,
    align: 'center',
    filter: { kind: 'boolean', placeholder: 'Two Factor' }
  },
  {
    key: 'lockoutEnabled',
    label: 'Lockout Enabled',
    sortable: true,
    align: 'center',
    filter: { kind: 'boolean', placeholder: 'Lockout Enabled' }
  },
  {
    key: 'accessFailedCount',
    label: 'Access Failed',
    sortable: true,
    align: 'end',
    filter: { kind: 'number', placeholder: 'Filter Failed Count' }
  }
] as const;

const DUMMY_USERS: readonly ManageUsersRow[] = createDummyUsers();

@Component({
  selector: 'app-manage-users-page',
  imports: [ActionButtonComponent, DataTableComponent],
  templateUrl: './manage-users.page.html',
  styleUrl: './manage-users.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageUsersPage {
  readonly allUsers = signal<readonly ManageUsersRow[]>(DUMMY_USERS);
  readonly users = signal<readonly ManageUsersRow[]>(DUMMY_USERS);
  readonly tableState = signal<DataTableState<ManageUsersRow> | null>(null);
  readonly columns = USER_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500] as const;

  readonly visibleUsersCount = computed(
    () => this.tableState()?.filteredRowsTotal ?? this.users().length
  );

  onTableStateChange(state: DataTableState<ManageUsersRow>): void {
    this.tableState.set(state);
  }

  onSearchRequested(state: DataTableState<ManageUsersRow>): void {
    this.users.set(filterRows(this.allUsers(), this.columns, state.appliedFilters));
  }
}

function createDummyUsers(): ManageUsersRow[] {
  const seeds = [
    'alice.owens',
    'brad.morris',
    'carmen.ivanova',
    'daniel.kovacs',
    'elena.popova',
    'filip.novak',
    'georgia.petrov',
    'ivan.dimitrov',
    'lina.stoyanova',
    'martin.georgiev',
    'nikolay.todorov',
    'olivia.hayes'
  ];

  return seeds.map((userName, index) => {
    const id = `user-${index + 1}`;
    const normalizedUserName = userName.toUpperCase();
    const email = `${userName}@sitewatch.test`;
    const normalizedEmail = email.toUpperCase();

    return {
      id,
      userName,
      normalizedUserName,
      email,
      normalizedEmail,
      emailConfirmed: index % 2 === 0,
      passwordHash: `hash-${index + 1}`,
      securityStamp: `security-stamp-${index + 1}`,
      concurrencyStamp: `concurrency-stamp-${index + 1}`,
      phoneNumber: index % 3 === 0 ? `+359 88 100 0${index + 1}` : null,
      phoneNumberConfirmed: index % 3 === 0,
      twoFactorEnabled: index % 4 === 0,
      lockoutEnd: null,
      lockoutEnabled: index % 5 !== 0,
      accessFailedCount: index % 3
    } satisfies ManageUsersRow;
  });
}
