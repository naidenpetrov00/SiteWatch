import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  WritableSignal,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  FormArray,
  FormControl,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn
} from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { DashboardRetailerLookup } from '../../models/dashboard-retailer.models';
import { OfferDefaultRetailer } from '../../models/offer-default-retailer.models';
import { DashboardRetailersService } from '../../services/dashboard-retailers.service';
import { OfferDefaultRetailersService } from '../../services/offer-default-retailers.service';
import { getRetailerError } from '../../utils/retailer-error';

type RetailerSlotValue =
  | string
  | DashboardRetailerLookup
  | OfferDefaultRetailer
  | null;

const SLOT_COUNT = 3;

@Component({
  selector: 'app-default-offer-retailers',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule
  ],
  templateUrl: './default-offer-retailers.component.html',
  styleUrl: './default-offer-retailers.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DefaultOfferRetailersComponent {
  readonly defaultsService = inject(OfferDefaultRetailersService);
  private readonly retailersService = inject(DashboardRetailersService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly searchRevisions = [0, 0, 0];

  readonly slotControls = Array.from(
    { length: SLOT_COUNT },
    () => new FormControl<RetailerSlotValue>(null, retailerSelectionValidator())
  );
  readonly slotForm = new FormArray(this.slotControls);
  readonly selectedSlots = signal<readonly (OfferDefaultRetailer | null)[]>(
    emptySlots()
  );
  readonly searchResults = signal<
    readonly (readonly DashboardRetailerLookup[])[]
  >([[], [], []]);
  readonly searchLoading = signal<readonly boolean[]>([false, false, false]);
  readonly searchErrors = signal<readonly (string | null)[]>([null, null, null]);
  readonly hasSearched = signal<readonly boolean[]>([false, false, false]);
  readonly saveMessage = signal<string | null>(null);
  readonly saveError = signal<string | null>(null);
  readonly serverStateChanged = signal(false);

  private readonly baselineIds = signal<readonly string[]>([]);
  private readonly lastServerSignature = signal<string | null>(null);
  private readonly pendingServerState = signal<
    readonly OfferDefaultRetailer[] | null
  >(null);
  private readonly initialized = signal(false);
  private readonly formStatus = toSignal(this.slotForm.statusChanges, {
    initialValue: this.slotForm.status
  });

  readonly configuredCount = computed(
    () => this.selectedSlots().filter(isSelectedRetailer).length
  );
  readonly currentIds = computed(() =>
    this.selectedSlots()
      .filter(isSelectedRetailer)
      .map((retailer) => retailer.retailerId)
  );
  readonly hasChanges = computed(
    () => !sameIds(this.currentIds(), this.baselineIds())
  );
  readonly hasDuplicates = computed(
    () => new Set(this.currentIds()).size !== this.currentIds().length
  );
  readonly canSave = computed(
    () =>
      this.initialized() &&
      this.hasChanges() &&
      this.formStatus() === 'VALID' &&
      !this.hasDuplicates() &&
      !this.serverStateChanged() &&
      !this.defaultsService.defaultsQuery.isError() &&
      !this.defaultsService.defaultsQuery.isFetching() &&
      !this.defaultsService.replaceMutation.isPending()
  );

  readonly displayRetailer = (value: RetailerSlotValue): string =>
    typeof value === 'string' ? value : value?.displayName ?? '';

  constructor() {
    this.syncControlAvailability();
    this.configureSearches();

    effect(() => {
      const defaults = this.defaultsService.defaultsQuery.data();
      if (!defaults) return;

      const signature = serverSignature(defaults);
      if (signature === this.lastServerSignature()) return;

      if (!this.initialized() || !this.hasChanges()) {
        this.applyServerState(defaults);
        return;
      }

      this.pendingServerState.set(defaults);
      this.serverStateChanged.set(true);
    });

    this.destroyRef.onDestroy(() => {
      this.searchRevisions.forEach((_, index) => {
        this.searchRevisions[index] += 1;
      });
    });
  }

  selectedAt(index: number): OfferDefaultRetailer | null {
    return this.selectedSlots()[index] ?? null;
  }

  resultsAt(index: number): readonly DashboardRetailerLookup[] {
    return this.searchResults()[index] ?? [];
  }

  isSearchLoading(index: number): boolean {
    return this.searchLoading()[index] ?? false;
  }

  searchErrorAt(index: number): string | null {
    return this.searchErrors()[index] ?? null;
  }

  hasSearchedAt(index: number): boolean {
    return this.hasSearched()[index] ?? false;
  }

  isInitialLoadError(): boolean {
    return (
      this.defaultsService.defaultsQuery.isError() &&
      !this.defaultsService.defaultsQuery.data()
    );
  }

  canMoveUp(index: number): boolean {
    return index > 0 && this.selectedAt(index) !== null;
  }

  canMoveDown(index: number): boolean {
    return (
      index < SLOT_COUNT - 1 &&
      this.selectedAt(index) !== null &&
      this.selectedAt(index + 1) !== null
    );
  }

  onRetailerSelected(
    index: number,
    event: MatAutocompleteSelectedEvent
  ): void {
    const selected = event.option.value as DashboardRetailerLookup;
    const duplicate = this.selectedSlots().some(
      (retailer, slotIndex) =>
        slotIndex !== index && retailer?.retailerId === selected.id
    );
    if (duplicate) {
      this.slotControls[index].setValue(this.selectedAt(index), {
        emitEvent: false
      });
      this.clearSearch(index);
      this.saveError.set(
        `${selected.displayName} is already configured in another slot.`
      );
      return;
    }

    const slots = [...this.selectedSlots()];
    const defaultRetailer: OfferDefaultRetailer = {
      retailerId: selected.id,
      displayName: selected.displayName,
      baseWebsiteUrl: selected.baseWebsiteUrl,
      websiteHost: selected.websiteHost
    };
    slots[index] = defaultRetailer;
    this.selectedSlots.set(slots);
    this.slotControls[index].setValue(defaultRetailer, { emitEvent: false });
    this.slotForm.markAsDirty();
    this.clearSearch(index);
    this.clearSaveFeedback();
    this.syncControlAvailability();
  }

  remove(index: number): void {
    const selected = this.selectedSlots().filter(isSelectedRetailer);
    selected.splice(index, 1);
    this.applyDraftSlots(selected);
  }

  move(index: number, offset: -1 | 1): void {
    const targetIndex = index + offset;
    const slots = [...this.selectedSlots()];
    if (!slots[index] || !slots[targetIndex]) return;

    [slots[index], slots[targetIndex]] = [slots[targetIndex], slots[index]];
    this.applyDraftSlots(slots.filter(isSelectedRetailer));
  }

  restoreEmptyInput(index: number): void {
    const control = this.slotControls[index];
    if (typeof control.value === 'string' && control.value.trim().length === 0) {
      control.setValue(this.selectedAt(index), { emitEvent: false });
      control.updateValueAndValidity();
    }
  }

  retryLoad(): void {
    void this.defaultsService.defaultsQuery.refetch();
  }

  reloadServerState(): void {
    const defaults =
      this.pendingServerState() ?? this.defaultsService.defaultsQuery.data();
    if (defaults) this.applyServerState(defaults);
  }

  async save(): Promise<void> {
    if (!this.canSave()) {
      this.slotForm.markAllAsTouched();
      return;
    }

    this.clearSaveFeedback();
    try {
      const saved = await this.defaultsService.replace(this.currentIds());
      this.applyServerState(this.defaultsService.defaultsQuery.data() ?? saved);
      this.saveMessage.set('Default Offer retailers saved successfully.');
    } catch (error) {
      this.saveError.set(
        getRetailerError(
          error,
          'The default Offer retailers could not be saved. Please try again.'
        )
      );
    }
  }

  private configureSearches(): void {
    this.slotControls.forEach((control, index) => {
      control.valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe((value) => {
          if (typeof value !== 'string') return;

          this.searchRevisions[index] += 1;
          this.setIndexed(this.searchResults, index, []);
          this.setIndexed(this.searchErrors, index, null);
          this.setIndexed(this.hasSearched, index, false);
          this.clearSaveFeedback();
        });

      control.valueChanges
        .pipe(
          debounceTime(250),
          distinctUntilChanged(),
          takeUntilDestroyed(this.destroyRef)
        )
        .subscribe((value) => {
          if (typeof value === 'string') {
            void this.search(index, value, this.searchRevisions[index]);
          }
        });
    });
  }

  private async search(
    index: number,
    searchTerm: string,
    revision: number
  ): Promise<void> {
    const normalized = searchTerm.trim();
    if (!normalized) {
      this.setIndexed(this.searchLoading, index, false);
      return;
    }

    this.setIndexed(this.searchLoading, index, true);
    this.setIndexed(this.hasSearched, index, true);
    try {
      const results = await this.retailersService.searchRetailers(normalized);
      if (revision !== this.searchRevisions[index]) return;

      const selectedIds = new Set(
        this.selectedSlots()
          .filter((retailer, slotIndex) =>
            slotIndex === index ? false : isSelectedRetailer(retailer)
          )
          .filter(isSelectedRetailer)
          .map((retailer) => retailer.retailerId)
      );
      this.setIndexed(
        this.searchResults,
        index,
        results.filter((retailer) => !selectedIds.has(retailer.id))
      );
    } catch {
      if (revision !== this.searchRevisions[index]) return;
      this.setIndexed(
        this.searchErrors,
        index,
        'Active retailers could not be searched. Try again.'
      );
    } finally {
      if (revision === this.searchRevisions[index]) {
        this.setIndexed(this.searchLoading, index, false);
      }
    }
  }

  private applyDraftSlots(retailers: readonly OfferDefaultRetailer[]): void {
    const slots = emptySlots();
    retailers.slice(0, SLOT_COUNT).forEach((retailer, index) => {
      slots[index] = retailer;
    });
    this.selectedSlots.set(slots);
    this.syncControlValues();
    this.slotForm.markAsDirty();
    this.clearSaveFeedback();
  }

  private applyServerState(defaults: readonly OfferDefaultRetailer[]): void {
    const slots = emptySlots();
    defaults.slice(0, SLOT_COUNT).forEach((retailer, index) => {
      slots[index] = retailer;
    });

    this.selectedSlots.set(slots);
    this.baselineIds.set(defaults.map((retailer) => retailer.retailerId));
    this.lastServerSignature.set(serverSignature(defaults));
    this.pendingServerState.set(null);
    this.serverStateChanged.set(false);
    this.initialized.set(true);
    this.syncControlValues();
    this.slotForm.markAsPristine();
    this.slotForm.markAsUntouched();
    this.saveError.set(null);
  }

  private syncControlValues(): void {
    this.selectedSlots().forEach((retailer, index) => {
      this.searchRevisions[index] += 1;
      this.slotControls[index].setValue(retailer, { emitEvent: false });
      this.clearSearch(index);
    });
    this.syncControlAvailability();
  }

  private syncControlAvailability(): void {
    this.slotControls.forEach((control, index) => {
      const canUse =
        this.initialized() &&
        (index === 0 ||
          this.selectedAt(index) !== null ||
          this.selectedAt(index - 1) !== null);
      if (canUse && control.disabled) {
        control.enable({ emitEvent: false });
      } else if (!canUse && control.enabled) {
        control.disable({ emitEvent: false });
      }
    });
    this.slotForm.updateValueAndValidity();
  }

  private clearSearch(index: number): void {
    this.setIndexed(this.searchResults, index, []);
    this.setIndexed(this.searchLoading, index, false);
    this.setIndexed(this.searchErrors, index, null);
    this.setIndexed(this.hasSearched, index, false);
  }

  private clearSaveFeedback(): void {
    this.saveMessage.set(null);
    this.saveError.set(null);
  }

  private setIndexed<T>(
    state: WritableSignal<readonly T[]>,
    index: number,
    value: T
  ): void {
    state.update((current) => {
      const next = [...current];
      next[index] = value;
      return next;
    });
  }
}

function retailerSelectionValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null =>
    typeof control.value === 'string' ? { retailerSelection: true } : null;
}

function emptySlots(): (OfferDefaultRetailer | null)[] {
  return Array.from({ length: SLOT_COUNT }, () => null);
}

function isSelectedRetailer(
  retailer: OfferDefaultRetailer | null
): retailer is OfferDefaultRetailer {
  return retailer !== null;
}

function sameIds(left: readonly string[], right: readonly string[]): boolean {
  return (
    left.length === right.length &&
    left.every((retailerId, index) => retailerId === right[index])
  );
}

function serverSignature(defaults: readonly OfferDefaultRetailer[]): string {
  return JSON.stringify(defaults);
}
