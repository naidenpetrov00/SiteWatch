import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  signal
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, merge, startWith } from 'rxjs';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { provideNativeDateAdapter } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DatepickerComponent } from '../../../../shared/ui/datepicker/datepicker.component';
import { DashboardPersonsService } from '../../../persons/services/dashboard-persons.service';
import { DashboardPersonLookup } from '../../../persons/models/dashboard-person-lookup.model';
import {
  formatDashboardPersonLookupLabel,
  formatDashboardPersonLookupSubtitle
} from '../../../persons/utils/dashboard-person-lookup-formatters';
import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardSiteLookup } from '../../../sites/models/dashboard-site-lookup.model';
import { DashboardInvoice } from '../../models/dashboard-invoice.model';
import { SITE_PAYMENT_DIRECTIONS } from '../../models/invoice-site-allocation.model';
import { DashboardInvoicesService } from '../../services/dashboard-invoices.service';
import { toCreateDashboardInvoiceRequest } from '../../utils/create-dashboard-invoice-request.mapper';
import {
  InvoiceSiteAllocationFormGroup,
  calculateAssignedSiteAmount,
  createInvoiceSiteAllocationForm,
  createInvoiceSiteAllocationsFormArray,
  getInvoiceSiteAllocationErrorMessage
} from '../../utils/invoice-site-allocation-form';
import {
  calculateInvoiceAmounts,
  formatInvoiceAmount,
  parseInvoiceDecimal
} from '../../utils/invoice-calculations';
import { deriveInvoiceSupplierDetails } from '../../utils/invoice-supplier-details';
import {
  ADD_INVOICE_VALIDATION_LIMITS,
  AddInvoiceDialogFormGroup,
  PAYMENT_METHOD_OPTIONS
} from '../add-invoice-dialog/add-invoice-dialog.types';
import {
  decimalValidator,
  positiveDecimalValidator,
  timeValidator,
  UUID_REGEX,
  uuidValidator
} from '../add-invoice-dialog/add-invoice-dialog.validators';

