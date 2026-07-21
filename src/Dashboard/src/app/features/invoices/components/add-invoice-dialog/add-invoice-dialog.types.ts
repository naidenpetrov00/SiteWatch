import { FormControl, FormGroup } from '@angular/forms';

export const PAYMENT_METHOD_OPTIONS = ['Bank', 'Cash'] as const;

export const ADD_INVOICE_VALIDATION_LIMITS = {
  supplierId: 36,
  invoiceNumber: 100,
  address: 500,
  email: 256,
  phoneNumber: 50,
  contactPerson: 200,
  iban: 34,
  paymentTerm: 100,
  totalValue: 18,
  paymentMethod: 100
} as const;

export interface AddInvoiceDialogFormControls {
  supplierId: FormControl<string>;
  invoiceNumber: FormControl<string>;
  date: FormControl<Date | null>;
  address: FormControl<string>;
  email: FormControl<string>;
  phoneNumber: FormControl<string>;
  contactPerson: FormControl<string>;
  iban: FormControl<string>;
  paymentTerm: FormControl<Date | null>;
  totalValue: FormControl<number | null>;
  vatRate: FormControl<number | null>;
  vatAmount: FormControl<string>;
  totalValueIncludingVat: FormControl<string>;
  paymentDate: FormControl<Date | null>;
  paymentTime: FormControl<string>;
  paymentMethod: FormControl<string>;
}

export type AddInvoiceDialogFormGroup = FormGroup<AddInvoiceDialogFormControls>;

export interface AddInvoiceSupplierDetails {
  address: string;
  email: string;
  phoneNumber: string;
  contactPerson: string;
}

export interface InvoiceSupplierValidationResult {
  details: AddInvoiceSupplierDetails | null;
  error: string | null;
}
