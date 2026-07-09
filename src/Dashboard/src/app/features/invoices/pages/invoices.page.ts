import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';

import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DataTableColumn } from '../../../shared/data-table/data-table.types';
import { InvoiceDialogComponent } from '../components/invoice-dialog/invoice-dialog.component';
import { DashboardInvoice } from '../models/dashboard-invoice.model';
import {
  formatInvoiceAmountValue,
  formatInvoiceDateTimeValue,
  formatInvoiceDateValue
} from '../utils/invoice-formatters';

const INVOICE_COLUMNS: readonly DataTableColumn<DashboardInvoice>[] = [
  {
    key: 'id',
    label: 'Id',
    sortable: true,
    cellType: 'button',
    filter: { kind: 'text', placeholder: 'Filter Id' }
  },
  {
    key: 'invoiceNumber',
    label: 'Invoice Number',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Invoice Number' }
  },
  {
    key: 'date',
    label: 'Date',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Date' },
    displayFormatter: (value) => formatInvoiceDateValue(value)
  },
  {
    key: 'supplierId',
    label: 'Supplier',
    sortable: true,
    cellType: 'button',
    valueAccessor: (invoice) => invoice.supplierDisplayLabel,
    filter: {
      kind: 'text',
      placeholder: 'Filter Supplier',
      valueAccessor: (invoice) => invoice.supplierDisplayLabel
    }
  },
  {
    key: 'eik',
    label: 'EIK',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter EIK' }
  },
  {
    key: 'address',
    label: 'Address',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Address' }
  },
  {
    key: 'email',
    label: 'Email',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Email' }
  },
  {
    key: 'phoneNumber',
    label: 'Phone Number',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Phone Number' }
  },
  {
    key: 'contactPerson',
    label: 'Contact Person',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Contact Person' }
  },
  {
    key: 'iban',
    label: 'IBAN',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter IBAN' }
  },
  {
    key: 'paymentTerm',
    label: 'Payment Term',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Payment Term' }
  },
  {
    key: 'totalValueExcludingVat',
    label: 'Total Excluding VAT',
    sortable: true,
    align: 'end',
    filter: { kind: 'number', placeholder: 'Filter Total Excluding VAT' },
    displayFormatter: (value) => formatInvoiceAmountValue(value)
  },
  {
    key: 'vat',
    label: 'VAT',
    sortable: true,
    align: 'end',
    filter: { kind: 'number', placeholder: 'Filter VAT' },
    displayFormatter: (value) => formatInvoiceAmountValue(value)
  },
  {
    key: 'totalValueIncludingVat',
    label: 'Total Including VAT',
    sortable: true,
    align: 'end',
    filter: { kind: 'number', placeholder: 'Filter Total Including VAT' },
    displayFormatter: (value) => formatInvoiceAmountValue(value)
  },
  {
    key: 'paymentDate',
    label: 'Payment Date',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Payment Date' },
    displayFormatter: (value) => formatInvoiceDateTimeValue(value)
  },
  {
    key: 'paymentTime',
    label: 'Payment Time',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Payment Time' },
    displayFormatter: (value) => formatInvoiceDateTimeValue(value)
  },
  {
    key: 'paymentMethod',
    label: 'Payment Method',
    sortable: true,
    filter: { kind: 'text', placeholder: 'Filter Payment Method' }
  }
] as const;

@Component({
  selector: 'app-invoices-page',
  imports: [ActionButtonComponent, DataTableComponent, MatDialogModule],
  templateUrl: './invoices.page.html',
  styleUrl: './invoices.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoicesPage {
  private readonly dialog = inject(MatDialog);

  readonly columns = INVOICE_COLUMNS;
  readonly invoices: readonly DashboardInvoice[] = [];
  readonly invoicesFilteredCount = 0;
  readonly invoicesTotalCount = 0;
  readonly pageSize = 50;
  readonly pageSizeOptions = [50, 100, 500, 1000] as const;

  onCellButtonClicked(event: { row: DashboardInvoice; column: DataTableColumn<DashboardInvoice> }): void {
    if (event.column.key !== 'id') {
      return;
    }

    this.dialog.open(InvoiceDialogComponent, {
      autoFocus: false,
      width: '72rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: event.row
    });
  }
}
