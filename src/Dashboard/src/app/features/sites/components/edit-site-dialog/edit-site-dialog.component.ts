import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardSite } from '../../models/dashboard-site.model';
import {
  ALL_MEDIA_FILTER,
  formatMediaPolicyPreset,
  MAX_MEDIA_CATEGORY_COUNT,
  MAX_MEDIA_CATEGORY_LENGTH,
  normalizeMediaCategory,
  OTHER_MEDIA_CATEGORY
} from '../../models/site-media-policy-presets';
import { UpdateDashboardSiteRequest } from '../../models/update-dashboard-site-request.model';
import { DashboardSitesService } from '../../services/dashboard-sites.service';

@Component({
  selector: 'app-edit-site-dialog',
  imports: [
    DialogActionBarComponent,
    DialogShellComponent,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
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

  readonly formId = 'edit-site-dialog-form';
  readonly isSaving = () => this.dashboardSitesService.updateSiteMutation.isPending();
  readonly newMediaCategories = signal<readonly string[]>([]);
  readonly categoryError = signal<string | null>(null);
  readonly separatorKeyCodes = [ENTER, COMMA] as const;
  readonly otherCategory = OTHER_MEDIA_CATEGORY;
  readonly maxCategoryLength = MAX_MEDIA_CATEGORY_LENGTH;
  readonly maxCategoryCount = MAX_MEDIA_CATEGORY_COUNT;
  readonly savedCategories = this.site.mediaPolicy.categories;
  readonly savedCategoriesWithoutOther = this.savedCategories.filter(
    (category) => category !== OTHER_MEDIA_CATEGORY
  );
  readonly mediaPolicyDisplayName = formatMediaPolicyPreset(this.site.mediaPolicy.preset);
  readonly siteForm = this.formBuilder.nonNullable.group({
    name: [this.site.name, [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
    address: [this.site.address, [Validators.required, Validators.minLength(5), Validators.maxLength(200)]]
  });

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = `Edit Site #${this.site.numberId}`;
  readonly dialogSubtitle = 'Update the enabled site fields.';

  closeDialog(): void {
    this.dialogRef.close();
  }

  addCategory(event: MatChipInputEvent): void {
    const category = normalizeMediaCategory(event.value);
    if (category.length === 0) {
      event.chipInput.clear();
      return;
    }

    if (category.length > MAX_MEDIA_CATEGORY_LENGTH) {
      this.categoryError.set(
        `Categories cannot exceed ${MAX_MEDIA_CATEGORY_LENGTH} characters.`
      );
      return;
    }

    if (category.toLowerCase() === ALL_MEDIA_FILTER.toLowerCase()) {
      this.categoryError.set(`${ALL_MEDIA_FILTER} is reserved for the filter that shows every item.`);
      return;
    }

    const existingCategories = [...this.savedCategories, ...this.newMediaCategories()];
    if (
      existingCategories.some(
        (item) => item.toLowerCase() === category.toLowerCase()
      )
    ) {
      event.chipInput.clear();
      this.categoryError.set(null);
      return;
    }

    if (existingCategories.length >= MAX_MEDIA_CATEGORY_COUNT) {
      this.categoryError.set(`A policy can contain up to ${MAX_MEDIA_CATEGORY_COUNT} categories.`);
      return;
    }

    this.newMediaCategories.update((categories) => [...categories, category]);
    event.chipInput.clear();
    this.categoryError.set(null);
  }

  removeNewCategory(category: string): void {
    this.newMediaCategories.update((categories) =>
      categories.filter((item) => item !== category)
    );
    this.categoryError.set(null);
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
      mediaCategoriesToAdd: this.newMediaCategories()
    };

    try {
      await this.dashboardSitesService.updateSite(request);
      this.dialogRef.close(true);
    } catch {
      // Keep the dialog open so the user can retry.
    }
  }
}
