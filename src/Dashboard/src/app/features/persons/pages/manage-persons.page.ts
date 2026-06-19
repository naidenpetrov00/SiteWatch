import {
  ChangeDetectionStrategy,
  Component,
  effect,
  inject,
  signal
} from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import {
  DataTableColumn,
  DataTableState
} from '../../../shared/data-table/data-table.types';
import { AddPersonDialogComponent } from '../components/add-person-dialog/add-person-dialog.component';
import { DashboardPerson } from '../models/dashboard-person.model';
import { DashboardPersonsService } from '../services/dashboard-persons.service';

const PERSON_COLUMNS: readonly DataTableColumn<DashboardPerson>[] = [
  {
    key: 'numberId',
    label: 'Number Id',
    sortable: true,
    cellType: 'button'
  },
  {
    key: 'id',
    label: 'Id',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Id' }
  },
  {
    key: 'type',
    label: 'Type',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Type' }
  },
  {
    key: 'displayName',
    label: 'Display Name',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Display Name' }
  },
  {
    key: 'firstName',
    label: 'First Name',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter First Name' }
  },
  {
    key: 'middleName',
    label: 'Middle Name',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Middle Name' }
  },
  {
    key: 'lastName',
    label: 'Last Name',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Last Name' }
  },
  {
    key: 'companyName',
    label: 'Company Name',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Company Name' }
  },
  {
    key: 'legalForm',
    label: 'Legal Form'
  },
  {
    key: 'egn',
    label: 'EGN',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter EGN' }
  },
  {
    key: 'eik',
    label: 'EIK',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter EIK' }
  },
  {
    key: 'vatNumber',
    label: 'VAT Number',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter VAT Number' }
  }
] as const;

@Component({
  selector: 'app-manage-persons-page',
  imports: [
    ActionButtonComponent,
    DataTableComponent,
    MatDialogModule
  ],
  templateUrl: './manage-persons.page.html',
  styleUrl: './manage-persons.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManagePersonsPage {
  private readonly dashboardPersonsService = inject(DashboardPersonsService);
  private readonly dialog = inject(MatDialog);

  readonly persons = signal<readonly DashboardPerson[]>([]);
  readonly personsFilteredCount = signal(0);
  readonly personsTotalCount = signal(0);
  readonly tableState = signal<DataTableState<DashboardPerson> | null>(null);
  readonly columns = PERSON_COLUMNS;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;

  constructor() {
    effect(() => {
      const tableState = this.tableState();

      if (!tableState) {
        return;
      }

      this.dashboardPersonsService.setTableState(tableState);
    });

    effect(() => {
      const dashboardPersons = this.dashboardPersonsService.dashboardPersonsQuery.data();

      if (!dashboardPersons) {
        return;
      }

      this.persons.set(dashboardPersons.items);
      this.personsFilteredCount.set(dashboardPersons.filteredCount);
      this.personsTotalCount.set(dashboardPersons.totalCount);
    });
  }

  onTableStateChange(state: DataTableState<DashboardPerson>): void {
    this.tableState.set(state);
  }

  openAddPersonDialog(): void {
    this.dialog.open(AddPersonDialogComponent, {
      autoFocus: false,
      width: '72rem',
      maxWidth: 'calc(100vw - 2rem)'
    });
  }

  onNumberIdClick(person: DashboardPerson): void {
    this.dialog.open(AddPersonDialogComponent, {
      autoFocus: false,
      width: '72rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: person
    });
  }
}
