import { ChangeDetectionStrategy, Component, ElementRef, OnChanges, SimpleChanges, ViewChild, forwardRef, inject, input, signal } from '@angular/core';
import { ControlValueAccessor, FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { TerritorialEntry, TerritorialEntryKind } from './territorial-structure.models';
import { TerritorialStructureService } from './territorial-structure.service';

@Component({
  selector: 'app-territorial-selector',
  imports: [ReactiveFormsModule, MatAutocompleteModule, MatFormFieldModule, MatInputModule],
  template: `
    <mat-form-field appearance="outline" class="territorial-selector">
      <mat-label>{{ label() }}</mat-label>
      <input #selectorInput matInput [formControl]="inputControl" [matAutocomplete]="autocomplete" [placeholder]="placeholder()" [disabled]="!available()" autocomplete="off" inputmode="search" (input)="search($any($event.target).value)" (focus)="search('')" />
      <mat-autocomplete #autocomplete="matAutocomplete" [displayWith]="display" [requireSelection]="true" [autoActiveFirstOption]="true" (optionSelected)="select($event)">
        @if (loading()) { <mat-option disabled>Loading…</mat-option> }
        @else if (results().length === 0) { <mat-option disabled>No matching entries.</mat-option> }
        @else { @for (entry of results(); track entry.id) { <mat-option [value]="entry" [disabled]="!entry.isActive && entry.id !== value">{{ entry.name }}@if (!entry.isActive) { (inactive) }</mat-option> } }
      </mat-autocomplete>
    </mat-form-field>
  `,
  styles: [':host,.territorial-selector{display:block;width:100%}'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => TerritorialSelectorComponent), multi: true }]
})
export class TerritorialSelectorComponent implements ControlValueAccessor, OnChanges {
  @ViewChild('selectorInput') private input?: ElementRef<HTMLInputElement>;
  private readonly service = inject(TerritorialStructureService);
  private timer: ReturnType<typeof setTimeout> | null = null;
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
    this.value = '';
    this.inputControl.setValue('', { emitEvent: false });
    this.results.set([]);
    this.onChange('');
  }
  writeValue(value: string | null): void { this.value = value ?? ''; this.inputControl.setValue('', { emitEvent: false }); if (this.value) void this.search(''); }
  registerOnChange(fn: (value: string) => void): void { this.onChange = fn; }
  registerOnTouched(fn: () => void): void { this.onTouched = fn; }
  setDisabledState(disabled: boolean): void { if (disabled) this.inputControl.disable({ emitEvent: false }); else this.inputControl.enable({ emitEvent: false }); }
  search(term: string): void { if (!this.available() || (!this.parentId() && this.kind() !== 0)) return; if (this.timer) clearTimeout(this.timer); this.timer = setTimeout(() => void this.load(term), 200); }
  select(event: MatAutocompleteSelectedEvent): void { const entry = event.option.value as TerritorialEntry; this.value = entry.id; this.inputControl.setValue(entry, { emitEvent: false }); this.onChange(entry.id); this.onTouched(); }
  private async load(term: string): Promise<void> { this.loading.set(true); try { const entries = await this.service.getEntries(this.kind(), this.parentId(), term, true); this.results.set(entries.filter(entry => entry.isActive || entry.id === this.value)); const selected = entries.find(entry => entry.id === this.value); if (selected) this.inputControl.setValue(selected, { emitEvent: false }); } finally { this.loading.set(false); } }
}
