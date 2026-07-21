import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardInvoice } from '../../models/dashboard-invoice.model';
import {
  formatInvoiceAmountValue,
  formatInvoiceDateTimeValue,
  formatInvoiceDateValue,
  formatInvoiceTextValue
} from '../../utils/invoice-formatters';

interface InvoiceDetailItem {
  label: string;
  value: string;
}

interface InvoiceDetailSection {
  title: string;
  items: readonly InvoiceDetailItem[];
}

@Component({
  selector: 'app-invoice-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent],
  templateUrl: './invoice-dialog.component.html',
  styleUrl: './invoice-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoiceDialogComponent {
  private readonly dialogRef = inject(MatDialogRef<InvoiceDialogComponent>);
  private readonly invoice = inject(MAT_DIALOG_DATA) as DashboardInvoice;

  readonly dialogEyebrow = 'Invoices';
  readonly dialogTitle = `Invoice ${this.invoice.invoiceNumber}`;
  readonly dialogSubtitle = 'Readonly invoice details.';
  readonly detailSections: readonly InvoiceDetailSection[] = [
    {
      title: 'Invoice',
      items: [
        { label: 'NumberId', value: this.invoice.numberId.toString() },
        { label: 'Id', value: this.invoice.id },
        { label: 'Invoice Number', value: this.invoice.invoiceNumber },
        { label: 'Date', value: formatInvoiceDateValue(this.invoice.date) }
      ]
    },
    {
      title: 'Supplier',
      items: [
        { label: 'Supplier Id', value: this.invoice.supplierId },
        { label: 'Supplier', value: formatInvoiceTextValue(this.invoice.supplierDisplayLabel) },
        { label: 'Tax Identifier', value: formatInvoiceTextValue(this.invoice.taxIdentifier) },
        { label: 'Address', value: formatInvoiceTextValue(this.invoice.address) }
      ]
    },
    {
      title: 'Contact',
      items: [
        { label: 'Email', value: formatInvoiceTextValue(this.invoice.email) },
        { label: 'Phone Number', value: formatInvoiceTextValue(this.invoice.phoneNumber) },
        { label: 'Contact Person', value: formatInvoiceTextValue(this.invoice.contactPerson) }
      ]
    },
    {
      title: 'Payment',
      items: [
        { label: 'IBAN', value: formatInvoiceTextValue(this.invoice.iban) },
        { label: 'Payment Term', value: formatInvoiceDateValue(this.invoice.paymentTerm) },
        {
          label: 'Total Value Excluding VAT',
          value: formatInvoiceAmountValue(this.invoice.totalValueExcludingVat)
        },
        { label: 'VAT', value: formatInvoiceAmountValue(this.invoice.vat) },
        {
          label: 'Total Value Including VAT',
          value: formatInvoiceAmountValue(this.invoice.totalValueIncludingVat)
        },
        { label: 'Payment Date', value: formatInvoiceDateTimeValue(this.invoice.paymentDate) },
        { label: 'Payment Time', value: formatInvoiceDateTimeValue(this.invoice.paymentTime) },
        { label: 'Payment Method', value: formatInvoiceTextValue(this.invoice.paymentMethod) }
      ]
    }
  ];

  closeDialog(): void {
    this.dialogRef.close();
  }
}
