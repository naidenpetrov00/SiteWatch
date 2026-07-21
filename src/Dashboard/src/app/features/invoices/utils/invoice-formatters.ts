export function formatInvoiceTextValue(value: string | null | undefined): string {
  if (value === null || value === undefined) {
    return '—';
  }

  const trimmedValue = value.trim();

  return trimmedValue.length > 0 ? trimmedValue : '—';
}

export function formatInvoiceDateValue(value: unknown): string {
  if (value === null || value === undefined || value === '') {
    return '—';
  }

  const dateValue = new Date(String(value));

  if (Number.isNaN(dateValue.getTime())) {
    return String(value);
  }

  return dateValue.toLocaleDateString();
}

export function formatInvoiceDateTimeValue(value: unknown): string {
  if (value === null || value === undefined || value === '') {
    return '—';
  }

  const dateValue = new Date(String(value));

  if (Number.isNaN(dateValue.getTime())) {
    return String(value);
  }

  return dateValue.toLocaleString();
}

export function formatInvoiceAmountValue(value: unknown): string {
  if (value === null || value === undefined || value === '') {
    return '—';
  }

  const numericValue = Number(value);

  if (Number.isNaN(numericValue)) {
    return String(value);
  }

  return new Intl.NumberFormat(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  }).format(numericValue);
}
