import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed, toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import {
  ASSET_CLASSIFICATIONS,
  ASSET_CONDITIONS,
  AssetActionRequest,
  AssetClassification,
  AssetDetails,
  REGISTRATION_STATUSES,
  SiteLookup,
  UserLookup
} from '../models/asset-tracking.models';
import { AssetTrackingService } from '../services/asset-tracking.service';

export type AssetActionKind =
  | 'registration-review'
  | 'details'
  | 'site'
  | 'condition'
  | 'operational-status'
  | 'missing'
  | 'retirement'
  | 'responsibility'
  | 'qr-identifier';

export interface AssetActionDialogData {
  asset: AssetDetails;
  kind: AssetActionKind;
  title: string;
  accepted?: boolean;
}

interface ConflictComparisonRow {
  label: string;
  draft: string;
  server: string;
  changed: boolean;
}

@Component({
  selector: 'app-asset-action-dialog',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatDialogModule,
    MatButtonModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule
  ],
  template: `
    <h2 mat-dialog-title>{{ data.title }}</h2>
    <mat-dialog-content>
      <form class="dialog-form" [formGroup]="form">
        @if (data.kind === 'details') {
          <mat-form-field>
            <mat-label>Classification</mat-label>
            <mat-select formControlName="classification">
              @for (value of classifications; track value) {
                <mat-option [value]="value">{{ value }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
          <mat-form-field>
            <mat-label>Name</mat-label>
            <input matInput formControlName="name" />
          </mat-form-field>
          <mat-form-field>
            <mat-label>Manufacturer</mat-label>
            <input matInput formControlName="manufacturer" />
          </mat-form-field>
          <mat-form-field>
            <mat-label>Model</mat-label>
            <input matInput formControlName="model" />
          </mat-form-field>
          <mat-form-field>
            <mat-label>Description</mat-label>
            <textarea matInput formControlName="description"></textarea>
          </mat-form-field>
          <mat-form-field>
            <mat-label>Manufacturer serial number</mat-label>
            <input matInput formControlName="manufacturerSerialNumber" />
          </mat-form-field>
        }
        @if (data.kind === 'site') {
          <section class="selection-summary" aria-label="Current asset Site">
            <span>Current Site</span>
            <strong>{{ currentAsset().siteName }}</strong>
          </section>
          <mat-form-field>
            <mat-label>Search destination Site</mat-label>
            <input
              matInput
              [formControl]="siteSearchControl"
              [matAutocomplete]="siteAuto"
              (input)="onSiteSearchInput($event)"
            />
            <mat-autocomplete
              #siteAuto="matAutocomplete"
              [displayWith]="displaySite"
              [requireSelection]="true"
              (optionSelected)="selectSite($event)"
            >
              @if (companyStockSiteResults().length > 0) {
                <mat-optgroup label="Company Stock">
                  @for (site of companyStockSiteResults(); track site.id) {
                    <mat-option [value]="site">
                      <span>{{ site.name }} · #{{ site.numberId }} · Company Stock</span>
                      <small>{{ site.address }}</small>
                    </mat-option>
                  }
                </mat-optgroup>
              }
              @if (ordinarySiteResults().length > 0) {
                <mat-optgroup label="Ordinary Sites">
                  @for (site of ordinarySiteResults(); track site.id) {
                    <mat-option [value]="site">
                      <span>{{ site.name }} · #{{ site.numberId }}</span>
                      <small>{{ site.address }}</small>
                    </mat-option>
                  }
                </mat-optgroup>
              }
            </mat-autocomplete>
            <mat-hint>Company Stock and ordinary Sites are shown separately. Type to search all Sites.</mat-hint>
          </mat-form-field>
          @if (siteSearching()) {
            <p class="lookup-status" role="status">
              {{ siteSearchAttempted() ? 'Searching Sites…' : 'Loading Site suggestions…' }}
            </p>
          } @else if (siteSearchError()) {
            <p class="lookup-status error" role="alert">
              {{ siteSearchError() }}
              <button mat-button type="button" (click)="retrySiteSearch()">Retry</button>
            </p>
          } @else if (siteSearchAttempted() && siteResults().length === 0 && !selectedSite()) {
            <p class="lookup-status" role="status">No other Sites match this search.</p>
          }
        }
        @if (data.kind === 'condition') {
          <mat-form-field>
            <mat-label>Condition</mat-label>
            <mat-select formControlName="condition">
              @for (value of conditions; track value) {
                <mat-option [value]="value">{{ value }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        }
        @if (data.kind === 'operational-status') {
          <mat-form-field>
            <mat-label>Status</mat-label>
            <mat-select formControlName="operationalStatus">
              @for (value of statuses; track value) {
                <mat-option [value]="value">{{ value }}</mat-option>
              }
            </mat-select>
          </mat-form-field>
        }
        @if (data.kind === 'responsibility') {
          <section class="selection-summary" aria-label="Current asset responsibility">
            <span>Current responsibility</span>
            <strong>{{ currentAsset().responsibleUserDisplayName || 'Unassigned' }}</strong>
            @if (currentAsset().pendingHandoverRecipientDisplayName) {
              <small>
                Pending handover to {{ currentAsset().pendingHandoverRecipientDisplayName }}.
                Reassignment will cancel it.
              </small>
            }
          </section>
          <mat-form-field>
            <mat-label>Search responsible user</mat-label>
            <input
              matInput
              [formControl]="responsibleUserSearchControl"
              [matAutocomplete]="responsibleUserAuto"
              (input)="onResponsibleUserSearchInput($event)"
            />
            <mat-autocomplete
              #responsibleUserAuto="matAutocomplete"
              [displayWith]="displayResponsibleUser"
              [requireSelection]="true"
              (optionSelected)="selectResponsibleUser($event)"
            >
              @for (user of responsibleUserResults(); track user.id) {
                <mat-option [value]="user">
                  <span>{{ user.displayName }}</span>
                  @if (user.email) {
                    <small>{{ user.email }}</small>
                  }
                </mat-option>
              }
            </mat-autocomplete>
            <mat-hint>Select an Administrator or Worker from the search results.</mat-hint>
          </mat-form-field>
          @if (responsibleUserSearching()) {
            <p class="lookup-status" role="status">Searching eligible responsible users…</p>
          } @else if (responsibleUserSearchError()) {
            <p class="lookup-status error" role="alert">
              {{ responsibleUserSearchError() }}
              <button mat-button type="button" (click)="retryResponsibleUserSearch()">Retry</button>
            </p>
          } @else if (
            responsibleUserSearchAttempted()
            && responsibleUserResults().length === 0
            && !selectedResponsibleUser()
            && !leaveUnassigned()
          ) {
            <p class="lookup-status" role="status">No eligible responsible users match this search.</p>
          }
          @if (canLeaveUnassigned()) {
            <mat-checkbox
              [checked]="leaveUnassigned()"
              (change)="setLeaveUnassigned($event.checked)"
            >
              Leave unassigned
            </mat-checkbox>
          }
          <mat-form-field>
            <mat-label>Reason (required)</mat-label>
            <textarea matInput formControlName="reason"></textarea>
          </mat-form-field>
        }
        @if (data.kind !== 'responsibility') {
          <mat-form-field>
            <mat-label>
              {{
                data.kind === 'registration-review' && !data.accepted
                  ? 'Rejection reason (optional)'
                  : 'Note (optional)'
              }}
            </mat-label>
            <textarea matInput formControlName="note"></textarea>
          </mat-form-field>
        }
        @if (conflict()) {
          <section class="conflict-review" role="alert" aria-live="assertive">
            <h3>Review the concurrent changes</h3>
            @if (refreshedAsset()) {
              <p>
                Your first save was not applied. Your draft remains in the form above. Compare
                it with the latest server values; differing fields are highlighted. Reconfirm
                explicitly to submit the draft with the refreshed concurrency token.
              </p>
              <div class="comparison-scroll">
                <table>
                  <thead>
                    <tr>
                      <th scope="col">Field</th>
                      <th scope="col">Your draft</th>
                      <th scope="col">Latest server value</th>
                    </tr>
                  </thead>
                  <tbody>
                    @for (row of conflictRows(); track row.label) {
                      <tr [class.changed]="row.changed">
                        <th scope="row">
                          {{ row.label }}
                          @if (row.changed) {
                            <span class="changed-label">Differs</span>
                          }
                        </th>
                        <td>{{ row.draft }}</td>
                        <td>{{ row.server }}</td>
                      </tr>
                    }
                  </tbody>
                </table>
              </div>
            } @else {
              <p>
                Your first save was not applied and your draft is preserved, but the latest
                server values could not be loaded.
              </p>
              <button
                mat-stroked-button
                type="button"
                (click)="refreshConflict()"
                [disabled]="saving()"
              >
                Retry refresh
              </button>
            }
          </section>
        }
        @if (error()) {
          <p class="error" role="alert">{{ error() }}</p>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button
        mat-flat-button
        (click)="confirm()"
        [disabled]="
          saving()
          || form.invalid
          || !actionSelectionValid()
          || (conflict() && !refreshedAsset())
        "
      >
        {{ conflict() ? 'Reconfirm draft' : 'Confirm' }}
      </button>
    </mat-dialog-actions>
  `,
  styles: `
    .dialog-form {
      display: grid;
      gap: 0.5rem;
      min-width: min(32rem, 75vw);
      padding-top: 0.5rem;
    }
    .selection-summary {
      display: grid;
      gap: 0.2rem;
      padding: 0.75rem;
      border: 1px solid var(--mat-sys-outline-variant);
      border-radius: 0.75rem;
    }
    .selection-summary span,
    .selection-summary small,
    mat-option small {
      color: var(--mat-sys-on-surface-variant);
    }
    mat-option span,
    mat-option small {
      display: block;
    }
    .lookup-status {
      margin: -0.25rem 0 0.5rem;
      color: var(--mat-sys-on-surface-variant);
    }
    .conflict-review {
      border: 1px solid var(--mat-sys-tertiary);
      border-radius: 0.75rem;
      padding: 1rem;
    }
    .conflict-review h3 {
      margin-top: 0;
    }
    .comparison-scroll {
      overflow-x: auto;
    }
    table {
      width: 100%;
      border-collapse: collapse;
    }
    th,
    td {
      padding: 0.5rem;
      text-align: left;
      vertical-align: top;
      border-bottom: 1px solid var(--mat-sys-outline-variant);
      white-space: pre-wrap;
    }
    tr.changed {
      background: var(--mat-sys-tertiary-container);
      color: var(--mat-sys-on-tertiary-container);
    }
    .changed-label {
      display: inline-block;
      margin-left: 0.35rem;
      font-size: 0.75rem;
      font-weight: 700;
      text-transform: uppercase;
    }
    .error {
      color: var(--mat-sys-error);
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AssetActionDialogComponent {
  readonly data = inject<AssetActionDialogData>(MAT_DIALOG_DATA);
  private readonly ref = inject(MatDialogRef<AssetActionDialogComponent>);
  private readonly service = inject(AssetTrackingService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly siteSearchTerms = new Subject<string>();
  private readonly responsibleUserSearchTerms = new Subject<string>();
  private siteSearchRevision = 0;
  private responsibleUserSearchRevision = 0;

  readonly classifications = ASSET_CLASSIFICATIONS;
  readonly conditions = ASSET_CONDITIONS;
  readonly statuses = REGISTRATION_STATUSES;
  readonly saving = signal(false);
  readonly conflict = signal(false);
  readonly error = signal('');
  readonly siteResults = signal<readonly SiteLookup[]>([]);
  readonly companyStockSiteResults = computed(() =>
    this.siteResults().filter((site) => site.isCompanyStock));
  readonly ordinarySiteResults = computed(() =>
    this.siteResults().filter((site) => !site.isCompanyStock));
  readonly responsibleUserResults = signal<readonly UserLookup[]>([]);
  readonly siteSearching = signal(false);
  readonly responsibleUserSearching = signal(false);
  readonly siteSearchAttempted = signal(false);
  readonly responsibleUserSearchAttempted = signal(false);
  readonly siteSearchError = signal('');
  readonly responsibleUserSearchError = signal('');
  readonly selectedSite = signal<SiteLookup | null>(null);
  readonly selectedResponsibleUser = signal<UserLookup | null>(null);
  readonly leaveUnassigned = signal(false);
  readonly siteSearchControl = new FormControl<string | SiteLookup | null>('');
  readonly responsibleUserSearchControl = new FormControl<string | UserLookup | null>('');
  readonly form = new FormGroup({
    classification: new FormControl<AssetClassification>(this.data.asset.classification, {
      nonNullable: true,
      validators: Validators.required
    }),
    name: new FormControl(this.data.asset.name, {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(200)]
    }),
    manufacturer: new FormControl(this.data.asset.manufacturer ?? '', {
      nonNullable: true,
      validators: Validators.maxLength(100)
    }),
    model: new FormControl(this.data.asset.model ?? '', {
      nonNullable: true,
      validators: Validators.maxLength(100)
    }),
    description: new FormControl(this.data.asset.description ?? '', {
      nonNullable: true,
      validators: Validators.maxLength(2000)
    }),
    manufacturerSerialNumber: new FormControl(
      this.data.asset.manufacturerSerialNumber ?? '',
      { nonNullable: true, validators: Validators.maxLength(100) }
    ),
    siteId: new FormControl('', {
      nonNullable: true,
      validators: this.data.kind === 'site' ? Validators.required : []
    }),
    condition: new FormControl(this.data.asset.condition, { nonNullable: true }),
    operationalStatus: new FormControl(this.data.asset.operationalStatus, {
      nonNullable: true
    }),
    responsibleUserId: new FormControl('', { nonNullable: true }),
    reason: new FormControl('', { nonNullable: true, validators: Validators.maxLength(2000) }),
    note: new FormControl('', { nonNullable: true, validators: Validators.maxLength(2000) })
  });
  private readonly draft = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue()
  });
  readonly refreshedAsset = signal<AssetDetails | null>(null);
  readonly currentAsset = computed(() => this.refreshedAsset() ?? this.data.asset);
  readonly canLeaveUnassigned = computed(() =>
    Boolean(
      this.currentAsset().responsibleUserId
      || this.currentAsset().pendingHandoverRecipientId
    )
  );
  readonly actionSelectionValid = computed(() => {
    if (this.data.kind === 'site') {
      const selectedSite = this.selectedSite();
      return selectedSite !== null && selectedSite.id !== this.currentAsset().siteId;
    }

    if (this.data.kind !== 'responsibility') {
      return true;
    }

    if (this.leaveUnassigned()) {
      return this.canLeaveUnassigned();
    }

    const selectedResponsibleUser = this.selectedResponsibleUser();
    return selectedResponsibleUser !== null
      && (
        selectedResponsibleUser.id !== this.currentAsset().responsibleUserId
        || this.currentAsset().pendingHandoverRecipientId !== null
      );
  });
  readonly displaySite = (value: string | SiteLookup | null): string =>
    typeof value === 'string' ? value : value ? this.siteDisplayLabel(value) : '';
  readonly displayResponsibleUser = (value: string | UserLookup | null): string =>
    typeof value === 'string' ? value : value?.displayName ?? '';
  readonly conflictRows = computed<readonly ConflictComparisonRow[]>(() => {
    const server = this.refreshedAsset();
    if (!server) {
      return [];
    }

    const draft = this.draft();
    const original = this.data.asset;
    const selectedSite = this.selectedSite();
    const selectedResponsibleUser = this.selectedResponsibleUser();
    const siteId = this.data.kind === 'site' ? selectedSite?.id ?? '' : original.siteId;
    const responsibleUserId = this.data.kind === 'responsibility'
      ? (this.leaveUnassigned() ? null : selectedResponsibleUser?.id ?? null)
      : original.responsibleUserId;
    const operationalStatus = this.data.kind === 'missing'
      ? 'Missing'
      : this.data.kind === 'retirement'
        ? 'Retired'
        : draft.operationalStatus;
    const reviewStatus = this.data.kind === 'registration-review'
      ? (this.data.accepted === true ? 'Active' : 'Rejected')
      : original.reviewStatus;

    return [
      this.comparison('Classification', draft.classification, server.classification),
      this.comparison('Name', draft.name, server.name),
      this.comparison('Manufacturer', draft.manufacturer || null, server.manufacturer),
      this.comparison('Model', draft.model || null, server.model),
      this.comparison('Description', draft.description || null, server.description),
      this.comparison(
        'Manufacturer serial number',
        draft.manufacturerSerialNumber || null,
        server.manufacturerSerialNumber
      ),
      this.comparison(
        'Site',
        siteId,
        server.siteId,
        this.data.kind === 'site'
          ? (selectedSite ? this.siteDisplayLabel(selectedSite) : null)
          : original.siteName,
        server.siteName
      ),
      this.comparison(
        'Responsible user',
        responsibleUserId,
        server.responsibleUserId,
        this.data.kind === 'responsibility'
          ? this.leaveUnassigned()
            ? 'Unassigned'
            : (selectedResponsibleUser
              ? this.responsibleUserDisplayLabel(selectedResponsibleUser)
              : null)
          : original.responsibleUserDisplayName,
        server.responsibleUserDisplayName
      ),
      this.comparison('Condition', draft.condition, server.condition),
      this.comparison('Operational status', operationalStatus, server.operationalStatus),
      this.comparison('Registration state', reviewStatus, server.reviewStatus),
      this.data.kind === 'qr-identifier'
        ? this.comparison(
            'QR identifier',
            'Generate a new identifier',
            server.qrIdentifier,
            'Generate a new identifier',
            server.qrIdentifier,
            true
          )
        : this.comparison('QR identifier', original.qrIdentifier, server.qrIdentifier)
    ];
  });

  constructor() {
    this.siteSearchTerms
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((value) => void this.searchSites(value));

    this.responsibleUserSearchTerms
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((value) => void this.searchEligibleResponsibleUsers(value));

    if (this.data.kind === 'site') {
      void this.searchSites('');
    }
  }

  onSiteSearchInput(event: Event): void {
    const searchTerm = (event.target as HTMLInputElement).value;
    this.siteSearchRevision += 1;
    this.siteResults.set([]);
    this.siteSearchAttempted.set(false);
    this.siteSearchError.set('');
    this.siteSearching.set(false);
    this.selectedSite.set(null);
    this.form.controls.siteId.setValue('', { emitEvent: false });
    this.siteSearchTerms.next(searchTerm);
  }

  onResponsibleUserSearchInput(event: Event): void {
    const searchTerm = (event.target as HTMLInputElement).value;
    this.responsibleUserSearchRevision += 1;
    this.responsibleUserResults.set([]);
    this.responsibleUserSearchAttempted.set(false);
    this.responsibleUserSearchError.set('');
    this.responsibleUserSearching.set(false);
    this.selectedResponsibleUser.set(null);
    this.form.controls.responsibleUserId.setValue('', { emitEvent: false });
    this.leaveUnassigned.set(false);
    this.responsibleUserSearchTerms.next(searchTerm);
  }

  selectSite(event: MatAutocompleteSelectedEvent): void {
    const site = event.option.value as SiteLookup;
    this.siteSearchRevision += 1;
    this.siteResults.set([]);
    this.siteSearchAttempted.set(false);
    this.siteSearchError.set('');
    this.selectedSite.set(site);
    this.form.controls.siteId.setValue(site.id, { emitEvent: false });
  }

  selectResponsibleUser(event: MatAutocompleteSelectedEvent): void {
    const user = event.option.value as UserLookup;
    this.responsibleUserSearchRevision += 1;
    this.responsibleUserResults.set([]);
    this.responsibleUserSearchAttempted.set(false);
    this.responsibleUserSearchError.set('');
    this.leaveUnassigned.set(false);
    this.selectedResponsibleUser.set(user);
    this.form.controls.responsibleUserId.setValue(user.id, { emitEvent: false });
  }

  setLeaveUnassigned(leaveUnassigned: boolean): void {
    this.leaveUnassigned.set(leaveUnassigned);
    if (!leaveUnassigned) {
      this.responsibleUserSearchControl.enable({ emitEvent: false });
      return;
    }

    this.responsibleUserSearchRevision += 1;
    this.responsibleUserResults.set([]);
    this.responsibleUserSearchAttempted.set(false);
    this.responsibleUserSearchError.set('');
    this.responsibleUserSearching.set(false);
    this.selectedResponsibleUser.set(null);
    this.responsibleUserSearchControl.setValue('', { emitEvent: false });
    this.responsibleUserSearchControl.disable({ emitEvent: false });
    this.form.controls.responsibleUserId.setValue('', { emitEvent: false });
  }

  retrySiteSearch(): void {
    const value = this.siteSearchControl.value;
    if (typeof value === 'string') {
      void this.searchSites(value);
    }
  }

  retryResponsibleUserSearch(): void {
    const value = this.responsibleUserSearchControl.value;
    if (typeof value === 'string') {
      void this.searchEligibleResponsibleUsers(value);
    }
  }

  async confirm(): Promise<void> {
    if (this.saving()) {
      return;
    }

    if (this.form.invalid || !this.actionSelectionValid()) {
      this.form.markAllAsTouched();
      if (this.data.kind === 'site') {
        this.siteSearchControl.markAsTouched();
        this.error.set('Select a different Site from the search results.');
      } else if (this.data.kind === 'responsibility') {
        this.responsibleUserSearchControl.markAsTouched();
        this.error.set('Select a different responsible user or explicitly leave the asset unassigned.');
      }
      return;
    }

    if (this.data.kind === 'responsibility' && !this.form.controls.reason.value.trim()) {
      this.error.set('A responsibility change requires a reason.');
      return;
    }

    const refreshedAsset = this.refreshedAsset();
    if (this.conflict() && !refreshedAsset) {
      this.error.set('Refresh the latest server values before reconfirming this draft.');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    try {
      const value = this.form.getRawValue();
      const concurrencyToken = refreshedAsset?.concurrencyToken
        ?? this.data.asset.concurrencyToken;
      let updated: AssetDetails;
      if (this.data.kind === 'details') {
        updated = await this.service.updateDetails(this.data.asset.id, {
          classification: value.classification,
          name: value.name,
          manufacturer: value.manufacturer || null,
          model: value.model || null,
          description: value.description || null,
          manufacturerSerialNumber: value.manufacturerSerialNumber || null,
          concurrencyToken,
          note: value.note || null
        });
      } else {
        const body: AssetActionRequest = {
          concurrencyToken,
          note: value.note || null
        };
        if (this.data.kind === 'registration-review') {
          body['accepted'] = this.data.accepted === true;
        }
        if (this.data.kind === 'site') {
          body['siteId'] = this.selectedSite()!.id;
        }
        if (this.data.kind === 'condition') {
          body['condition'] = value.condition;
        }
        if (this.data.kind === 'operational-status') {
          body['operationalStatus'] = value.operationalStatus;
        }
        if (this.data.kind === 'responsibility') {
          body['responsibleUserId'] = this.leaveUnassigned()
            ? null
            : this.selectedResponsibleUser()!.id;
          body['reason'] = value.reason;
        }
        updated = await this.service.mutate(this.data.asset.id, this.data.kind, body);
      }

      await this.service.invalidate(this.data.asset.id);
      this.ref.close(updated);
    } catch (error) {
      if (this.service.isConcurrencyConflict(error)) {
        this.conflict.set(true);
        await this.refreshConflict();
      } else {
        this.error.set('The action was rejected. Review the current state and supplied values.');
      }
    } finally {
      this.saving.set(false);
    }
  }

  async refreshConflict(): Promise<void> {
    this.saving.set(true);
    this.error.set('');
    this.refreshedAsset.set(null);
    try {
      await this.service.invalidate(this.data.asset.id);
      this.refreshedAsset.set(await this.service.getAsset(this.data.asset.id));
    } catch {
      this.error.set(
        'The record changed, but its latest values could not be loaded. Your draft is preserved.'
      );
    } finally {
      this.saving.set(false);
    }
  }

  private async searchSites(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const revision = ++this.siteSearchRevision;
    this.siteResults.set([]);
    this.siteSearchError.set('');
    this.siteSearchAttempted.set(searchTerm.length > 0);
    this.siteSearching.set(true);

    try {
      const sites = searchTerm.length > 0
        ? await this.service.searchSites(searchTerm)
        : await this.service.getDefaultSiteSuggestions();
      if (revision !== this.siteSearchRevision) {
        return;
      }

      const currentSiteId = this.currentAsset().siteId;
      this.siteResults.set(sites.filter((site) => site.id !== currentSiteId));
    } catch {
      if (revision === this.siteSearchRevision) {
        this.siteSearchError.set(
          searchTerm.length > 0
            ? 'Sites could not be searched.'
            : 'Site suggestions could not be loaded.');
      }
    } finally {
      if (revision === this.siteSearchRevision) {
        this.siteSearching.set(false);
      }
    }
  }

  private async searchEligibleResponsibleUsers(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const revision = ++this.responsibleUserSearchRevision;
    this.responsibleUserResults.set([]);
    this.responsibleUserSearchError.set('');
    this.responsibleUserSearchAttempted.set(searchTerm.length > 0);
    this.responsibleUserSearching.set(searchTerm.length > 0);
    if (!searchTerm) {
      return;
    }

    try {
      const users = await this.service.searchEligibleResponsibleUsers(searchTerm);
      if (revision !== this.responsibleUserSearchRevision) {
        return;
      }

      const current = this.currentAsset();
      this.responsibleUserResults.set(users.filter((user) =>
        current.pendingHandoverRecipientId !== null
        || user.id !== current.responsibleUserId
      ));
    } catch {
      if (revision === this.responsibleUserSearchRevision) {
        this.responsibleUserSearchError.set('Eligible responsible users could not be searched.');
      }
    } finally {
      if (revision === this.responsibleUserSearchRevision) {
        this.responsibleUserSearching.set(false);
      }
    }
  }

  private siteDisplayLabel(site: SiteLookup): string {
    const classification = site.isCompanyStock ? ' · Company Stock' : '';
    return `${site.name} · #${site.numberId} · ${site.address}${classification}`;
  }

  private responsibleUserDisplayLabel(user: UserLookup): string {
    return user.email
      ? `${user.displayName} · ${user.email}`
      : user.displayName;
  }

  private comparison(
    label: string,
    draftValue: unknown,
    serverValue: unknown,
    draftDisplay: unknown = draftValue,
    serverDisplay: unknown = serverValue,
    forceChanged = false
  ): ConflictComparisonRow {
    return {
      label,
      draft: this.displayValue(draftDisplay),
      server: this.displayValue(serverDisplay),
      changed: forceChanged || draftValue !== serverValue
    };
  }

  private displayValue(value: unknown): string {
    return value === null || value === undefined || value === '' ? '—' : String(value);
  }
}
