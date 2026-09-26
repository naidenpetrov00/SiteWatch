import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked
} from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  AbstractControl,
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { DashboardRetailerLookup } from '../../../retailers/models/dashboard-retailer.models';
import { DashboardRetailersService } from '../../../retailers/services/dashboard-retailers.service';
import { RetailerPriceHistoryDialogComponent } from '../../../retailer-listings/components/retailer-price-history-dialog/retailer-price-history-dialog.component';
import { RetailerPriceHistoryDialogData } from '../../../retailer-listings/models/retailer-listing.models';
import {
  OfferPriceBasis,
  OfferPricingRequirement,
  OfferPricingProductRow,
  OfferRetailerPriceCell
} from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { getOfferError } from '../../utils/offer-error';
import {
  formatOfferQuantity,
  measurementUnitLabel,
  packageUnitLabel
} from '../../utils/offer-quantity';

type PriceCellForm = FormGroup<{
  amount: FormControl<number | null>;
  basis: FormControl<OfferPriceBasis>;
  productUrl: FormControl<string>;
  retailerProductCode: FormControl<string>;
}>;

interface PricingActivityDisplayRow {
  kind: 'activity';
  id: string;
  activityNumberId: number;
  activityName: string;
}

interface PricingSectionDisplayRow {
  kind: 'section';
  id: string;
  offerActivityId: string;
  sectionName: string | null;
}

interface PricingProductDisplayRow {
  kind: 'product';
  id: string;
  product: OfferPricingProductRow;
  requirement: OfferPricingRequirement;
  sharedRequirementCount: number;
}

type PricingDisplayRow =
  | PricingActivityDisplayRow
  | PricingSectionDisplayRow
  | PricingProductDisplayRow;
