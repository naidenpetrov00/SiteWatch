import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, startWith } from 'rxjs';
import {
  MAT_DIALOG_DATA,
  MatDialogRef
} from '@angular/material/dialog';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardSiteLookup } from '../../../sites/models/dashboard-site-lookup.model';
import { DashboardInvoice } from '../../models/dashboard-invoice.model';
import { SITE_PAYMENT_DIRECTIONS } from '../../models/invoice-site-allocation.model';
import { DashboardInvoicesService } from '../../services/dashboard-invoices.service';
import {
  InvoiceSiteAllocationFormGroup,
  calculateAssignedSiteAmount,
  createInvoiceSiteAllocationForm,
  createInvoiceSiteAllocationsFormArray,
  getInvoiceSiteAllocationErrorMessage,
  toInvoiceSiteAllocationRequests
} from '../../utils/invoice-site-allocation-form';
import { formatInvoiceAmount } from '../../utils/invoice-calculations';
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
  imports: [
    ReactiveFormsModule,
    DialogActionBarComponent,
    DialogShellComponent,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  templateUrl: './invoice-dialog.component.html',
  styleUrl: './invoice-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoiceDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<InvoiceDialogComponent>);
  private readonly invoice = inject(MAT_DIALOG_DATA) as DashboardInvoice;
  private readonly dashboardInvoicesService = inject(DashboardInvoicesService);
  private readonly dashboardSitesService = inject(DashboardSitesService);

  private siteSearchRevision = 0;

  readonly dialogEyebrow = 'Invoices';
  readonly dialogTitle = `Invoice ${this.invoice.invoiceNumber}`;
  readonly dialogSubtitle = 'Invoice details are read-only. Site allocations can be edited.';
  readonly invoiceTotalIncludingVat = signal(this.invoice.totalValueIncludingVat);
  readonly assignedSiteAmount = signal(0);
  readonly remainingSiteAmount = computed(() =>
    Math.max(this.invoiceTotalIncludingVat() - this.assignedSiteAmount(), 0)
  );
  readonly siteSearchResults = signal<readonly DashboardSiteLookup[]>([]);
  readonly siteAllocationSaveError = signal<string | null>(null);
  readonly sitePaymentDirections = SITE_PAYMENT_DIRECTIONS;
  readonly allocationForm = this.formBuilder.group({
    siteAllocations: createInvoiceSiteAllocationsFormArray(
      () => this.invoiceTotalIncludingVat(),
      (this.invoice.siteAllocations ?? []).map((allocation) =>
        createInvoiceSiteAllocationForm(this.formBuilder, allocation)
      )
    )
  });
  readonly isUpdatingSiteAllocations = () =>
    this.dashboardInvoicesService.updateSiteAllocationsMutation.isPending();
  readonly formatSiteAmount = formatInvoiceAmount;
  readonly displaySiteSearchValue = (value: string | DashboardSiteLookup | null): string => {
    if (typeof value === 'string') {
      return value;
    }

    return value ? `#${value.numberId} ${value.name}` : '';
  };
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

  constructor() {
    for (const allocation of this.allocationForm.controls.siteAllocations.controls) {
      this.registerSiteSearch(allocation);
    }

    this.allocationForm.controls.siteAllocations.valueChanges
      .pipe(
        startWith(this.allocationForm.controls.siteAllocations.getRawValue()),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.updateSiteAllocationSummary());
  }

  addSiteAllocation(): void {
    const allocation = createInvoiceSiteAllocationForm(this.formBuilder);
    this.registerSiteSearch(allocation);
    this.allocationForm.controls.siteAllocations.push(allocation);
  }

  removeSiteAllocation(index: number): void {
    this.allocationForm.controls.siteAllocations.removeAt(index);
    this.allocationForm.controls.siteAllocations.markAsTouched();
  }

  onSiteSelected(
    allocation: InvoiceSiteAllocationFormGroup,
    event: MatAutocompleteSelectedEvent
  ): void {
    const site = event.option.value as DashboardSiteLookup;
    allocation.controls.siteId.setValue(site.id);
    allocation.controls.siteSearch.setValue(site, { emitEvent: false });
    this.siteSearchResults.set([]);
    this.allocationForm.controls.siteAllocations.markAsTouched();
    this.allocationForm.controls.siteAllocations.updateValueAndValidity();
  }

  async saveSiteAllocations(): Promise<void> {
    if (this.allocationForm.invalid) {
      this.allocationForm.markAllAsTouched();
      return;
    }

    this.siteAllocationSaveError.set(null);
    try {
      await this.dashboardInvoicesService.updateSiteAllocations({
        invoiceId: this.invoice.id,
        siteAllocations: toInvoiceSiteAllocationRequests(
          this.allocationForm.controls.siteAllocations
        )
      });
      this.dialogRef.close(true);
    } catch (error) {
      this.siteAllocationSaveError.set(
        getInvoiceSiteAllocationErrorMessage(error) ?? 'Unable to save site allocations.'
      );
    }
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  private registerSiteSearch(allocation: InvoiceSiteAllocationFormGroup): void {
    allocation.controls.siteSearch.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') {
          return;
        }

        allocation.controls.siteId.setValue('', { emitEvent: false });
        this.allocationForm.controls.siteAllocations.updateValueAndValidity();
        void this.searchSites(value);
      });
  }

  private async searchSites(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const searchRevision = ++this.siteSearchRevision;
    this.siteSearchResults.set([]);

    if (searchTerm.length === 0) {
      return;
    }

    try {
      const sites = await this.dashboardSitesService.searchSites(searchTerm);
      if (searchRevision === this.siteSearchRevision) {
        this.siteSearchResults.set(sites);
      }
    } catch {
      if (searchRevision === this.siteSearchRevision) {
        this.siteSearchResults.set([]);
      }
    }
  }

  private updateSiteAllocationSummary(): void {
    this.assignedSiteAmount.set(
      calculateAssignedSiteAmount(this.allocationForm.controls.siteAllocations)
    );
  }
}
