import { AddInvoiceDialogFormGroup } from '../components/add-invoice-dialog/add-invoice-dialog.types';
import { CreateDashboardInvoiceRequest } from '../models/create-dashboard-invoice-request.model';

function normalizeOptionalValue(value: string): string | undefined {
  const normalizedValue = value.trim();
  return normalizedValue.length > 0 ? normalizedValue : undefined;
}

export function toCreateDashboardInvoiceRequest(
  form: AddInvoiceDialogFormGroup
): CreateDashboardInvoiceRequest {
  const value = form.getRawValue();

  return {
    supplierId: value.supplierId.trim(),
    invoiceNumber: value.invoiceNumber.trim(),
    date: value.date.trim(),
    iban: value.iban.trim(),
    paymentTerm: value.paymentTerm.trim(),
    totalValue: Number(value.totalValue),
    vatRate: value.vatRate,
    paymentMethod: value.paymentMethod.trim(),
    paymentDate: normalizeOptionalValue(value.paymentDate),
    paymentTime: normalizeOptionalValue(value.paymentTime)
  };
}
