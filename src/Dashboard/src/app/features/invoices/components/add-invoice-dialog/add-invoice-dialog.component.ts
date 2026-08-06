import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, merge, startWith } from 'rxjs';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { provideNativeDateAdapter } from '@angular/material/core';

import { DashboardPersonsService } from '../../../persons/services/dashboard-persons.service';
import { DashboardPersonLookup } from '../../../persons/models/dashboard-person-lookup.model';
import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DatepickerComponent } from '../../../../shared/ui/datepicker/datepicker.component';
import { DashboardInvoicesService } from '../../services/dashboard-invoices.service';
import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardSiteLookup } from '../../../sites/models/dashboard-site-lookup.model';
import { toCreateDashboardInvoiceRequest } from '../../utils/create-dashboard-invoice-request.mapper';
import {
  ADD_INVOICE_VALIDATION_LIMITS,
  AddInvoiceDialogFormGroup,
  PAYMENT_METHOD_OPTIONS
} from './add-invoice-dialog.types';
import {
  decimalValidator,
  UUID_REGEX,
  positiveDecimalValidator,
  uuidValidator,
  timeValidator
} from './add-invoice-dialog.validators';
import {
  calculateInvoiceAmounts,
  formatInvoiceAmount,
  parseInvoiceDecimal
} from '../../utils/invoice-calculations';
import { deriveInvoiceSupplierDetails } from '../../utils/invoice-supplier-details';
import {
  formatDashboardPersonLookupLabel,
  formatDashboardPersonLookupSubtitle
} from '../../../persons/utils/dashboard-person-lookup-formatters';
import {
  InvoiceSiteAllocationFormGroup,
  calculateAssignedSiteAmount,
  createInvoiceSiteAllocationForm,
  createInvoiceSiteAllocationsFormArray,
  getInvoiceSiteAllocationErrorMessage
} from '../../utils/invoice-site-allocation-form';
import { SITE_PAYMENT_DIRECTIONS } from '../../models/invoice-site-allocation.model';

const MAX_INVOICE_FILE_SIZE = 20 * 1024 * 1024;
const ALLOWED_INVOICE_FILE_TYPES = new Set([
  'application/pdf',
  'image/jpeg',
  'image/png',
  'image/webp',
  'image/gif',
  'image/heic',
  'image/heif'
]);
const INVOICE_FILE_TYPES_BY_EXTENSION: Readonly<Record<string, string>> = {
  pdf: 'application/pdf',
  jpg: 'image/jpeg',
  jpeg: 'image/jpeg',
  png: 'image/png',
  webp: 'image/webp',
  gif: 'image/gif',
  heic: 'image/heic',
  heif: 'image/heif'
};

