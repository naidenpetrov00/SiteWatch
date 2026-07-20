import { FormControl, FormGroup } from '@angular/forms';

export const VAT_RATE_OPTIONS = [
  { value: 20, label: '20%' },
  { value: 9, label: '9%' },
  { value: 0, label: '0%' }
] as const satisfies readonly {
  value: number;
  label: string;
}[];

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
  date: FormControl<string>;
  address: FormControl<string>;
  email: FormControl<string>;
  phoneNumber: FormControl<string>;
  contactPerson: FormControl<string>;
  iban: FormControl<string>;
  paymentTerm: FormControl<string>;
  totalValue: FormControl<string>;
  vatRate: FormControl<number>;
  vatAmount: FormControl<string>;
  totalValueIncludingVat: FormControl<string>;
  paymentDate: FormControl<string>;
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
