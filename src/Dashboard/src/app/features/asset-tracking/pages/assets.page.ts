import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { DataTableComponent } from '../../../shared/data-table/data-table.component';
import { DataTableColumn, DataTableState } from '../../../shared/data-table/data-table.types';
import { ASSET_CLASSIFICATIONS, ASSET_CONDITIONS, AssetListItem, REGISTRATION_STATUSES, REVIEW_STATUSES } from '../models/asset-tracking.models';
import { AssetTrackingService } from '../services/asset-tracking.service';

const COLUMNS: readonly DataTableColumn<AssetListItem>[] = [
  { key: 'assetCode', label: 'Asset Code', sortable: true, cellType: 'internal-link', linkRouteAccessor: (row) => ['/asset-tracking/assets', row.id], filter: { kind: 'text', placeholder: 'Code, QR, name, serial, site or worker' } },
  { key: 'name', label: 'Name', sortable: true, filter: { kind: 'text', placeholder: 'Name' } },
  { key: 'classification', label: 'Class', sortable: true, filter: { kind: 'select', options: ASSET_CLASSIFICATIONS.map((x) => ({label:x,value:x})) } },
  { key: 'manufacturer', label: 'Manufacturer', sortable: true, filter: { kind: 'text', placeholder: 'Manufacturer' } },
  { key: 'model', label: 'Model', sortable: true, filter: { kind: 'text', placeholder: 'Model' } },
  { key: 'siteName', label: 'Site', sortable: true }, { key: 'responsibleUserDisplayName', label: 'Responsible User', sortable: true },
  { key: 'condition', label: 'Condition', sortable: true, filter: { kind: 'select', options: ASSET_CONDITIONS.map((x) => ({label:x,value:x})) } },
  { key: 'operationalStatus', label: 'Status', sortable: true, filter: { kind: 'select', options: [...REGISTRATION_STATUSES, 'InTransfer', 'Missing', 'Retired'].map((x) => ({label:x,value:x})) } },
  { key: 'reviewStatus', label: 'Registration', sortable: true, filter: { kind: 'select', options: REVIEW_STATUSES.map((x) => ({label:x,value:x})) } },
  { key: 'hasPendingHandover', label: 'Pending Handover' }
];

@Component({
  selector: 'app-assets-page', imports: [DataTableComponent], template: `
  <main class="tracking-page"><header><div><h2>{{pending ? 'Pending Registrations' : 'Assets'}}</h2><p>{{pending ? 'Review asset registrations awaiting a decision.' : 'Search the operational asset register.'}}</p></div></header>
  @if (query.isPending()) {<p role="status">Loading assets…</p>} @if (query.isError()) {<p class="error" role="alert">{{service.errorMessage(query.error(), 'Assets')}}</p>}
  <app-data-table tableEyebrow="Register" [tableLabel]="pending ? 'Pending registrations' : 'Tracked assets'" emptyMessage="No assets match the current filters." filterApplyMode="search" [rows]="query.data()?.items ?? []" [filteredRowsTotal]="query.data()?.filteredCount ?? 0" [overallRowsTotal]="query.data()?.totalCount ?? 0" [columns]="columns" [pageSize]="25" [pageSizeOptions]="[25,50,100]" (tableStateChange)="state.set($event)" /></main>`, styleUrl: '../asset-tracking.css', changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetsPage {
  readonly service = inject(AssetTrackingService); private readonly route = inject(ActivatedRoute);
  readonly pending = this.route.snapshot.data['pending'] === true; readonly columns = COLUMNS; readonly state = signal<DataTableState<AssetListItem> | null>(null);
  readonly query = injectQuery(() => { const state = this.state(); return { queryKey: ['asset-tracking','assets',this.pending,JSON.stringify(state)] as const, enabled: state !== null, queryFn: () => this.service.getAssets(state!, this.pending ? 'PendingReview' : undefined) }; });
}
