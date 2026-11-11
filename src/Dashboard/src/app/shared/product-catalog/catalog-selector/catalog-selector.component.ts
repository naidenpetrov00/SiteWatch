import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  ViewChild,
  forwardRef,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import {
  ControlValueAccessor,
  FormControl,
  NG_VALUE_ACCESSOR,
  ReactiveFormsModule
} from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent,
  MatAutocompleteTrigger
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import {
  ProductCatalogBrand,
  ProductCatalogCategory,
  ProductCatalogPage,
  ProductCatalogProductType
} from '../product-catalog.models';
import { ProductCatalogLookupService } from '../product-catalog-lookup.service';

export type CatalogSelectorKind = 'category' | 'product-type' | 'brand';
export type CatalogSelection = ProductCatalogCategory | ProductCatalogProductType | ProductCatalogBrand;

@Component({
  selector: 'app-catalog-selector',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './catalog-selector.component.html',
  styleUrl: './catalog-selector.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => CatalogSelectorComponent),
      multi: true
    }
  ]
})
export class CatalogSelectorComponent
  implements ControlValueAccessor, OnChanges, OnDestroy
{
  @ViewChild('selectorInput') private selectorInput?: ElementRef<HTMLInputElement>;
  @ViewChild(MatAutocompleteTrigger)
  private autocompleteTrigger?: MatAutocompleteTrigger;

  private readonly catalogLookupService = inject(ProductCatalogLookupService);
  private searchTimer: ReturnType<typeof setTimeout> | null = null;
  private panelOpenTimer: ReturnType<typeof setTimeout> | null = null;
  private panelOpenRequested = false;
  private searchRevision = 0;
  private onChange: (value: CatalogSelection | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  readonly kind = input.required<CatalogSelectorKind>();
  readonly categoryId = input<string | null>(null);
  readonly label = input.required<string>();
  readonly name = input.required<string>();
  readonly placeholder = input('');
  readonly hint = input<string | null>(null);
  readonly required = input(false);
  readonly errorMessage = input('Select a value from the list.');
  readonly selectionChange = output<CatalogSelection | null>();

  readonly inputControl = new FormControl<string | CatalogSelection | null>(null);
  readonly results = signal<readonly CatalogSelection[]>([]);
  readonly loading = signal(false);
  readonly lookupError = signal<string | null>(null);
  readonly hasLoaded = signal(false);
  readonly disabled = signal(false);
  readonly displaySelection = (
    value: string | CatalogSelection | null
  ): string => {
    if (typeof value === 'string' || value === null) return value ?? '';
    return this.isProductType(value)
      ? `${value.name} · ${value.categoryName}`
      : value.name;
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['categoryId'] || this.kind() !== 'product-type') return;
    const panelWasOpen = this.autocompleteTrigger?.panelOpen ?? false;
    this.invalidateLookup();
    if (panelWasOpen) {
      this.panelOpenRequested = true;
      void this.loadOptions('');
    }
  }

  ngOnDestroy(): void {
    this.stopPendingWork();
  }

  writeValue(value: CatalogSelection | null): void {
    this.inputControl.setValue(value, { emitEvent: false });
  }

  registerOnChange(fn: (value: CatalogSelection | null) => void): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
    if (isDisabled) {
      this.stopPanelOpen();
      this.autocompleteTrigger?.closePanel();
      this.inputControl.disable({ emitEvent: false });
    } else {
      this.inputControl.enable({ emitEvent: false });
    }
  }

  focus(): void {
    const input = this.selectorInput?.nativeElement;
    if (!input) return;
    input.focus();
    if (input.ownerDocument.activeElement === input && !this.loading()) {
      this.onClick();
    }
  }

  onInput(value: string): void {
    this.panelOpenRequested = true;
    this.invalidateLookup();
    this.inputControl.setValue(value, { emitEvent: false });
    this.publishSelection(null);
    this.searchTimer = setTimeout(() => this.loadOptions(value), 250);
  }

  onFocus(): void {
    if (this.isSelection(this.inputControl.value)) return;
    this.openOptionsForCurrentValue();
  }

  onClick(): void {
    this.openOptionsForCurrentValue();
  }

  private openOptionsForCurrentValue(): void {
    this.panelOpenRequested = true;
    if (this.loading()) return;
    const value = this.inputControl.value;
    this.loadOptions(typeof value === 'string' ? value : '');
  }

  onKeyboardOpen(): void {
    this.panelOpenRequested = true;
    if (!this.hasLoaded() && !this.loading()) this.loadOptions('');
  }

  onOpened(): void {
    this.panelOpenRequested = true;
    if (this.loading() || this.hasLoaded()) return;
    const value = this.inputControl.value;
    this.loadOptions(typeof value === 'string' ? value : '');
  }

  onBlur(): void {
    this.stopPanelOpen();
    this.onTouched();
  }

  onPanelDismissed(): void {
    this.stopPanelOpen();
  }

  onOptionSelected(event: MatAutocompleteSelectedEvent): void {
    const selection = event.option.value as CatalogSelection;
    this.stopPanelOpen();
    this.invalidateLookup();
    this.inputControl.setValue(selection, { emitEvent: false });
    this.publishSelection(selection);
    this.autocompleteTrigger?.closePanel();
  }

  retry(): void {
    this.focus();
  }

  private async loadOptions(searchTerm: string): Promise<void> {
    this.clearSearchTimer();
    const revision = ++this.searchRevision;
    const normalizedSearchTerm = searchTerm.trim();
    this.results.set([]);
    this.loading.set(true);
    this.lookupError.set(null);
    this.hasLoaded.set(true);
    this.openPanelSoon(revision);

    try {
      const response: ProductCatalogPage<CatalogSelection> = this.kind() === 'category'
        ? await this.catalogLookupService.getCategories({
              searchTerm: normalizedSearchTerm,
              pageSize: 20
            })
        : this.kind() === 'product-type'
          ? await this.catalogLookupService.getProductTypes({
              searchTerm: normalizedSearchTerm,
              categoryId: this.categoryId(),
              pageSize: 20
            })
          : await this.catalogLookupService.getProductBrands({
              searchTerm: normalizedSearchTerm,
              pageSize: 20
            });
      if (revision !== this.searchRevision) return;
      this.results.set(response.items);
      this.openPanelSoon(revision);
    } catch {
      if (revision !== this.searchRevision) return;
      this.lookupError.set(`${this.pluralLabel()} could not be loaded. Try again.`);
    } finally {
      if (revision === this.searchRevision) this.loading.set(false);
    }
  }

  private invalidateLookup(): void {
    this.clearSearchTimer();
    this.searchRevision += 1;
    this.results.set([]);
    this.loading.set(false);
    this.lookupError.set(null);
    this.hasLoaded.set(false);
  }

  private clearSearchTimer(): void {
    if (this.searchTimer !== null) {
      clearTimeout(this.searchTimer);
      this.searchTimer = null;
    }
  }

  private clearPanelOpenTimer(): void {
    if (this.panelOpenTimer !== null) {
      clearTimeout(this.panelOpenTimer);
      this.panelOpenTimer = null;
    }
  }

  private stopPanelOpen(): void {
    this.panelOpenRequested = false;
    this.clearPanelOpenTimer();
  }

  private stopPendingWork(): void {
    this.stopPanelOpen();
    this.clearSearchTimer();
    this.searchRevision += 1;
  }

  private publishSelection(selection: CatalogSelection | null): void {
    this.onChange(selection);
    this.selectionChange.emit(selection);
  }

  private isSelection(
    value: string | CatalogSelection | null
  ): value is CatalogSelection {
    return value !== null && typeof value !== 'string';
  }

  private openPanelSoon(revision: number): void {
    this.clearPanelOpenTimer();
    this.panelOpenTimer = setTimeout(() => {
      this.panelOpenTimer = null;
      const input = this.selectorInput?.nativeElement;
      if (
        revision !== this.searchRevision ||
        !this.panelOpenRequested ||
        this.disabled() ||
        !input ||
        input.ownerDocument.activeElement !== input
      ) {
        return;
      }

      this.autocompleteTrigger?.openPanel();
    });
  }

  isProductType(
    selection: CatalogSelection
  ): selection is ProductCatalogProductType {
    return 'categoryId' in selection;
  }

  pluralLabel(): string {
    if (this.kind() === 'category') return 'Categories';
    if (this.kind() === 'product-type') return 'Product Types';
    return 'Product Brands';
  }
}
