import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import {
  ASSET_CLASSIFICATIONS,
  ASSET_CONDITIONS,
  AssetActionRequest,
  AssetClassification,
  AssetDetails,
  REGISTRATION_STATUSES
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
    MatDialogModule,
    MatButtonModule,
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
          <mat-form-field>
            <mat-label>Site ID</mat-label>
            <input matInput formControlName="siteId" />
          </mat-form-field>
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
          <mat-form-field>
            <mat-label>Worker ID (empty removes)</mat-label>
            <input matInput formControlName="responsibleWorkerId" />
          </mat-form-field>
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
        [disabled]="saving() || form.invalid || (conflict() && !refreshedAsset())"
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

  readonly classifications = ASSET_CLASSIFICATIONS;
  readonly conditions = ASSET_CONDITIONS;
  readonly statuses = REGISTRATION_STATUSES;
  readonly saving = signal(false);
  readonly conflict = signal(false);
  readonly error = signal('');
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
    siteId: new FormControl('', { nonNullable: true }),
    condition: new FormControl(this.data.asset.condition, { nonNullable: true }),
    operationalStatus: new FormControl(this.data.asset.operationalStatus, {
      nonNullable: true
    }),
    responsibleWorkerId: new FormControl(this.data.asset.responsibleWorkerId ?? '', {
      nonNullable: true
    }),
    reason: new FormControl('', { nonNullable: true, validators: Validators.maxLength(2000) }),
    note: new FormControl('', { nonNullable: true, validators: Validators.maxLength(2000) })
  });
  private readonly draft = toSignal(this.form.valueChanges, {
    initialValue: this.form.getRawValue()
  });
  readonly refreshedAsset = signal<AssetDetails | null>(null);
  readonly conflictRows = computed<readonly ConflictComparisonRow[]>(() => {
    const server = this.refreshedAsset();
    if (!server) {
      return [];
    }

    const draft = this.draft();
    const original = this.data.asset;
    const siteId = this.data.kind === 'site' ? draft.siteId : original.siteId;
    const workerId = this.data.kind === 'responsibility'
      ? draft.responsibleWorkerId || null
      : original.responsibleWorkerId;
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
        siteId === original.siteId ? original.siteName : siteId,
        server.siteName
      ),
      this.comparison(
        'Responsible worker',
        workerId,
        server.responsibleWorkerId,
        workerId === original.responsibleWorkerId
          ? original.responsibleWorkerDisplayName
          : workerId,
        server.responsibleWorkerDisplayName
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

  async confirm(): Promise<void> {
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
          body['siteId'] = value.siteId;
        }
        if (this.data.kind === 'condition') {
          body['condition'] = value.condition;
        }
        if (this.data.kind === 'operational-status') {
          body['operationalStatus'] = value.operationalStatus;
        }
        if (this.data.kind === 'responsibility') {
          body['responsibleWorkerId'] = value.responsibleWorkerId || null;
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
