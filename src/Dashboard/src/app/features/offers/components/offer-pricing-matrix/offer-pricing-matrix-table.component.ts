import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';

import {
  OfferPriceBasis,
  OfferPricingMatrix,
  OfferPricingProductRow,
  OfferRetailerPriceCell
} from '../../models/offer.models';
import { formatOfferQuantity, measurementUnitLabel, packageUnitLabel } from '../../utils/offer-quantity';
import {
  PriceCellForm,
  PricingCellAction,
  PricingDisplayRow,
  PricingEditAction,
  PricingProductDisplayRow
} from './offer-pricing-matrix.types';

@Component({
  selector: 'app-offer-pricing-matrix-table',
  imports: [ReactiveFormsModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule, MatSelectModule, MatTableModule, MatTooltipModule, RouterLink],
  templateUrl: './offer-pricing-matrix-table.component.html',
  styleUrl: './offer-pricing-matrix-table.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferPricingMatrixTableComponent {
  readonly matrix = input.required<OfferPricingMatrix>();
  readonly editable = input(false);
  readonly isMutating = input(false);
  readonly forms = input.required<ReadonlyMap<string, PriceCellForm>>();
  readonly editingLocations = input.required<ReadonlyMap<string, string>>();
  readonly savingCellKey = input<string | null>(null);
  readonly cellErrors = input.required<ReadonlyMap<string, string>>();

  readonly retailerRemovalRequested = output<{ retailerId: string; displayName: string }>();
  readonly productLinkRequested = output<PricingCellAction>();
  readonly editStarted = output<PricingEditAction>();
  readonly editCancelled = output<PricingEditAction>();
  readonly priceSaveRequested = output<PricingEditAction>();
  readonly priceUseRequested = output<PricingCellAction>();
  readonly selectionClearRequested = output<OfferPricingProductRow>();
  readonly historyRequested = output<PricingCellAction>();

  readonly displayedColumns = computed(() => ['product', ...this.matrix().retailers.map(retailer => retailer.retailerId)]);
  readonly displayRows = computed(() => buildDisplayRows(this.matrix().products));
  readonly groupColumns = ['group'];
  readonly isActivityGroup = (_index: number, row: PricingDisplayRow) => row.kind === 'activity';
  readonly isSectionGroup = (_index: number, row: PricingDisplayRow) => row.kind === 'section';
  readonly isProductRow = (_index: number, row: PricingDisplayRow) => row.kind === 'product';
  readonly formatQuantity = formatOfferQuantity;
  readonly measurementUnit = measurementUnitLabel;

  formFor(row: OfferPricingProductRow, cell: OfferRetailerPriceCell): PriceCellForm {
    const form = this.forms().get(this.cellKey(row.offerProductLineId, cell.retailerId));
    if (!form) throw new Error('Missing Offer pricing cell form.');
    return form;
  }

  cellFor(row: OfferPricingProductRow, retailerId: string): OfferRetailerPriceCell {
    const cell = row.retailerPrices.find(item => item.retailerId === retailerId);
    if (!cell) throw new Error('Missing Offer retailer price cell.');
    return cell;
  }

  isEditing(displayRow: PricingProductDisplayRow, retailerId: string): boolean {
    return this.editingLocations().get(this.cellKey(displayRow.product.offerProductLineId, retailerId)) === displayRow.id;
  }

  isEditingElsewhere(displayRow: PricingProductDisplayRow, retailerId: string): boolean {
    const location = this.editingLocations().get(this.cellKey(displayRow.product.offerProductLineId, retailerId));
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
    return this.editable() && cell.retailerListingIsActive !== false && this.matrix().retailers.find(retailer => retailer.retailerId === cell.retailerId)?.isActive !== false;
  }

  canRemoveRetailer(retailerId: string): boolean {
    return !this.matrix().products.some(row => row.selectedPrice?.retailerId === retailerId);
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

  formatCurrency(amount: number | null): string {
    return amount === null ? '—' : new Intl.NumberFormat(undefined, { style: 'currency', currency: 'EUR' }).format(amount);
  }

  relativeAge(value: string): string {
    const observed = new Date(value);
    const today = new Date();
    const observedDay = new Date(observed.getFullYear(), observed.getMonth(), observed.getDate());
    const todayDay = new Date(today.getFullYear(), today.getMonth(), today.getDate());
    const days = Math.max(0, Math.round((todayDay.getTime() - observedDay.getTime()) / 86_400_000));
    return new Intl.RelativeTimeFormat(undefined, { numeric: 'auto' }).format(-days, 'day');
  }

  exactDate(value: string): string {
    return new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(new Date(value));
  }

  private cellKey(lineId: string, retailerId: string): string { return `${lineId}/${retailerId}`; }
}

function buildDisplayRows(products: readonly OfferPricingProductRow[]): PricingDisplayRow[] {
  const occurrences = products.flatMap(product => product.requirements.map(requirement => ({ product, requirement })));
  occurrences.sort((left, right) => left.requirement.activitySortOrder - right.requirement.activitySortOrder || left.requirement.activityNumberId - right.requirement.activityNumberId || left.requirement.offerActivityId.localeCompare(right.requirement.offerActivityId) || left.requirement.sectionSortOrder - right.requirement.sectionSortOrder || left.requirement.offerActivitySectionId.localeCompare(right.requirement.offerActivitySectionId) || left.requirement.sortOrder - right.requirement.sortOrder || left.product.productNumberId - right.product.productNumberId || left.requirement.id.localeCompare(right.requirement.id));
  const counts = new Map<string, number>();
  for (const occurrence of occurrences) counts.set(occurrence.product.offerProductLineId, (counts.get(occurrence.product.offerProductLineId) ?? 0) + 1);
  const rows: PricingDisplayRow[] = [];
  let activityId: string | null = null;
  let sectionId: string | null = null;
  for (const { product, requirement } of occurrences) {
    if (activityId !== requirement.offerActivityId) { activityId = requirement.offerActivityId; sectionId = null; rows.push({ kind: 'activity', id: `activity/${requirement.offerActivityId}`, activityNumberId: requirement.activityNumberId, activityName: requirement.activityName }); }
    if (sectionId !== requirement.offerActivitySectionId) { sectionId = requirement.offerActivitySectionId; rows.push({ kind: 'section', id: `section/${requirement.offerActivitySectionId}`, offerActivityId: requirement.offerActivityId, sectionName: requirement.sectionName }); }
    rows.push({ kind: 'product', id: `requirement/${requirement.id}`, product, requirement, sharedRequirementCount: counts.get(product.offerProductLineId) ?? 1 });
  }
  return rows;
}
