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
  cellType?: 'text' | 'button' | 'external-link';
  filter?: DataTableFilterConfig<T>;
  valueAccessor?: (row: T) => unknown;
  displayFormatter?: (value: unknown, row: T) => string;
  linkHrefAccessor?: (row: T) => string | null;
  ariaLabelAccessor?: (row: T) => string;
  buttonDisabledPredicate?: (row: T) => boolean;
  exportable?: boolean;
  exportLabel?: string;
  width?: string;
  align?: 'start' | 'center' | 'end';
}

export interface DataTableRowAction<T extends object> {
  id: string;
  label: string;
  ariaLabelAccessor?: (row: T) => string;
  disabledPredicate?: (row: T) => boolean;
}

export interface DataTableRowActionEvent<T extends object> {
  row: T;
  action: DataTableRowAction<T>;
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
