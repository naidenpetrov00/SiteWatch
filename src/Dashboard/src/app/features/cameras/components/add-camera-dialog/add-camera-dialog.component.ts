import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardSiteLookup } from '../../../sites/models/dashboard-site-lookup.model';
import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardCamerasService } from '../../services/dashboard-cameras.service';

const BRAND_OPTIONS = ['Dahua'] as const;
const PROTOCOL_OPTIONS = ['Http', 'Https'] as const;

@Component({
  selector: 'app-add-camera-dialog',
  imports: [ReactiveFormsModule, MatAutocompleteModule, MatFormFieldModule, MatInputModule, MatSelectModule, DialogActionBarComponent, DialogShellComponent],
  templateUrl: './add-camera-dialog.component.html',
  styleUrl: './camera-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddCameraDialogComponent {
  private siteSearchRevision = 0;
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<AddCameraDialogComponent>);
  private readonly dashboardCamerasService = inject(DashboardCamerasService);
  private readonly dashboardSitesService = inject(DashboardSitesService);

  readonly brands = BRAND_OPTIONS;
  readonly protocols = PROTOCOL_OPTIONS;
  readonly siteResults = signal<readonly DashboardSiteLookup[]>([]);
  readonly siteSearchControl = this.formBuilder.control<string | DashboardSiteLookup | null>('');
  readonly formId = 'add-camera-dialog-form';
  readonly isCreating = () => this.dashboardCamerasService.createCameraMutation.isPending();
  readonly cameraForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.maxLength(100)]],
    brand: ['Dahua', [Validators.required]],
    model: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
    username: ['', [Validators.maxLength(50)]],
    password: ['', [Validators.maxLength(50)]],
    ipAddress: ['', [Validators.maxLength(39)]],
    rtspPort: [554, [Validators.required, Validators.min(1), Validators.max(65535)]],
    ptzPort: [443, [Validators.required, Validators.min(1), Validators.max(65535)]],
    protocol: ['Https', [Validators.required]],
    siteId: ['', [Validators.required]]
  });
  readonly displaySite = (value: string | DashboardSiteLookup | null): string => typeof value === 'string' ? value : value?.name ?? '';

  constructor() {
    this.siteSearchControl.valueChanges.pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      if (typeof value !== 'string') return;
      this.cameraForm.controls.siteId.setValue('', { emitEvent: false });
      void this.searchSites(value);
    });
  }

  closeDialog(): void { this.dialogRef.close(); }
  onSiteSelected(event: MatAutocompleteSelectedEvent): void {
    const site = event.option.value as DashboardSiteLookup;
    this.siteSearchRevision += 1;
    this.siteResults.set([]);
    this.cameraForm.controls.siteId.setValue(site.id, { emitEvent: false });
  }

  async submitCamera(): Promise<void> {
    if (this.cameraForm.invalid) { this.cameraForm.markAllAsTouched(); this.siteSearchControl.markAsTouched(); return; }
    const value = this.cameraForm.getRawValue();
    try {
      await this.dashboardCamerasService.createCamera({
        ...value,
        username: this.emptyToNull(value.username), password: this.emptyToNull(value.password), ipAddress: this.emptyToNull(value.ipAddress)
      });
      this.dialogRef.close(true);
    } catch { /* Keep the dialog open for retry. */ }
  }

  private async searchSites(rawSearch: string): Promise<void> {
    const searchTerm = rawSearch.trim();
    const revision = ++this.siteSearchRevision;
    this.siteResults.set([]);
    if (!searchTerm) return;
    try {
      const sites = await this.dashboardSitesService.searchSites(searchTerm);
      if (revision === this.siteSearchRevision) this.siteResults.set(sites);
    } catch { if (revision === this.siteSearchRevision) this.siteResults.set([]); }
  }

  private emptyToNull(value: string): string | null { return value.trim() || null; }
}
