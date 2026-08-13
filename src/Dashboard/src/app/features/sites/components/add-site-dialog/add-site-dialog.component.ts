import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  ALL_MEDIA_FILTER,
  MAX_MEDIA_CATEGORY_COUNT,
  MAX_MEDIA_CATEGORY_LENGTH,
  normalizeMediaCategory,
  OTHER_MEDIA_CATEGORY,
  SiteMediaPolicyPreset,
  SiteMediaPolicyPresetDefinition
} from '../../models/site-media-policy-presets';
import { DashboardSitesService } from '../../services/dashboard-sites.service';

@Component({
  selector: 'app-add-site-dialog',
  imports: [
    DialogActionBarComponent,
    DialogShellComponent,
    MatChipsModule,
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

  readonly mediaPolicyPresets = signal<readonly SiteMediaPolicyPresetDefinition[]>([]);
  readonly mediaCategories = signal<readonly string[]>([OTHER_MEDIA_CATEGORY]);
  readonly isLoadingPresets = signal(true);
  readonly presetLoadFailed = signal(false);
  readonly categoryError = signal<string | null>(null);
  readonly separatorKeyCodes = [ENTER, COMMA] as const;
  readonly otherCategory = OTHER_MEDIA_CATEGORY;
  readonly maxCategoryLength = MAX_MEDIA_CATEGORY_LENGTH;
  readonly maxCategoryCount = MAX_MEDIA_CATEGORY_COUNT;
  readonly formId = 'add-site-dialog-form';
  readonly isCreating = () => this.dashboardSitesService.createSiteMutation.isPending();
  readonly siteForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
    address: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
    mediaPolicyPreset: ['ApartmentRenovation' as SiteMediaPolicyPreset, [Validators.required]]
  });

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = 'Add Site';
  readonly dialogSubtitle = 'Enter the details for the new site.';

  constructor() {
    void this.loadMediaPolicyPresets();
  }

  selectPreset(preset: SiteMediaPolicyPreset): void {
    const definition = this.mediaPolicyPresets().find((item) => item.preset === preset);
    if (!definition) {
      return;
    }

    this.siteForm.controls.mediaPolicyPreset.setValue(preset);
    this.mediaCategories.set([...definition.categories]);
    this.categoryError.set(null);
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

    const categories = this.mediaCategories();
    if (categories.some((item) => item.toLowerCase() === category.toLowerCase())) {
      event.chipInput.clear();
      this.categoryError.set(null);
      return;
    }

    if (categories.length >= MAX_MEDIA_CATEGORY_COUNT) {
      this.categoryError.set(`A policy can contain up to ${MAX_MEDIA_CATEGORY_COUNT} categories.`);
      return;
    }

    this.markPolicyCustom();
    this.mediaCategories.set([
      ...categories.filter((item) => item !== OTHER_MEDIA_CATEGORY),
      category,
      OTHER_MEDIA_CATEGORY
    ]);
    event.chipInput.clear();
    this.categoryError.set(null);
  }

  removeCategory(category: string): void {
    if (category === OTHER_MEDIA_CATEGORY) {
      return;
    }

    this.markPolicyCustom();
    this.mediaCategories.update((categories) => categories.filter((item) => item !== category));
    this.categoryError.set(null);
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  async submitSite(): Promise<void> {
    if (this.siteForm.invalid) {
      this.siteForm.markAllAsTouched();
      return;
    }

    try {
      const value = this.siteForm.getRawValue();
      await this.dashboardSitesService.createSite({
        name: value.name,
        address: value.address,
        mediaPolicyPreset: value.mediaPolicyPreset,
        mediaCategories: this.mediaCategories()
      });
      this.dialogRef.close(true);
    } catch {
      // Keep the dialog open so the user can retry.
    }
  }

  private async loadMediaPolicyPresets(): Promise<void> {
    try {
      const presets = await this.dashboardSitesService.getMediaPolicyPresets();
      this.mediaPolicyPresets.set(presets);
      const defaultPreset = presets[0];
      if (defaultPreset) {
        this.selectPreset(defaultPreset.preset);
      } else {
        this.presetLoadFailed.set(true);
      }
    } catch {
      this.presetLoadFailed.set(true);
    } finally {
      this.isLoadingPresets.set(false);
    }
  }

  private markPolicyCustom(): void {
    if (this.siteForm.controls.mediaPolicyPreset.value !== 'Custom') {
      this.siteForm.controls.mediaPolicyPreset.setValue('Custom');
    }
  }
}