@Component({
  selector: 'app-invoice-dialog',
  imports: [
    ReactiveFormsModule,
    DialogActionBarComponent,
    DialogShellComponent,
    DatepickerComponent,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  templateUrl: './invoice-dialog.component.html',
  styleUrl: './invoice-dialog.component.css',
  providers: [provideNativeDateAdapter()],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoiceDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<InvoiceDialogComponent>);
  private readonly invoice = inject(MAT_DIALOG_DATA) as DashboardInvoice;
  private readonly dashboardInvoicesService = inject(DashboardInvoicesService);
  private readonly dashboardPersonsService = inject(DashboardPersonsService);
  private readonly dashboardSitesService = inject(DashboardSitesService);

  private supplierSearchRevision = 0;
  private supplierDetailsRevision = 0;
  private siteSearchRevision = 0;

  readonly dialogEyebrow = 'Invoices';
  readonly dialogTitle = this.invoice.invoiceNumber ?? `Invoice #${this.invoice.numberId}`;
  readonly submittedFromSiteName = this.invoice.submittedFromSiteName;
  readonly dialogSubtitle = this.invoice.isComplete
    ? 'Modify the invoice details and site allocations.'
    : 'Complete the missing invoice details.';
  readonly submitLabel = this.invoice.isComplete ? 'Save' : 'Complete Invoice';
  readonly validationLimits = ADD_INVOICE_VALIDATION_LIMITS;
  readonly paymentMethodOptions = PAYMENT_METHOD_OPTIONS;
  readonly sitePaymentDirections = SITE_PAYMENT_DIRECTIONS;
  readonly supplierDetailsReady = signal(this.invoice.supplierId !== null);
  readonly supplierSearchResults = signal<readonly DashboardPersonLookup[]>([]);
  readonly supplierSearchControl = this.formBuilder.control<string | DashboardPersonLookup | null>(
    this.invoice.supplierDisplayLabel ?? ''
  );
  readonly invoiceTotalIncludingVat = signal<number | null>(
    this.invoice.totalValueIncludingVat
  );
  readonly assignedSiteAmount = signal(0);
  readonly remainingSiteAmount = computed(() =>
    Math.max((this.invoiceTotalIncludingVat() ?? 0) - this.assignedSiteAmount(), 0)
  );
  readonly siteSearchResults = signal<readonly DashboardSiteLookup[]>([]);
  readonly saveError = signal<string | null>(null);
  readonly invoiceForm = this.createInvoiceForm();
  readonly isSaving = () => this.dashboardInvoicesService.updateInvoiceMutation.isPending();
  readonly formatInvoiceSupplierOptionLabel = formatDashboardPersonLookupLabel;
  readonly formatInvoiceSupplierOptionSubtitle = formatDashboardPersonLookupSubtitle;
  readonly formatSiteAmount = formatInvoiceAmount;
  readonly displaySupplierSearchValue = (
    value: string | DashboardPersonLookup | null
  ): string => typeof value === 'string' ? value : value?.displayName ?? '';
  readonly displaySiteSearchValue = (
    value: string | DashboardSiteLookup | null
  ): string => typeof value === 'string' ? value : value ? `#${value.numberId} ${value.name}` : '';

  constructor() {
    const totalValueControl = this.invoiceForm.controls.totalValue;
    const vatRateControl = this.invoiceForm.controls.vatRate;
    merge(
      totalValueControl.valueChanges.pipe(startWith(totalValueControl.value)),
      vatRateControl.valueChanges.pipe(startWith(vatRateControl.value))
    )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.updateCalculatedAmounts());

    this.supplierSearchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') {
          void this.onSupplierSearchTermChanged(value);
        }
      });

    for (const allocation of this.invoiceForm.controls.siteAllocations.controls) {
      this.registerSiteSearch(allocation);
    }

    this.invoiceForm.controls.siteAllocations.valueChanges
      .pipe(
        startWith(this.invoiceForm.controls.siteAllocations.getRawValue()),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.updateSiteAllocationSummary());
    this.updateCalculatedAmounts();
  }

  onSupplierSelected(event: MatAutocompleteSelectedEvent): void {
    const supplier = event.option.value as DashboardPersonLookup;
    this.supplierSearchRevision += 1;
    this.supplierSearchResults.set([]);
    this.supplierSearchControl.markAsTouched();
    this.invoiceForm.controls.supplierId.setValue(supplier.id, { emitEvent: false });
    void this.loadSupplierDetails(supplier.id);
  }

  addSiteAllocation(): void {
    const allocation = createInvoiceSiteAllocationForm(this.formBuilder);
    this.registerSiteSearch(allocation);
    this.invoiceForm.controls.siteAllocations.push(allocation);
  }

  removeSiteAllocation(index: number): void {
    this.invoiceForm.controls.siteAllocations.removeAt(index);
    this.invoiceForm.controls.siteAllocations.markAsTouched();
  }

  onSiteSelected(
    allocation: InvoiceSiteAllocationFormGroup,
    event: MatAutocompleteSelectedEvent
  ): void {
    const site = event.option.value as DashboardSiteLookup;
    allocation.controls.siteId.setValue(site.id);
    allocation.controls.siteSearch.setValue(site, { emitEvent: false });
    this.siteSearchResults.set([]);
    this.invoiceForm.controls.siteAllocations.markAsTouched();
    this.invoiceForm.controls.siteAllocations.updateValueAndValidity();
  }

  async saveInvoice(): Promise<void> {
    if (this.invoiceForm.invalid || !this.supplierDetailsReady()) {
      this.invoiceForm.markAllAsTouched();
      this.supplierSearchControl.markAsTouched();
      return;
    }

    this.saveError.set(null);
    try {
      await this.dashboardInvoicesService.updateInvoice({
        invoiceId: this.invoice.id,
        ...toCreateDashboardInvoiceRequest(this.invoiceForm)
      });
      this.dialogRef.close(true);
    } catch (error) {
      this.saveError.set(
        this.getSupplierValidationMessage(error)
        ?? getInvoiceSiteAllocationErrorMessage(error)
        ?? 'Unable to save the invoice.'
      );
    }
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  private async onSupplierSearchTermChanged(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();

    this.invalidateSupplierDetailsLookup();
    this.clearSelectedSupplier();
    this.clearSupplierSearchErrors();

    const searchRevision = ++this.supplierSearchRevision;
    this.supplierSearchResults.set([]);

    if (searchTerm.length === 0) {
      return;
    }

    try {
      const suppliers = await this.dashboardPersonsService.searchSuppliers(searchTerm);

      if (searchRevision !== this.supplierSearchRevision) {
        return;
      }

      this.supplierSearchResults.set(suppliers);
    } catch {
      if (searchRevision !== this.supplierSearchRevision) {
        return;
      }

      this.supplierSearchResults.set([]);
    }
  }

  private async loadSupplierDetails(supplierId: string): Promise<void> {
    const lookupRevision = this.nextSupplierDetailsRevision();

    this.supplierDetailsReady.set(false);
    this.clearSupplierDetails();

    if (!UUID_REGEX.test(supplierId)) {
      this.clearSelectedSupplier();
      this.setSupplierSearchError('supplierNotFound', true);
      return;
    }

    try {
      const supplier = await this.dashboardPersonsService.getPersonById(supplierId);

      if (lookupRevision !== this.supplierDetailsRevision) {
        return;
      }

      const result = deriveInvoiceSupplierDetails(supplier);

      if (!result.details) {
        this.clearSelectedSupplier();
        this.setSupplierSearchError('supplierDetailsIncomplete', result.error ?? true);
        return;
      }

      this.clearSupplierSearchErrors();
      this.invoiceForm.controls.supplierId.setValue(supplier.id, { emitEvent: false });
      this.invoiceForm.controls.address.setValue(result.details.address, { emitEvent: false });
      this.invoiceForm.controls.email.setValue(result.details.email, { emitEvent: false });
      this.invoiceForm.controls.phoneNumber.setValue(result.details.phoneNumber, { emitEvent: false });
      this.invoiceForm.controls.contactPerson.setValue(result.details.contactPerson, {
        emitEvent: false
      });
      this.invoiceForm.controls.iban.setValue(result.details.iban, { emitEvent: false });
      this.supplierDetailsReady.set(true);
      this.saveError.set(null);
    } catch (error) {
      if (lookupRevision !== this.supplierDetailsRevision) {
        return;
      }

      this.clearSelectedSupplier();
      const supplierValidationMessage = this.getSupplierValidationMessage(error);
      this.setSupplierSearchError(
        supplierValidationMessage ? 'supplierDetailsIncomplete' : 'supplierNotFound',
        supplierValidationMessage ?? true
      );
    }
  }

  private clearSelectedSupplier(): void {
    this.invoiceForm.controls.supplierId.setValue('', { emitEvent: false });
    this.clearSupplierDetails();
    this.supplierDetailsReady.set(false);
  }

  private invalidateSupplierDetailsLookup(): void {
    this.supplierDetailsRevision += 1;
  }

  private nextSupplierDetailsRevision(): number {
    this.supplierDetailsRevision += 1;
    return this.supplierDetailsRevision;
  }

  private clearSupplierDetails(): void {
    this.invoiceForm.controls.address.setValue('', { emitEvent: false });
    this.invoiceForm.controls.email.setValue('', { emitEvent: false });
    this.invoiceForm.controls.phoneNumber.setValue('', { emitEvent: false });
    this.invoiceForm.controls.contactPerson.setValue('', { emitEvent: false });
    this.invoiceForm.controls.iban.setValue('', { emitEvent: false });
  }

  private clearSupplierSearchErrors(): void {
    this.setSupplierSearchError('supplierNotFound', false);
    this.setSupplierSearchError('supplierDetailsIncomplete', false);
  }

  private setSupplierSearchError(errorKey: string, errorValue: boolean | string): void {
    const supplierSearchControl = this.supplierSearchControl;
    const nextErrors = { ...(supplierSearchControl.errors ?? {}) };

    if (errorValue) {
      nextErrors[errorKey] = errorValue;
    } else {
      delete nextErrors[errorKey];
    }

    supplierSearchControl.setErrors(Object.keys(nextErrors).length > 0 ? nextErrors : null);
  }

  private registerSiteSearch(allocation: InvoiceSiteAllocationFormGroup): void {
    allocation.controls.siteSearch.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') return;
        allocation.controls.siteId.setValue('', { emitEvent: false });
        this.invoiceForm.controls.siteAllocations.updateValueAndValidity();
        void this.searchSites(value);
      });
  }

  private async searchSites(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const revision = ++this.siteSearchRevision;
    this.siteSearchResults.set([]);
    if (!searchTerm) return;

    try {
      const sites = await this.dashboardSitesService.searchSites(searchTerm);
      if (revision === this.siteSearchRevision) this.siteSearchResults.set(sites);
    } catch {
      if (revision === this.siteSearchRevision) this.siteSearchResults.set([]);
    }
  }

  private updateCalculatedAmounts(): void {
    const totalValue = parseInvoiceDecimal(this.invoiceForm.controls.totalValue.value);
    const vatRate = this.invoiceForm.controls.vatRate.value;
    if (totalValue === null || vatRate === null || vatRate < 0 || vatRate > 100) {
      this.invoiceForm.controls.vatAmount.setValue('', { emitEvent: false });
      this.invoiceForm.controls.totalValueIncludingVat.setValue('', { emitEvent: false });
      this.invoiceTotalIncludingVat.set(null);
      return;
    }

    const amounts = calculateInvoiceAmounts(totalValue, vatRate);
    this.invoiceForm.controls.vatAmount.setValue(formatInvoiceAmount(amounts.vatAmount), { emitEvent: false });
    this.invoiceForm.controls.totalValueIncludingVat.setValue(
      formatInvoiceAmount(amounts.totalValueIncludingVat),
      { emitEvent: false }
    );
    this.invoiceTotalIncludingVat.set(amounts.totalValueIncludingVat);
    this.invoiceForm.controls.siteAllocations.updateValueAndValidity();
  }

  private updateSiteAllocationSummary(): void {
    this.assignedSiteAmount.set(
      calculateAssignedSiteAmount(this.invoiceForm.controls.siteAllocations)
    );
  }

  private getSupplierValidationMessage(error: unknown): string | null {
    if (!(error instanceof HttpErrorResponse)) return null;
    const details = error.error?.details as readonly { field?: string; message?: string }[] | undefined;
    return details?.find((detail) => detail.field?.toLowerCase() === 'supplierid')?.message ?? null;
  }

  private createInvoiceForm(): AddInvoiceDialogFormGroup {
    const total = this.invoice.totalValueExcludingVat;
    const vatRate = this.invoice.vatRate ?? 20;

    return this.formBuilder.nonNullable.group({
      supplierId: this.formBuilder.nonNullable.control(this.invoice.supplierId ?? '', {
        validators: [Validators.required, uuidValidator()]
      }),
      invoiceNumber: this.formBuilder.nonNullable.control(this.invoice.invoiceNumber ?? '', {
        validators: [Validators.required, Validators.maxLength(this.validationLimits.invoiceNumber)]
      }),
      date: this.formBuilder.control<Date | null>(this.toDate(this.invoice.date), {
        validators: [Validators.required]
      }),
      address: this.formBuilder.nonNullable.control({ value: this.invoice.address ?? '', disabled: true }),
      email: this.formBuilder.nonNullable.control({ value: this.invoice.email ?? '', disabled: true }),
      phoneNumber: this.formBuilder.nonNullable.control({ value: this.invoice.phoneNumber ?? '', disabled: true }),
      contactPerson: this.formBuilder.nonNullable.control({ value: this.invoice.contactPerson ?? '', disabled: true }),
      iban: this.formBuilder.nonNullable.control({ value: '', disabled: true }),
      paymentTerm: this.formBuilder.control<Date | null>(this.toDate(this.invoice.paymentTerm), {
        validators: [Validators.required]
      }),
      totalValue: this.formBuilder.control<number | null>(total, {
        validators: [Validators.required, decimalValidator(), positiveDecimalValidator()]
      }),
      vatRate: this.formBuilder.control<number | null>(vatRate, {
        validators: [Validators.required, Validators.min(0), Validators.max(100)]
      }),
      vatAmount: this.formBuilder.nonNullable.control({ value: '', disabled: true }),
      totalValueIncludingVat: this.formBuilder.nonNullable.control({ value: '', disabled: true }),
      paymentDate: this.formBuilder.control<Date | null>(this.toDate(this.invoice.paymentDate)),
      paymentTime: this.formBuilder.nonNullable.control(this.toTime(this.invoice.paymentTime), {
        validators: [timeValidator()]
      }),
      paymentMethod: this.formBuilder.nonNullable.control(this.invoice.paymentMethod ?? '', {
        validators: [Validators.required]
      }),
      siteAllocations: createInvoiceSiteAllocationsFormArray(
        () => this.invoiceTotalIncludingVat(),
        this.invoice.siteAllocations.map((allocation) =>
          createInvoiceSiteAllocationForm(this.formBuilder, allocation)
        )
      )
    });
  }

  private toDate(value: string | null): Date | null {
    if (!value) return null;
    const date = new Date(value);
    return Number.isNaN(date.getTime()) ? null : date;
  }

  private toTime(value: string | null): string {
    if (!value) return '';
    const date = new Date(value);
    return Number.isNaN(date.getTime())
      ? ''
      : `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
  }
}
