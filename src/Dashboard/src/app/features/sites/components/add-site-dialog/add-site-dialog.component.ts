import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { SITE_MEDIA_POLICY_PRESETS } from '../../models/site-media-policy-presets';
import { DashboardSitesService } from '../../services/dashboard-sites.service';

@Component({
  selector: 'app-add-site-dialog',
  imports: [
    DialogActionBarComponent,
    DialogShellComponent,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    ReactiveFormsModule
  ],
  templateUrl: './add-site-dialog.component.html',
  styleUrl: './add-site-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddSiteDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<AddSiteDialogComponent>);
  private readonly dashboardSitesService = inject(DashboardSitesService);

  readonly mediaPolicyPresets = SITE_MEDIA_POLICY_PRESETS;
  readonly formId = 'add-site-dialog-form';
  readonly isCreating = () => this.dashboardSitesService.createSiteMutation.isPending();
  readonly siteForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
    address: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
    mediaPolicyPreset: [this.mediaPolicyPresets[0], [Validators.required]]
  });

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = 'Add Site';
  readonly dialogSubtitle = 'Enter the details for the new site.';

  closeDialog(): void {
    this.dialogRef.close();
  }

  async submitSite(): Promise<void> {
    if (this.siteForm.invalid) {
      this.siteForm.markAllAsTouched();
      return;
    }

    try {
      await this.dashboardSitesService.createSite(this.siteForm.getRawValue());
      this.dialogRef.close(true);
    } catch {
      // Keep the dialog open so the user can retry.
    }
  }
}