@Component({
  selector: 'app-add-invoice-dialog',
  imports: [
    ReactiveFormsModule,
    DialogActionBarComponent,
    DialogShellComponent,
    MatAutocompleteModule,
    DatepickerComponent,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
  ],
  templateUrl: './add-invoice-dialog.component.html',
  styleUrl: './add-invoice-dialog.component.css',
  providers: [provideNativeDateAdapter()],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddInvoiceDialogComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<AddInvoiceDialogComponent>);
  private readonly dashboardInvoicesService = inject(DashboardInvoicesService);
  private readonly dashboardPersonsService = inject(DashboardPersonsService);
  private readonly dashboardSitesService = inject(DashboardSitesService);

  private supplierSearchRevision = 0;
  private supplierDetailsRevision = 0;
  private siteSearchRevision = 0;

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = 'Add Invoice';
  readonly dialogSubtitle =
    'Search for a supplier by name, EGN, or EIK, then review the derived supplier details.';
  readonly paymentMethodOptions = PAYMENT_METHOD_OPTIONS;
  readonly validationLimits = ADD_INVOICE_VALIDATION_LIMITS;
  readonly supplierDetailsReady = signal(false);
  readonly supplierSearchResults = signal<readonly DashboardPersonLookup[]>([]);
  readonly supplierSearchControl = this.formBuilder.control<string | DashboardPersonLookup | null>('');
  readonly invoiceTotalIncludingVat = signal<number | null>(null);
  readonly assignedSiteAmount = signal(0);
  readonly remainingSiteAmount = computed(() =>
    Math.max((this.invoiceTotalIncludingVat() ?? 0) - this.assignedSiteAmount(), 0)
  );
  readonly siteSearchResults = signal<readonly DashboardSiteLookup[]>([]);
  readonly siteAllocationSaveError = signal<string | null>(null);
  readonly attachment = signal<File | null>(null);
  readonly attachmentError = signal<string | null>(null);
  readonly createdInvoiceId = signal<string | null>(null);
  readonly sitePaymentDirections = SITE_PAYMENT_DIRECTIONS;
  readonly invoiceForm = this.createInvoiceForm();
  readonly isCreatingInvoice = () => this.dashboardInvoicesService.createInvoiceMutation.isPending();
  readonly isSavingInvoice = () =>
    this.isCreatingInvoice() || this.dashboardInvoicesService.uploadInvoiceFileMutation.isPending();
  readonly formatInvoiceSupplierOptionLabel = formatDashboardPersonLookupLabel;
  readonly formatInvoiceSupplierOptionSubtitle = formatDashboardPersonLookupSubtitle;
  readonly formatSiteAmount = formatInvoiceAmount;
  readonly displaySupplierSearchValue = (
    value: string | DashboardPersonLookup | null
  ): string => {
    if (typeof value === 'string') {
      return value;
    }

    return value ? value.displayName : '';
  };
  readonly displaySiteSearchValue = (value: string | DashboardSiteLookup | null): string => {
    if (typeof value === 'string') {
      return value;
    }

    return value ? `#${value.numberId} ${value.name}` : '';
  };

  ngOnInit(): void {
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
        if (typeof value !== 'string') {
          return;
        }

        void this.onSupplierSearchTermChanged(value);
      });

    this.invoiceForm.controls.siteAllocations.valueChanges
      .pipe(
        startWith(this.invoiceForm.controls.siteAllocations.getRawValue()),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(() => this.updateSiteAllocationSummary());

    this.updateCalculatedAmounts();
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

  closeDialog(): void {
    this.dialogRef.close();
  }

  onAttachmentSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    this.attachmentError.set(null);

    if (!file) {
      this.attachment.set(null);
      return;
    }

    const extension = file.name.split('.').pop()?.toLowerCase() ?? '';
    const contentType = file.type || INVOICE_FILE_TYPES_BY_EXTENSION[extension] || '';
    if (!ALLOWED_INVOICE_FILE_TYPES.has(contentType)) {
      this.attachment.set(null);
      this.attachmentError.set('Choose a PDF or supported image file.');
      input.value = '';
      return;
    }

    if (file.size > MAX_INVOICE_FILE_SIZE) {
      this.attachment.set(null);
      this.attachmentError.set('The attachment cannot exceed 20 MB.');
      input.value = '';
      return;
    }

    this.attachment.set(
      file.type === contentType ? file : new File([file], file.name, { type: contentType })
    );
  }

  async submitInvoice(): Promise<void> {
    if (this.invoiceForm.invalid || !this.supplierDetailsReady()) {
      this.invoiceForm.markAllAsTouched();
      this.supplierSearchControl.markAsTouched();
      return;
    }

    this.siteAllocationSaveError.set(null);
    this.attachmentError.set(null);

    let invoiceId = this.createdInvoiceId();
    if (!invoiceId) {
      try {
        const request = toCreateDashboardInvoiceRequest(this.invoiceForm);
        const response = await this.dashboardInvoicesService.createInvoice(request);
        invoiceId = response.id;
        this.createdInvoiceId.set(invoiceId);
      } catch (error) {
        const supplierValidationMessage = this.getSupplierValidationMessage(error);
        if (supplierValidationMessage) {
          this.setSupplierSearchError('supplierDetailsIncomplete', supplierValidationMessage);
          this.supplierSearchControl.markAsTouched();
        } else {
          this.siteAllocationSaveError.set(
            getInvoiceSiteAllocationErrorMessage(error) ?? 'Unable to create the invoice.'
          );
        }
        return;
      }
    }

    const attachment = this.attachment();
    if (attachment) {
      try {
        await this.dashboardInvoicesService.uploadInvoiceFile(invoiceId, attachment);
      } catch {
        this.attachmentError.set(
          'The invoice was created, but its attachment could not be uploaded. Submit again to retry the attachment.'
        );
        return;
      }
    }

    this.dialogRef.close(true);
  }

  onSupplierSelected(event: MatAutocompleteSelectedEvent): void {
    const supplier = event.option.value as DashboardPersonLookup;

    this.supplierSearchRevision += 1;
    this.supplierSearchResults.set([]);
    this.supplierSearchControl.markAsTouched();
    this.invoiceForm.controls.supplierId.setValue(supplier.id, { emitEvent: false });
    void this.loadSupplierDetails(supplier.id);
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

  private getSupplierValidationMessage(error: unknown): string | null {
    if (!(error instanceof HttpErrorResponse)) {
      return null;
    }

    const details = error.error?.details as
      | readonly { field?: string; message?: string }[]
      | undefined;
    const supplierDetail = details?.find(
      (detail) => detail.field?.toLowerCase() === 'supplierid'
    );

    if (supplierDetail?.message) {
      return supplierDetail.message;
    }

    const errors = error.error?.errors as Record<string, readonly string[]> | undefined;
    const messages = Object.entries(errors ?? {}).find(
      ([field]) => field.toLowerCase() === 'supplierid'
    )?.[1];
    return messages?.find((message) => message.length > 0) ?? null;
  }

  private updateCalculatedAmounts(): void {
    const totalValue = parseInvoiceDecimal(this.invoiceForm.controls.totalValue.value);
    const vatRate = this.invoiceForm.controls.vatRate.value;

    if (
      totalValue === null ||
      vatRate === null ||
      !Number.isFinite(vatRate) ||
      vatRate < 0 ||
      vatRate > 100
    ) {
      this.invoiceForm.controls.vatAmount.setValue('', { emitEvent: false });
      this.invoiceForm.controls.totalValueIncludingVat.setValue('', { emitEvent: false });
      this.invoiceTotalIncludingVat.set(null);
      this.invoiceForm.controls.siteAllocations.updateValueAndValidity();
      return;
    }

    const { vatAmount, totalValueIncludingVat } = calculateInvoiceAmounts(totalValue, vatRate);

    this.invoiceForm.controls.vatAmount.setValue(formatInvoiceAmount(vatAmount), {
      emitEvent: false
    });
    this.invoiceForm.controls.totalValueIncludingVat.setValue(
      formatInvoiceAmount(totalValueIncludingVat),
      { emitEvent: false }
    );
    this.invoiceTotalIncludingVat.set(totalValueIncludingVat);
    this.invoiceForm.controls.siteAllocations.updateValueAndValidity();
    this.updateSiteAllocationSummary();
  }

  private registerSiteSearch(allocation: InvoiceSiteAllocationFormGroup): void {
    allocation.controls.siteSearch.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') {
          return;
        }

        allocation.controls.siteId.setValue('', { emitEvent: false });
        this.invoiceForm.controls.siteAllocations.updateValueAndValidity();
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
      calculateAssignedSiteAmount(this.invoiceForm.controls.siteAllocations)
    );
  }

  private createInvoiceForm(): AddInvoiceDialogFormGroup {
    return this.formBuilder.nonNullable.group({
      supplierId: this.formBuilder.nonNullable.control('', {
        validators: [Validators.required, uuidValidator()]
      }),
      invoiceNumber: this.formBuilder.nonNullable.control('', {
        validators: [Validators.required, Validators.maxLength(this.validationLimits.invoiceNumber)]
      }),
      date: this.formBuilder.control<Date | null>(null, {
        validators: [Validators.required]
      }),
      address: this.formBuilder.nonNullable.control({ value: '', disabled: true }),
      email: this.formBuilder.nonNullable.control({ value: '', disabled: true }),
      phoneNumber: this.formBuilder.nonNullable.control({ value: '', disabled: true }),
      contactPerson: this.formBuilder.nonNullable.control({ value: '', disabled: true }),
      iban: this.formBuilder.nonNullable.control({ value: '', disabled: true }, {
        validators: [Validators.maxLength(this.validationLimits.iban)]
      }),
      paymentTerm: this.formBuilder.control<Date | null>(null, {
        validators: [Validators.required]
      }),
      totalValue: this.formBuilder.control<number | null>(null, {
        validators: [Validators.required, decimalValidator(), positiveDecimalValidator()]
      }),
      vatRate: this.formBuilder.control<number | null>(20, {
        validators: [Validators.required, Validators.min(0), Validators.max(100)]
      }),
      vatAmount: this.formBuilder.nonNullable.control({ value: '', disabled: true }),
      totalValueIncludingVat: this.formBuilder.nonNullable.control({ value: '', disabled: true }),
      paymentDate: this.formBuilder.control<Date | null>(null),
      paymentTime: this.formBuilder.nonNullable.control('', {
        validators: [timeValidator()]
      }),
      paymentMethod: this.formBuilder.nonNullable.control('', {
        validators: [Validators.required]
      }),
      siteAllocations: createInvoiceSiteAllocationsFormArray(
        () => this.invoiceTotalIncludingVat()
      )
    });
  }
}
