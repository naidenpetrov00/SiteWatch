import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  input,
  output,
  signal
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatTableModule } from '@angular/material/table';

import {
  DataTableColumn,
  DataTableExportRequest,
  DataTableFilterMode,
  DataTablePageState,
  DataTableState,
  DataTableSortState
} from './data-table.types';
import {
  areFilterStatesEqual,
  filterRows,
  formatCellValue,
  getColumnValue,
  normalizeFilterState,
  sortRows
} from './data-table.utils';
import { ActionButtonComponent } from '../ui/action-button/action-button.component';

@Component({
  selector: 'app-data-table',
  imports: [
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatPaginatorModule,
    MatSelectModule,
    MatSortModule,
    MatTableModule,
    ActionButtonComponent
  ],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DataTableComponent<T extends object> {
  readonly columns = input.required<readonly DataTableColumn<T>[]>();
  readonly rows = input.required<readonly T[]>();
  readonly tableLabel = input('Data table');
  readonly tableEyebrow = input('Overview');
  readonly emptyMessage = input('No matching records found.');
  readonly pageSize = input(5);
  readonly pageSizeOptions = input<readonly number[]>([5, 10, 25]);
  readonly filterApplyMode = input<DataTableFilterMode>('instant');

  readonly tableStateChange = output<DataTableState<T>>();
  readonly searchRequested = output<DataTableState<T>>();
  readonly exportRequested = output<DataTableExportRequest<T>>();

  readonly draftFilterState = signal<Record<string, string>>({});
  readonly appliedFilterState = signal<Record<string, string>>({});
  readonly filtersVisible = signal(false);
  readonly sortState = signal<DataTableSortState>({
    active: '',
    direction: ''
  });
  readonly pageState = signal<DataTablePageState>({
    pageIndex: 0,
    pageSize: this.pageSize()
  });

  readonly displayedColumns = computed(() => this.columns().map((column) => column.key));
  readonly filterableColumns = computed(() =>
    this.columns().filter((column) => column.filter)
  );
  readonly hasPendingFilterChanges = computed(
    () => !areFilterStatesEqual(this.draftFilterState(), this.appliedFilterState())
  );
  readonly activeFilterState = computed(() =>
    this.filterApplyMode() === 'search'
      ? this.appliedFilterState()
      : normalizeFilterState(this.draftFilterState())
  );
  readonly filteredRows = computed(() =>
    this.filterApplyMode() === 'search'
      ? [...this.rows()]
      : filterRows(this.rows(), this.columns(), this.activeFilterState())
  );
  readonly sortedRows = computed(() =>
    sortRows(this.filteredRows(), this.columns(), this.sortState())
  );
  readonly effectivePageIndex = computed(() => {
    const pageState = this.pageState();
    const totalPages = Math.max(1, Math.ceil(this.sortedRows().length / pageState.pageSize));

    return Math.min(pageState.pageIndex, totalPages - 1);
  });
  readonly pagedRows = computed(() => {
    const pageState = this.pageState();
    const pageIndex = this.effectivePageIndex();
    const startIndex = pageIndex * pageState.pageSize;

    return this.sortedRows().slice(startIndex, startIndex + pageState.pageSize);
  });
  readonly tableState = computed<DataTableState<T>>(() => ({
    rowsTotal: this.rows().length,
    filteredRowsTotal: this.filteredRows().length,
    page: {
      pageIndex: this.effectivePageIndex(),
      pageSize: this.pageState().pageSize
    },
    sort: {
      ...this.sortState()
    },
    filters: {
      ...this.appliedFilterState()
    },
    draftFilters: {
      ...this.draftFilterState()
    },
    appliedFilters: {
      ...this.appliedFilterState()
    },
    exportableColumns: [
      ...this.columns().filter((column) => column.exportable !== false)
    ]
  }));

  constructor() {
    effect(() => {
      const defaultPageSize = this.pageSize();

      this.pageState.update((page) =>
        page.pageSize === defaultPageSize
          ? page
          : {
              pageIndex: 0,
              pageSize: defaultPageSize
            }
      );
    });

    effect(() => {
      this.tableStateChange.emit(this.tableState());
    });
  }

  onSortChange(sort: Sort): void {
    this.sortState.set({
      active: sort.active,
      direction: sort.direction
    });
    this.pageState.update((page) => ({
      ...page,
      pageIndex: 0
    }));
  }

  onPageChange(event: PageEvent): void {
    this.pageState.set({
      pageIndex: event.pageIndex,
      pageSize: event.pageSize
    });
  }

  onFilterValueChange(columnKey: string, value: string): void {
    const normalizedValue = value.trim();
    const nextDraftFilters = { ...this.draftFilterState() };

    if (normalizedValue.length === 0) {
      delete nextDraftFilters[columnKey];
    } else {
      nextDraftFilters[columnKey] = value;
    }

    this.draftFilterState.set(nextDraftFilters);

    if (this.filterApplyMode() === 'instant') {
      this.appliedFilterState.set(normalizeFilterState(nextDraftFilters));
      this.pageState.update((page) => ({
        ...page,
        pageIndex: 0
      }));
    }
  }

  clearFilters(): void {
    this.draftFilterState.set({});

    if (this.filterApplyMode() === 'instant') {
      this.appliedFilterState.set({});
      this.pageState.update((page) => ({
        ...page,
        pageIndex: 0
      }));
    }
  }

  toggleFiltersVisibility(): void {
    this.filtersVisible.update((isVisible) => !isVisible);
  }

  getFilterValue(columnKey: string): string {
    return this.draftFilterState()[columnKey] ?? '';
  }

  applySearchFilters(): void {
    if (this.filterApplyMode() !== 'search') {
      return;
    }

    this.appliedFilterState.set(normalizeFilterState(this.draftFilterState()));
    this.pageState.update((page) => ({
      ...page,
      pageIndex: 0
    }));
    this.searchRequested.emit(this.tableState());
  }

  onExportClick(): void {
    this.exportRequested.emit({
      scope: 'filteredRows',
      columns: this.tableState().exportableColumns,
      rows: this.filteredRows(),
      state: this.tableState()
    });
  }

  renderCell(row: T, column: DataTableColumn<T>): string {
    const value = getColumnValue(row, column);
    const formattedValue = column.displayFormatter?.(value, row);

    if (formattedValue !== undefined) {
      return formattedValue;
    }

    return formatCellValue(value);
  }
}
