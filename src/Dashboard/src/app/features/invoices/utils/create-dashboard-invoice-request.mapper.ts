import { AddInvoiceDialogFormGroup } from '../components/add-invoice-dialog/add-invoice-dialog.types';
import { CreateDashboardInvoiceRequest } from '../models/create-dashboard-invoice-request.model';

function formatLocalDate(value: Date | null): string {
  if (!value) {
    return '';
  }

  const year = value.getFullYear();
  const month = String(value.getMonth() + 1).padStart(2, '0');
  const day = String(value.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

export function toCreateDashboardInvoiceRequest(
  form: AddInvoiceDialogFormGroup
): CreateDashboardInvoiceRequest {
  const value = form.getRawValue();

  return {
    supplierId: value.supplierId.trim(),
    invoiceNumber: value.invoiceNumber.trim(),
    date: formatLocalDate(value.date),
    paymentTerm: formatLocalDate(value.paymentTerm),
    totalValue: Number(value.totalValue),
    vatRate: value.vatRate ?? 0,
    paymentMethod: value.paymentMethod.trim(),
    paymentDate: value.paymentDate ? formatLocalDate(value.paymentDate) : undefined,
    paymentTime:
      value.paymentDate && value.paymentTime.trim().length > 0
        ? `${formatLocalDate(value.paymentDate)}T${value.paymentTime.trim()}:00`
        : undefined
  };
}
