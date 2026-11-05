import { ChangeDetectionStrategy, Component, OnChanges, OnDestroy, SimpleChanges, forwardRef, inject, input, signal } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTooltipModule } from '@angular/material/tooltip';
import { RouterLink } from '@angular/router';

import { TerritorialEntry, TerritorialEntryKind } from './territorial-structure.models';
import { TerritorialStructureService } from './territorial-structure.service';

@Component({
  selector: 'app-territorial-selector',
  imports: [ReactiveFormsModule, RouterLink, MatAutocompleteModule, MatButtonModule, MatFormFieldModule, MatIconModule, MatInputModule, MatTooltipModule],
  template: `
    <mat-form-field appearance="outline" class="territorial-selector">
      <mat-label>{{ label() }}</mat-label>
      <input matInput [formControl]="inputControl" [matAutocomplete]="autocomplete" [placeholder]="placeholder()" [disabled]="!available()" autocomplete="off" inputmode="search" (input)="handleInput($any($event.target).value)" (focus)="search('')" (blur)="markTouched()" />
      <a mat-icon-button matSuffix class="territorial-selector__manage-link" routerLink="/territorial-structure" target="_blank" rel="noopener noreferrer" aria-label="Manage territorial structure in a new tab" matTooltip="Manage territorial structure in a new tab">
        <mat-icon aria-hidden="true">open_in_new</mat-icon>
      </a>
      <mat-autocomplete #autocomplete="matAutocomplete" [displayWith]="display" [requireSelection]="true" [autoActiveFirstOption]="true" (optionSelected)="select($event)">
        @if (loading()) { <mat-option disabled>Loading…</mat-option> }
        @else if (results().length === 0) { <mat-option disabled>No matching entries.</mat-option> }
        @else { @for (entry of results(); track entry.id) { <mat-option [value]="entry" [disabled]="!entry.isActive && entry.id !== value">{{ entry.name }}@if (!entry.isActive) { (inactive) }</mat-option> } }
      </mat-autocomplete>
    </mat-form-field>
  `,
  styles: [
    ':host,.territorial-selector{display:block;width:100%}',
    '.territorial-selector__manage-link{margin-right:.25rem;margin-left:.25rem;border-left:1px solid var(--mat-sys-outline-variant);border-radius:0;color:var(--mat-sys-primary);pointer-events:auto}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TerritorialSelectorComponent), multi: true }]
})
export class TerritorialSelectorComponent implements ControlValueAccessor, OnChanges, OnDestroy {
  private readonly service = inject(TerritorialStructureService);
  private timer: ReturnType<typeof setTimeout> | null = null;
  private requestVersion = 0;
  private onChange: (value: string) => void = () => undefined;
  private onTouched: () => void = () => undefined;
  readonly kind = input.required<TerritorialEntryKind>();
  readonly parentId = input<string | null>(null);
  readonly available = input(true);
  readonly label = input.required<string>();
  readonly placeholder = input('Search…');
  readonly inputControl = new FormControl<string | TerritorialEntry>('');
  readonly results = signal<readonly TerritorialEntry[]>([]);
  readonly loading = signal(false);
  value = '';
  readonly display = (value: string | TerritorialEntry | null): string => typeof value === 'object' && value ? value.name : value ?? '';

  ngOnChanges(changes: SimpleChanges): void {
    if (!changes['parentId'] || changes['parentId'].firstChange) return;
    this.cancelPendingLoad();
    this.value = '';
    this.inputControl.setValue('', { emitEvent: false });
    this.results.set([]);
    this.onChange('');
  }
  ngOnDestroy(): void { this.cancelPendingLoad(); }
  writeValue(value: string | null): void { this.cancelPendingLoad(); this.value = value ?? ''; this.inputControl.setValue('', { emitEvent: false }); if (this.value) this.search(''); }
  registerOnChange(fn: (value: string) => void): void { this.onChange = fn; }
  registerOnTouched(fn: () => void): void { this.onTouched = fn; }
  setDisabledState(disabled: boolean): void { if (disabled) this.inputControl.disable({ emitEvent: false }); else this.inputControl.enable({ emitEvent: false }); }
  markTouched(): void { this.onTouched(); }
  handleInput(term: string): void { if (this.value) { this.value = ''; this.onChange(''); } this.search(term); }
  search(term: string): void {
    this.cancelPendingLoad();
    if (!this.available() || (!this.parentId() && this.kind() !== 0)) { this.results.set([]); return; }
    const requestVersion = this.requestVersion;
    const parentId = this.parentId();
    this.timer = setTimeout(() => void this.load(term, parentId, requestVersion), 200);
  }
  select(event: MatAutocompleteSelectedEvent): void { const entry = event.option.value as TerritorialEntry; this.value = entry.id; this.inputControl.setValue(entry, { emitEvent: false }); this.onChange(entry.id); this.onTouched(); }
  private async load(term: string, parentId: string | null, requestVersion: number): Promise<void> {
    if (requestVersion !== this.requestVersion) return;
    this.loading.set(true);
    try {
      const entries = await this.service.getEntries(this.kind(), parentId, term, true);
      if (requestVersion !== this.requestVersion || parentId !== this.parentId()) return;
      this.results.set(entries.filter(entry => entry.isActive || entry.id === this.value));
      const selected = entries.find(entry => entry.id === this.value);
      if (selected) this.inputControl.setValue(selected, { emitEvent: false });
    } catch {
      if (requestVersion === this.requestVersion) this.results.set([]);
    } finally {
      if (requestVersion === this.requestVersion) this.loading.set(false);
    }
  }
  private cancelPendingLoad(): void {
    this.requestVersion += 1;
    if (this.timer) clearTimeout(this.timer);
    this.timer = null;
    this.loading.set(false);
  }
}
