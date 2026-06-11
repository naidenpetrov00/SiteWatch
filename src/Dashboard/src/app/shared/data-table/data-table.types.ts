export type DataTableSortDirection = 'asc' | 'desc' | '';
export type DataTableExportScope = 'allRows' | 'filteredRows' | 'currentPage';
export type DataTableFilterMode = 'instant' | 'search';

export type DataTableFilterState = Readonly<Record<string, string>>;

export interface DataTableFilterOption {
  label: string;
  value: string;
}

export interface DataTableFilterConfig<T extends object> {
  kind: 'text' | 'number' | 'boolean' | 'select';
  placeholder?: string;
  options?: readonly DataTableFilterOption[];
  valueAccessor?: (row: T) => unknown;
}

export interface DataTableColumn<T extends object> {
  key: keyof T & string;
  label: string;
  sortable?: boolean;
  filter?: DataTableFilterConfig<T>;
  valueAccessor?: (row: T) => unknown;
  displayFormatter?: (value: unknown, row: T) => string;
  exportable?: boolean;
  exportLabel?: string;
  width?: string;
  align?: 'start' | 'center' | 'end';
}

export interface DataTableSortState {
  active: string;
  direction: DataTableSortDirection;
}

export interface DataTablePageState {
  pageIndex: number;
  pageSize: number;
}

export interface DataTableState<T extends object> {
  overallRowsTotal: number;
  filteredRowsTotal: number;
  page: DataTablePageState;
  sort: DataTableSortState;
  filters: DataTableFilterState;
  draftFilters: DataTableFilterState;
  appliedFilters: DataTableFilterState;
  exportableColumns: readonly DataTableColumn<T>[];
}

export interface DataTableExportRequest<T extends object> {
  scope: DataTableExportScope;
  columns: readonly DataTableColumn<T>[];
  rows: readonly T[];
  state: DataTableState<T>;
}
