import { ChangeDetectionStrategy, Component, ElementRef, OnChanges, SimpleChanges, ViewChild, forwardRef, inject, input, output, signal } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent, MatAutocompleteTrigger } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { ProductCatalogCategory, ProductCatalogProductType } from '../../models/product-catalog.models';
import { ProductCatalogService } from '../../services/product-catalog.service';

export type CatalogSelectorKind = 'category' | 'product-type';
export type CatalogSelection = ProductCatalogCategory | ProductCatalogProductType;

@Component({
  selector: 'app-catalog-selector',
  imports: [ReactiveFormsModule, MatAutocompleteModule, MatButtonModule, MatFormFieldModule, MatInputModule],
  templateUrl: './catalog-selector.component.html',
  styleUrl: './catalog-selector.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => CatalogSelectorComponent), multi: true }]
})
export class CatalogSelectorComponent implements ControlValueAccessor, OnChanges {
  @ViewChild('selectorInput') private selectorInput?: ElementRef<HTMLInputElement>;
  @ViewChild(MatAutocompleteTrigger) private autocompleteTrigger?: MatAutocompleteTrigger;

  private readonly catalogService = inject(ProductCatalogService);
  private searchTimer: ReturnType<typeof setTimeout> | null = null;
  private searchRevision = 0;
  private onChange: (value: CatalogSelection | null) => void = () => undefined;
  private onTouched: () => void = () => undefined;

  readonly kind = input.required<CatalogSelectorKind>();
  readonly categoryId = input<string | null>(null);
  readonly label = input.required<string>();
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
  readonly displaySelection = (value: string | CatalogSelection | null): string => {
    if (typeof value === 'string' || value === null) return value ?? '';
    return this.isProductType(value) ? `${value.name} · ${value.categoryName}` : value.name;
  };

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['categoryId'] || this.kind() !== 'product-type') return;
    this.invalidateLookup();
    if (this.autocompleteTrigger?.panelOpen) this.loadOptions('');
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
    if (isDisabled) this.inputControl.disable({ emitEvent: false });
    else this.inputControl.enable({ emitEvent: false });
  }

  focus(): void {
    this.selectorInput?.nativeElement.focus();
    this.loadOptions('');
  }

  onInput(value: string): void {
    this.invalidateLookup();
    this.inputControl.setValue(value, { emitEvent: false });
    this.publishSelection(null);
    this.searchTimer = setTimeout(() => this.loadOptions(value), 250);
  }

  onFocus(): void {
    if (this.loading()) return;
    const value = this.inputControl.value;
    this.loadOptions(typeof value === 'string' ? value : '');
  }

  onKeyboardOpen(): void {
    if (!this.hasLoaded() && !this.loading()) this.loadOptions('');
  }

  onOpened(): void {
    if (this.loading() || this.hasLoaded()) return;
    const value = this.inputControl.value;
    this.loadOptions(typeof value === 'string' ? value : '');
  }

  onBlur(): void {
    this.onTouched();
  }

  onOptionSelected(event: MatAutocompleteSelectedEvent): void {
    const selection = event.option.value as CatalogSelection;
    this.invalidateLookup();
    this.inputControl.setValue(selection, { emitEvent: false });
    this.publishSelection(selection);
  }

  retry(): void {
    const value = this.inputControl.value;
    this.loadOptions(typeof value === 'string' ? value : '');
  }

  private async loadOptions(searchTerm: string): Promise<void> {
    this.clearSearchTimer();
    const revision = ++this.searchRevision;
    const normalizedSearchTerm = searchTerm.trim();
    this.results.set([]);
    this.loading.set(true);
    this.lookupError.set(null);
    this.hasLoaded.set(true);
    this.openPanelSoon();

    try {
      const response = this.kind() === 'category'
        ? await this.catalogService.getCategories({ searchTerm: normalizedSearchTerm, pageSize: 20 })
        : await this.catalogService.getProductTypes({
          searchTerm: normalizedSearchTerm,
          categoryId: this.categoryId(),
          pageSize: 20
        });
      if (revision !== this.searchRevision) return;
      this.results.set(response.items);
      this.openPanelSoon();
    } catch {
      if (revision !== this.searchRevision) return;
      this.lookupError.set(this.kind() === 'category'
        ? 'Categories could not be loaded. Try again.'
        : 'Product Types could not be loaded. Try again.');
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

  private publishSelection(selection: CatalogSelection | null): void {
    this.onChange(selection);
    this.selectionChange.emit(selection);
  }

  private openPanelSoon(): void {
    setTimeout(() => this.autocompleteTrigger?.openPanel());
  }

  isProductType(selection: CatalogSelection): selection is ProductCatalogProductType {
    return 'categoryId' in selection;
  }
}