@Component({
  selector: 'app-offer-pricing-matrix',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatTableModule,
    MatTooltipModule,
    RouterLink
  ],
  templateUrl: './offer-pricing-matrix.component.html',
  styleUrl: './offer-pricing-matrix.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferPricingMatrixComponent {
  readonly offersService = inject(OffersService);
  private readonly retailersService = inject(DashboardRetailersService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialog = inject(MatDialog);
  private searchRevision = 0;
  private configuredIdentity: string | null = null;

  readonly siteId = input.required<string>();
  readonly offerId = input.required<string>();
  readonly matrix = computed(() => this.offersService.pricingMatrixQuery.data());
  readonly editable = computed(() => this.matrix()?.status === 'Draft');
  readonly displayedColumns = computed(() => [
    'product',
    ...(this.matrix()?.retailers.map((retailer) => retailer.retailerId) ?? [])
  ]);
  readonly displayRows = computed(() =>
    buildDisplayRows(this.matrix()?.products ?? [])
  );
  readonly groupColumns = ['group'];
  readonly isActivityGroup = (_index: number, row: PricingDisplayRow): boolean =>
    row.kind === 'activity';
  readonly isSectionGroup = (_index: number, row: PricingDisplayRow): boolean =>
    row.kind === 'section';
  readonly isProductRow = (_index: number, row: PricingDisplayRow): boolean =>
    row.kind === 'product';

  readonly forms = signal<ReadonlyMap<string, PriceCellForm>>(new Map());
  readonly editingLocations = signal<ReadonlyMap<string, string>>(new Map());
  readonly retailerResults = signal<readonly DashboardRetailerLookup[]>([]);
  readonly retailerSearch = this.formBuilder.control<
    string | DashboardRetailerLookup | null
  >('');
  readonly feedback = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly savingCellKey = signal<string | null>(null);
  readonly cellErrors = signal<ReadonlyMap<string, string>>(new Map());

  readonly isMutating = computed(
    () =>
      this.offersService.addPricingRetailerMutation.isPending() ||
      this.offersService.removePricingRetailerMutation.isPending() ||
      this.offersService.recordManualPriceMutation.isPending() ||
      this.offersService.selectProductPriceMutation.isPending() ||
      this.offersService.clearProductPriceMutation.isPending()
  );

  readonly displayRetailer = (
    value: string | DashboardRetailerLookup | null
  ): string => (typeof value === 'string' ? value : value?.displayName ?? '');

  constructor() {
    this.retailerSearch.valueChanges
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((value) => {
        if (typeof value === 'string') {
          void this.searchRetailers(value);
        }
      });

    effect(() => {
      const identity = `${this.siteId()}/${this.offerId()}`;
      const matrix = this.matrix();
      if (this.configuredIdentity !== identity) {
        this.forms.set(new Map());
        this.editingLocations.set(new Map());
        this.cellErrors.set(new Map());
        this.savingCellKey.set(null);
        this.configuredIdentity = identity;
      }
      if (matrix) {
        this.reconcileForms(matrix.products, matrix.status === 'Draft', untracked(this.forms));
      }
    });
  }

  async onRetailerSelected(event: MatAutocompleteSelectedEvent): Promise<void> {
    const retailer = event.option.value as DashboardRetailerLookup;
    this.searchRevision += 1;
    this.retailerResults.set([]);
    this.retailerSearch.setValue('', { emitEvent: false });
    this.clearFeedback();
    try {
      await this.offersService.addPricingRetailer({
        siteId: this.siteId(),
        offerId: this.offerId(),
        retailerId: retailer.id
      });
      this.feedback.set(`${retailer.displayName} added to the comparison.`);
    } catch (error) {
      this.error.set(getOfferError(error));
    }
  }

  async removeRetailer(retailerId: string, displayName: string): Promise<void> {
    if (!this.editable() || this.isMutating()) return;
    this.clearFeedback();
    try {
      await this.offersService.removePricingRetailer({
        siteId: this.siteId(),
        offerId: this.offerId(),
        retailerId
      });
      this.feedback.set(`${displayName} removed from the comparison.`);
    } catch (error) {
      this.error.set(getOfferError(error));
    }
  }

  startEdit(
    displayRow: PricingProductDisplayRow,
    cell: OfferRetailerPriceCell
  ): void {
    if (!this.canEditCell(cell)) return;
    const row = displayRow.product;
    const key = this.cellKey(row.offerProductLineId, cell.retailerId);
    this.clearCellError(key);
    this.editingLocations.update((current) => {
      const next = new Map(current);
      next.set(key, displayRow.id);
      return next;
    });
  }

  cancelEdit(
    displayRow: PricingProductDisplayRow,
    cell: OfferRetailerPriceCell
  ): void {
    const row = displayRow.product;
    const key = this.cellKey(row.offerProductLineId, cell.retailerId);
    this.clearCellError(key);
    this.resetForm(this.formFor(row, cell), row, cell, this.editable());
    this.editingLocations.update((current) => {
      const next = new Map(current);
      next.delete(key);
      return next;
    });
  }

  async savePrice(
    displayRow: PricingProductDisplayRow,
    cell: OfferRetailerPriceCell
  ): Promise<void> {
    const row = displayRow.product;
    const form = this.formFor(row, cell);
    if (!this.canEditCell(cell) || form.invalid || this.isMutating()) {
      form.markAllAsTouched();
      return;
    }

    this.clearFeedback();
    const key = this.cellKey(row.offerProductLineId, cell.retailerId);
    this.clearCellError(key);
    this.savingCellKey.set(key);
    const value = form.getRawValue();
    try {
      const savedCell = await this.offersService.recordManualPrice({
        siteId: this.siteId(),
        offerId: this.offerId(),
        offerProductLineId: row.offerProductLineId,
        productId: row.productId,
        retailerId: cell.retailerId,
        amount: value.amount!,
        basis: value.basis,
        productUrl: normalizeOptional(value.productUrl),
        retailerProductCode: normalizeOptional(value.retailerProductCode)
      });
      this.resetForm(form, row, savedCell, this.editable());
      this.editingLocations.update((current) => {
        const next = new Map(current);
        next.delete(key);
        return next;
      });
      this.feedback.set('Retailer price saved.');
    } catch (error) {
      this.cellErrors.update((current) => {
        const next = new Map(current);
        next.set(key, getOfferError(error));
        return next;
      });
    } finally {
      if (this.savingCellKey() === key) this.savingCellKey.set(null);
    }
  }

  async usePrice(
    row: OfferPricingProductRow,
    cell: OfferRetailerPriceCell
  ): Promise<void> {
    if (
      !this.canEditCell(cell) ||
      !cell.latestObservation ||
      this.isMutating()
    ) return;
    this.clearFeedback();
    try {
      await this.offersService.selectProductPrice({
        siteId: this.siteId(),
        offerId: this.offerId(),
        offerProductLineId: row.offerProductLineId,
        retailerId: cell.retailerId,
        observationId: cell.latestObservation.id
      });
      this.feedback.set(`Selected ${this.retailerName(cell.retailerId)} price.`);
    } catch (error) {
      this.error.set(getOfferError(error));
    }
  }

  async clearSelection(row: OfferPricingProductRow): Promise<void> {
    if (!this.editable() || !row.selectedPrice || this.isMutating()) return;
    this.clearFeedback();
    try {
      await this.offersService.clearProductPrice({
        siteId: this.siteId(),
        offerId: this.offerId(),
        offerProductLineId: row.offerProductLineId
      });
      this.feedback.set('Selected price cleared.');
    } catch (error) {
      this.error.set(getOfferError(error));
    }
  }

  openHistory(row: OfferPricingProductRow, cell: OfferRetailerPriceCell): void {
    this.dialog.open<
      RetailerPriceHistoryDialogComponent,
      RetailerPriceHistoryDialogData
    >(RetailerPriceHistoryDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      ariaLabel: `${this.retailerName(cell.retailerId)} price history for ${row.title}`,
      width: '42rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: {
        productId: row.productId,
        productLabel: `#${row.productNumberId} · ${row.title}`,
        retailerId: cell.retailerId,
        retailerName: this.retailerName(cell.retailerId)
      }
    });
  }

  formFor(row: OfferPricingProductRow, cell: OfferRetailerPriceCell): PriceCellForm {
    const form = this.forms().get(this.cellKey(row.offerProductLineId, cell.retailerId));
    if (!form) throw new Error('Missing Offer pricing cell form.');
    return form;
  }

  cellFor(row: OfferPricingProductRow, retailerId: string): OfferRetailerPriceCell {
    const cell = row.retailerPrices.find((item) => item.retailerId === retailerId);
    if (!cell) throw new Error('Missing Offer retailer price cell.');
    return cell;
  }

  isEditing(displayRow: PricingProductDisplayRow, retailerId: string): boolean {
    const key = this.cellKey(displayRow.product.offerProductLineId, retailerId);
    return this.editingLocations().get(key) === displayRow.id;
  }

  isEditingElsewhere(displayRow: PricingProductDisplayRow, retailerId: string): boolean {
    const key = this.cellKey(displayRow.product.offerProductLineId, retailerId);
    const location = this.editingLocations().get(key);
    return location !== undefined && location !== displayRow.id;
  }

  isSaving(row: OfferPricingProductRow, retailerId: string): boolean {
    return this.savingCellKey() === this.cellKey(row.offerProductLineId, retailerId);
  }

  cellError(row: OfferPricingProductRow, retailerId: string): string | null {
    return this.cellErrors().get(this.cellKey(row.offerProductLineId, retailerId)) ?? null;
  }

  isSelected(row: OfferPricingProductRow, retailerId: string): boolean {
    return row.selectedPrice?.retailerId === retailerId;
  }

  canEditCell(cell: OfferRetailerPriceCell): boolean {
    return (
      this.editable() &&
      cell.retailerListingIsActive !== false &&
      this.matrix()?.retailers.find(
        (retailer) => retailer.retailerId === cell.retailerId
      )?.isActive !== false
    );
  }

  canRemoveRetailer(retailerId: string): boolean {
    return !(this.matrix()?.products.some(
      (row) => row.selectedPrice?.retailerId === retailerId
    ) ?? false);
  }

  allowedBases(row: OfferPricingProductRow): readonly OfferPriceBasis[] {
    if (row.packageQuantity === null || row.packageUnit === null) return ['item'];
    return row.packageUnit === 'piece' ? ['item', 'package'] : ['package'];
  }

  productIdentity(row: OfferPricingProductRow): string {
    return [row.brand, row.model].filter(Boolean).join(' · ');
  }

  packageDescription(row: OfferPricingProductRow): string {
    if (row.packageQuantity === null || row.packageUnit === null) return 'Individual item';
    return `${formatOfferQuantity(row.packageQuantity)} ${packageUnitLabel(row.packageUnit)}`;
  }

  retailerName(retailerId: string): string {
    return this.matrix()?.retailers.find((item) => item.retailerId === retailerId)
      ?.displayName ?? 'Retailer';
  }

  formatQuantity = formatOfferQuantity;
  measurementUnit = measurementUnitLabel;

  formatCurrency(amount: number | null): string {
    return amount === null
      ? '—'
      : new Intl.NumberFormat(undefined, {
          style: 'currency',
          currency: 'EUR'
        }).format(amount);
  }

  relativeAge(value: string): string {
    const observed = new Date(value);
    const today = new Date();
    const observedDay = new Date(
      observed.getFullYear(),
      observed.getMonth(),
      observed.getDate()
    );
    const todayDay = new Date(today.getFullYear(), today.getMonth(), today.getDate());
    const days = Math.max(
      0,
      Math.round((todayDay.getTime() - observedDay.getTime()) / 86_400_000)
    );
    return new Intl.RelativeTimeFormat(undefined, { numeric: 'auto' }).format(
      -days,
      'day'
    );
  }

  exactDate(value: string): string {
    return new Intl.DateTimeFormat(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  private reconcileForms(
    rows: readonly OfferPricingProductRow[],
    editable: boolean,
    existing: ReadonlyMap<string, PriceCellForm>
  ): void {
    const next = new Map<string, PriceCellForm>();
    for (const row of rows) {
      for (const cell of row.retailerPrices) {
        const key = this.cellKey(row.offerProductLineId, cell.retailerId);
        const cellEditable = editable && this.canEditCell(cell);
        const form = existing.get(key) ?? this.createForm(row, cell, cellEditable);
        if (!form.dirty) this.resetForm(form, row, cell, cellEditable);
        else if (cellEditable) form.enable({ emitEvent: false });
        else form.disable({ emitEvent: false });
        next.set(key, form);
      }
    }
    this.forms.set(next);
  }

  private createForm(
    row: OfferPricingProductRow,
    cell: OfferRetailerPriceCell,
    editable: boolean
  ): PriceCellForm {
    const form = this.formBuilder.group({
      amount: this.formBuilder.control<number | null>(cell.latestObservation?.amount ?? null, [
        Validators.required,
        Validators.min(0.01),
        twoDecimalPlaces()
      ]),
      basis: this.formBuilder.nonNullable.control<OfferPriceBasis>(
        cell.latestObservation?.basis ?? this.allowedBases(row)[0],
        [Validators.required]
      ),
      productUrl: this.formBuilder.nonNullable.control(cell.productUrl ?? '', [
        Validators.maxLength(2048),
        optionalHttpUrl()
      ]),
      retailerProductCode: this.formBuilder.nonNullable.control(
        cell.retailerProductCode ?? '',
        [Validators.maxLength(100)]
      )
    });
    if (!editable) form.disable({ emitEvent: false });
    return form;
  }

  private resetForm(
    form: PriceCellForm,
    row: OfferPricingProductRow,
    cell: OfferRetailerPriceCell,
    editable: boolean
  ): void {
    const allowed = this.allowedBases(row);
    const currentBasis = cell.latestObservation?.basis;
    form.reset(
      {
        amount: cell.latestObservation?.amount ?? null,
        basis: currentBasis && allowed.includes(currentBasis) ? currentBasis : allowed[0],
        productUrl: cell.productUrl ?? '',
        retailerProductCode: cell.retailerProductCode ?? ''
      },
      { emitEvent: false }
    );
    if (editable) form.enable({ emitEvent: false });
    else form.disable({ emitEvent: false });
  }

  private async searchRetailers(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const revision = ++this.searchRevision;
    this.retailerResults.set([]);
    if (!searchTerm) return;
    try {
      const retailers = await this.retailersService.searchRetailers(searchTerm);
      if (revision !== this.searchRevision) return;
      const selectedIds = new Set(
        this.matrix()?.retailers.map((retailer) => retailer.retailerId) ?? []
      );
      this.retailerResults.set(
        retailers.filter((retailer) => !selectedIds.has(retailer.id))
      );
    } catch {
      if (revision === this.searchRevision) this.retailerResults.set([]);
    }
  }

  private cellKey(lineId: string, retailerId: string): string {
    return `${lineId}/${retailerId}`;
  }

  private clearCellError(key: string): void {
    if (!this.cellErrors().has(key)) return;
    this.cellErrors.update((current) => {
      const next = new Map(current);
      next.delete(key);
      return next;
    });
  }

  private clearFeedback(): void {
    this.feedback.set(null);
    this.error.set(null);
  }
}

function buildDisplayRows(
  products: readonly OfferPricingProductRow[]
): PricingDisplayRow[] {
  const occurrences = products.flatMap((product) =>
    product.requirements.map((requirement) => ({ product, requirement }))
  );
  occurrences.sort(
    (left, right) =>
      left.requirement.activitySortOrder - right.requirement.activitySortOrder ||
      left.requirement.activityNumberId - right.requirement.activityNumberId ||
      left.requirement.offerActivityId.localeCompare(
        right.requirement.offerActivityId
      ) ||
      left.requirement.sectionSortOrder - right.requirement.sectionSortOrder ||
      left.requirement.offerActivitySectionId.localeCompare(
        right.requirement.offerActivitySectionId
      ) ||
      left.requirement.sortOrder - right.requirement.sortOrder ||
      left.product.productNumberId - right.product.productNumberId ||
      left.requirement.id.localeCompare(right.requirement.id)
  );

  const counts = new Map<string, number>();
  for (const occurrence of occurrences) {
    counts.set(
      occurrence.product.offerProductLineId,
      (counts.get(occurrence.product.offerProductLineId) ?? 0) + 1
    );
  }

  const rows: PricingDisplayRow[] = [];
  let activityId: string | null = null;
  let sectionId: string | null = null;
  for (const occurrence of occurrences) {
    const { product, requirement } = occurrence;
    if (activityId !== requirement.offerActivityId) {
      activityId = requirement.offerActivityId;
      sectionId = null;
      rows.push({
        kind: 'activity',
        id: `activity/${requirement.offerActivityId}`,
        activityNumberId: requirement.activityNumberId,
        activityName: requirement.activityName
      });
    }

    if (sectionId !== requirement.offerActivitySectionId) {
      sectionId = requirement.offerActivitySectionId;
      rows.push({
        kind: 'section',
        id: `section/${requirement.offerActivitySectionId}`,
        offerActivityId: requirement.offerActivityId,
        sectionName: requirement.sectionName
      });
    }

    rows.push({
      kind: 'product',
      id: `requirement/${requirement.id}`,
      product,
      requirement,
      sharedRequirementCount: counts.get(product.offerProductLineId) ?? 1
    });
  }

  return rows;
}

function normalizeOptional(value: string): string | null {
  const normalized = value.trim();
  return normalized ? normalized : null;
}

function twoDecimalPlaces(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = Number(control.value);
    if (!Number.isFinite(value)) return { decimalScale: true };
    return Math.abs(value * 100 - Math.round(value * 100)) < 0.000001
      ? null
      : { decimalScale: true };
  };
}

function optionalHttpUrl(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = String(control.value ?? '').trim();
    if (!value) return null;
    try {
      const url = new URL(value);
      return (url.protocol === 'http:' || url.protocol === 'https:') &&
        !url.username &&
        !url.password &&
        !url.hash
        ? null
        : { productUrl: true };
    } catch {
      return { productUrl: true };
    }
  };
}
