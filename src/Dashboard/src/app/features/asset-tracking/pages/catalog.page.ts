import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute } from '@angular/router';
import { injectQuery } from '@tanstack/angular-query-experimental';

import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DataTableColumn, DataTableState } from '../../../shared/data-table/data-table.types';
import { ASSET_CLASSIFICATIONS, CatalogListItem } from '../models/asset-tracking.models';
import { AssetTrackingService } from '../services/asset-tracking.service';

const COLUMNS: readonly DataTableColumn<CatalogListItem>[] = [
  { key: 'name', label: 'Name', sortable: true, cellType: 'internal-link', linkRouteAccessor: (row) => ['/asset-tracking/catalog', row.id], filter: { kind: 'text', placeholder: 'Search catalog' } },
  { key: 'classification', label: 'Classification', sortable: true, filter: { kind: 'select', options: ASSET_CLASSIFICATIONS.map((x) => ({ label: x, value: x })) } },
  { key: 'manufacturer', label: 'Manufacturer', sortable: true },
  { key: 'model', label: 'Model' },
  { key: 'reviewStatus', label: 'Review State', sortable: true }
];

@Component({
  selector: 'app-catalog-page',
  imports: [DataTableComponent, MatButtonModule, MatFormFieldModule, MatInputModule, MatSelectModule, ReactiveFormsModule],
  template: `
    <main class="tracking-page">
      <header><div><h2>{{ pending ? 'Pending Catalog Reviews' : 'Asset Catalog' }}</h2><p>{{ pending ? 'Review catalog suggestions awaiting an administrator decision.' : 'Create and maintain the Machine and Tool catalog.' }}</p></div></header>
      @if (!pending) {
        <form class="card form-grid" [formGroup]="form" (ngSubmit)="create()">
          <h3>Create active catalog item</h3>
          <mat-form-field><mat-label>Classification</mat-label><mat-select formControlName="classification">@for (value of classifications; track value) {<mat-option [value]="value">{{value}}</mat-option>}</mat-select></mat-form-field>
          <mat-form-field><mat-label>Name</mat-label><input matInput formControlName="name" /></mat-form-field>
          <mat-form-field><mat-label>Manufacturer</mat-label><input matInput formControlName="manufacturer" /></mat-form-field>
          <mat-form-field><mat-label>Model</mat-label><input matInput formControlName="model" /></mat-form-field>
          <mat-form-field class="wide"><mat-label>Description</mat-label><textarea matInput formControlName="description"></textarea></mat-form-field>
          <div class="wide"><button mat-flat-button type="submit" [disabled]="form.invalid || saving()">Create active item</button></div>
          @if (message()) { <p class="wide notice" role="status">{{message()}}</p> }
        </form>
      }
      @if (query.isPending()) { <p role="status">Loading catalog…</p> }
      @if (query.isError()) { <p class="error" role="alert">{{service.errorMessage(query.error(), 'The catalog')}}</p> }
      <app-data-table tableEyebrow="Catalog" [tableLabel]="pending ? 'Pending catalog reviews' : 'Asset catalog'" emptyMessage="No catalog items match the current filters." filterApplyMode="search" [rows]="query.data()?.items ?? []" [filteredRowsTotal]="query.data()?.filteredCount ?? 0" [overallRowsTotal]="query.data()?.totalCount ?? 0" [columns]="columns" [pageSize]="25" [pageSizeOptions]="[25,50,100]" (tableStateChange)="state.set($event)" />
    </main>`,
  styleUrl: '../asset-tracking.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CatalogPage {
  readonly service = inject(AssetTrackingService);
  private readonly route = inject(ActivatedRoute);
  readonly pending = this.route.snapshot.data['pending'] === true;
  readonly columns = COLUMNS;
  readonly classifications = ASSET_CLASSIFICATIONS;
  readonly state = signal<DataTableState<CatalogListItem> | null>(null);
  readonly saving = signal(false);
  readonly message = signal('');
  readonly query = injectQuery(() => {
    const state = this.state();
    return { queryKey: ['asset-tracking', 'catalog', this.pending, JSON.stringify(state)] as const, enabled: state !== null, queryFn: () => this.service.getCatalog(state!, this.pending ? 'PendingReview' : undefined) };
  });
  readonly form = new FormGroup({ classification: new FormControl<'Machine' | 'Tool'>('Machine', { nonNullable: true, validators: Validators.required }), name: new FormControl('', { nonNullable: true, validators: [Validators.required, Validators.maxLength(200)] }), manufacturer: new FormControl(''), model: new FormControl(''), description: new FormControl('') });
  async create(): Promise<void> {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.saving.set(true); this.message.set('');
    try { const value = this.form.getRawValue(); await this.service.createCatalogItem({ ...value, manufacturer: value.manufacturer || null, model: value.model || null, description: value.description || null }); this.form.reset({ classification: 'Machine', name: '', manufacturer: '', model: '', description: '' }); await this.service.invalidate(); this.message.set('Catalog item created.'); }
    catch { this.message.set('The catalog item could not be created. Review validation and permissions.'); }
    finally { this.saving.set(false); }
  }
}
