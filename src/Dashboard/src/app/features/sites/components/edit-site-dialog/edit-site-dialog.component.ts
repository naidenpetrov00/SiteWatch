import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardSite } from '../../models/dashboard-site.model';
import { SITE_MEDIA_POLICY_PRESETS } from '../../models/site-media-policy-presets';
import { UpdateDashboardSiteRequest } from '../../models/update-dashboard-site-request.model';
import { DashboardSitesService } from '../../services/dashboard-sites.service';

@Component({
  selector: 'app-edit-site-dialog',
  imports: [
    DialogActionBarComponent,
    DialogShellComponent,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ReactiveFormsModule
  ],
  templateUrl: './edit-site-dialog.component.html',
  styleUrl: './edit-site-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditSiteDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<EditSiteDialogComponent>);
  private readonly dashboardSitesService = inject(DashboardSitesService);
  readonly site = inject(MAT_DIALOG_DATA) as DashboardSite;

  readonly mediaPolicyPresets = SITE_MEDIA_POLICY_PRESETS;
  readonly formId = 'edit-site-dialog-form';
  readonly isSaving = () => this.dashboardSitesService.updateSiteMutation.isPending();
  readonly siteForm = this.formBuilder.nonNullable.group({
    name: [this.site.name, [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
    address: [this.site.address, [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
    mediaPolicyPreset: [this.site.mediaPolicy, [Validators.required]]
  });

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = `Edit Site #${this.site.numberId}`;
  readonly dialogSubtitle = 'Update the enabled site fields.';

  closeDialog(): void {
    this.dialogRef.close();
  }

  async saveSite(): Promise<void> {
    if (this.siteForm.invalid) {
      this.siteForm.markAllAsTouched();
      return;
    }

    const value = this.siteForm.getRawValue();
    const request: UpdateDashboardSiteRequest = {
      id: this.site.id,
      name: value.name,
      address: value.address,
      mediaPolicyPreset: value.mediaPolicyPreset
    };

    try {
      await this.dashboardSitesService.updateSite(request);
      this.dialogRef.close(true);
    } catch {
      // Keep the dialog open so the user can retry.
    }
  }
}
