import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
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
          <p class="conflict" role="alert">
            The record changed. Your input is preserved and current state has been refreshed.
            Review it, then confirm again.
          </p>
        }
        @if (error()) {
          <p class="error" role="alert">{{ error() }}</p>
        }
      </form>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-flat-button (click)="confirm()" [disabled]="saving() || form.invalid">
        Confirm
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
    .conflict {
      color: var(--mat-sys-tertiary);
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

  async confirm(): Promise<void> {
    if (this.data.kind === 'responsibility' && !this.form.controls.reason.value.trim()) {
      this.error.set('A responsibility change requires a reason.');
      return;
    }

    this.saving.set(true);
    this.error.set('');
    try {
      const value = this.form.getRawValue();
      let updated: AssetDetails;
      if (this.data.kind === 'details') {
        updated = await this.service.updateDetails(this.data.asset.id, {
          classification: value.classification,
          name: value.name,
          manufacturer: value.manufacturer || null,
          model: value.model || null,
          description: value.description || null,
          manufacturerSerialNumber: value.manufacturerSerialNumber || null,
          concurrencyToken: this.data.asset.concurrencyToken,
          note: value.note || null
        });
      } else {
        const body: AssetActionRequest = {
          concurrencyToken: this.data.asset.concurrencyToken,
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
        await this.service.invalidate(this.data.asset.id);
        this.data.asset = await this.service.getAsset(this.data.asset.id);
      } else {
        this.error.set('The action was rejected. Review the current state and supplied values.');
      }
    } finally {
      this.saving.set(false);
    }
  }
}
