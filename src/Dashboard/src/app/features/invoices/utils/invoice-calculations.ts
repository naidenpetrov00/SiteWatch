export interface InvoiceAmountCalculation {
  vatAmount: number;
  totalValueIncludingVat: number;
}

export function parseInvoiceDecimal(value: string): number | null {
  const normalizedValue = value.trim();

  if (normalizedValue.length === 0) {
    return null;
  }

  const parsedValue = Number(normalizedValue);
  return Number.isFinite(parsedValue) ? parsedValue : null;
}

export function calculateInvoiceAmounts(
  totalValue: number,
  vatRate: number
): InvoiceAmountCalculation {
  const vatAmount = roundCurrency(totalValue * (vatRate / 100));

  return {
    vatAmount,
    totalValueIncludingVat: roundCurrency(totalValue + vatAmount)
  };
}

export function formatInvoiceAmount(value: number): string {
  return value.toFixed(2);
}

function roundCurrency(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}
