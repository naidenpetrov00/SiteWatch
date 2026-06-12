import { DataTableColumn, DataTableSortState } from './data-table.types';

export function filterRows<T extends object>(
  rows: readonly T[],
  columns: readonly DataTableColumn<T>[],
  filterState: Readonly<Record<string, string>>
): T[] {
  return rows.filter((row) =>
    columns.every((column) => {
      const filterConfig = column.filter;

      if (!filterConfig) {
        return true;
      }

      const filterValue = normalizeFilterValue(filterState[column.key]);

      if (filterValue.length === 0) {
        return true;
      }

      const cellValue = getFilterColumnValue(row, column, filterConfig);

      if (filterConfig.kind !== 'text') {
        return normalizeComparableValue(cellValue) === filterValue;
      }

      return normalizeComparableValue(cellValue).includes(filterValue);
    })
  );
}

export function sortRows<T extends object>(
  rows: readonly T[],
  columns: readonly DataTableColumn<T>[],
  sortState: DataTableSortState
): T[] {
  if (!sortState.active || sortState.direction === '') {
    return [...rows];
  }

  const column = columns.find((candidate) => candidate.key === sortState.active);

  if (!column || column.sortable === false) {
    return [...rows];
  }

  const sortedRows = [...rows].sort((leftRow, rightRow) => {
    const leftValue = getColumnValue(leftRow, column);
    const rightValue = getColumnValue(rightRow, column);
    const comparison = compareValues(leftValue, rightValue);

    return sortState.direction === 'asc' ? comparison : -comparison;
  });

  return sortedRows;
}

export function getColumnValue<T extends object>(
  row: T,
  column: DataTableColumn<T>
): unknown {
  return column.valueAccessor?.(row) ?? row[column.key];
}

export function getFilterColumnValue<T extends object>(
  row: T,
  column: DataTableColumn<T>,
  filterConfig: NonNullable<DataTableColumn<T>['filter']>
): unknown {
  return filterConfig.valueAccessor?.(row) ?? getColumnValue(row, column);
}

export function compareValues(leftValue: unknown, rightValue: unknown): number {
  if (typeof leftValue === 'number' && typeof rightValue === 'number') {
    return leftValue - rightValue;
  }

  if (typeof leftValue === 'boolean' && typeof rightValue === 'boolean') {
    return Number(leftValue) - Number(rightValue);
  }

  return normalizeComparableValue(leftValue).localeCompare(
    normalizeComparableValue(rightValue),
    undefined,
    {
      numeric: true,
      sensitivity: 'base'
    }
  );
}

export function formatCellValue(value: unknown): string {
  if (value === null || value === undefined || value === '') {
    return '—';
  }

  if (typeof value === 'boolean') {
    return value ? 'Yes' : 'No';
  }

  return String(value);
}

export function normalizeFilterValue(value: string | undefined): string {
  return (value ?? '').trim().toLowerCase();
}

export function normalizeFilterState(
  filterState: Readonly<Record<string, string>>
): Record<string, string> {
  return Object.entries(filterState).reduce<Record<string, string>>((normalizedState, [key, value]) => {
    const normalizedValue = normalizeFilterValue(value);

    if (normalizedValue.length > 0) {
      normalizedState[key] = normalizedValue;
    }

    return normalizedState;
  }, {});
}

export function areFilterStatesEqual(
  leftState: Readonly<Record<string, string>>,
  rightState: Readonly<Record<string, string>>
): boolean {
  const normalizedLeftState = normalizeFilterState(leftState);
  const normalizedRightState = normalizeFilterState(rightState);
  const leftKeys = Object.keys(normalizedLeftState);
  const rightKeys = Object.keys(normalizedRightState);

  if (leftKeys.length !== rightKeys.length) {
    return false;
  }

  return leftKeys.every(
    (key) => normalizedLeftState[key] === normalizedRightState[key]
  );
}

export function normalizeComparableValue(value: unknown): string {
  if (value === null || value === undefined) {
    return '';
  }

  if (typeof value === 'boolean') {
    return value ? 'true' : 'false';
  }

  return String(value).trim().toLowerCase();
}
