import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime, distinctUntilChanged, filter } from 'rxjs';

import { DatepickerComponent } from '../../../../shared/ui/datepicker/datepicker.component';
import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardUserLookup } from '../../../users/models/dashboard-user-lookup.model';
import { DashboardUsersService } from '../../../users/services/dashboard-users.service';
import { DashboardSite } from '../../models/dashboard-site.model';
import { ALL_MEDIA_FILTER, formatMediaPolicyPreset, MAX_MEDIA_CATEGORY_COUNT, MAX_MEDIA_CATEGORY_LENGTH, normalizeMediaCategory, OTHER_MEDIA_CATEGORY } from '../../models/site-media-policy-presets';
import { SITE_STATUSES } from '../../models/site-statuses';
import { UpdateDashboardSiteRequest } from '../../models/update-dashboard-site-request.model';
import { DashboardSitesService } from '../../services/dashboard-sites.service';
import { siteDateRangeValidator } from '../site-date-range.validator';

@Component({
  selector: 'app-edit-site-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent, MatChipsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatAutocompleteModule, DatepickerComponent, ReactiveFormsModule],
  templateUrl: './edit-site-dialog.component.html',
  styleUrl: './edit-site-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditSiteDialogComponent {
  private managerSearchRevision = 0;
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<EditSiteDialogComponent>);
  private readonly dashboardSitesService = inject(DashboardSitesService);
  private readonly dashboardUsersService = inject(DashboardUsersService);
  readonly site = inject(MAT_DIALOG_DATA) as DashboardSite;

  readonly siteStatuses = SITE_STATUSES;
  readonly managerSearchResults = signal<readonly DashboardUserLookup[]>([]);
  readonly managerSearchControl = this.formBuilder.control<string | DashboardUserLookup | null>(this.site.managerDisplayName);
  readonly newMediaCategories = signal<readonly string[]>([]);
  readonly categoryError = signal<string | null>(null);
  readonly separatorKeyCodes = [ENTER, COMMA] as const;
  readonly otherCategory = OTHER_MEDIA_CATEGORY;
  readonly maxCategoryLength = MAX_MEDIA_CATEGORY_LENGTH;
  readonly maxCategoryCount = MAX_MEDIA_CATEGORY_COUNT;
  readonly savedCategories = this.site.mediaPolicy.categories;
  readonly savedCategoriesWithoutOther = this.savedCategories.filter((category) => category !== OTHER_MEDIA_CATEGORY);
  readonly mediaPolicyDisplayName = formatMediaPolicyPreset(this.site.mediaPolicy.preset);
  readonly formId = 'edit-site-dialog-form';
  readonly isSaving = () => this.dashboardSitesService.updateSiteMutation.isPending();
  readonly siteForm = this.formBuilder.nonNullable.group(
    {
      name: [this.site.name, [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
      address: [this.site.address, [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
      managerId: [this.site.managerId, [Validators.required]],
      startDate: this.formBuilder.control<Date | null>(this.fromDateOnly(this.site.startDate), [Validators.required]),
      endDate: this.formBuilder.control<Date | null>(this.fromDateOnly(this.site.endDate)),
      status: [this.site.status, [Validators.required]]
    },
    { validators: siteDateRangeValidator() }
  );

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = `Edit Site #${this.site.numberId}`;
  readonly dialogSubtitle = 'Update the enabled site fields.';
  readonly displayManagerSearchValue = (value: string | DashboardUserLookup | null): string => typeof value === 'string' ? value : value?.displayName ?? '';

  constructor() {
    this.managerSearchControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((value) => {
      if (typeof value !== 'string') return;
      this.managerSearchRevision += 1;
      this.managerSearchResults.set([]);
      this.siteForm.controls.managerId.setValue('', { emitEvent: false });
    });
    this.managerSearchControl.valueChanges.pipe(filter((value): value is string => typeof value === 'string'), debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef)).subscribe((value) => void this.searchManagers(value));
  }

  onManagerSelected(event: MatAutocompleteSelectedEvent): void {
    const manager = event.option.value as DashboardUserLookup;
    this.managerSearchRevision += 1;
    this.managerSearchResults.set([]);
    this.siteForm.controls.managerId.setValue(manager.id, { emitEvent: false });
  }
  closeDialog(): void { this.dialogRef.close(); }

  addCategory(event: MatChipInputEvent): void {
    const category = normalizeMediaCategory(event.value);
    if (!category) { event.chipInput.clear(); return; }
    if (category.length > MAX_MEDIA_CATEGORY_LENGTH) { this.categoryError.set(`Categories cannot exceed ${MAX_MEDIA_CATEGORY_LENGTH} characters.`); return; }
    if (category.toLowerCase() === ALL_MEDIA_FILTER.toLowerCase()) { this.categoryError.set(`${ALL_MEDIA_FILTER} is reserved for the filter that shows every item.`); return; }
    const existingCategories = [...this.savedCategories, ...this.newMediaCategories()];
    if (existingCategories.some((item) => item.toLowerCase() === category.toLowerCase())) { event.chipInput.clear(); this.categoryError.set(null); return; }
    if (existingCategories.length >= MAX_MEDIA_CATEGORY_COUNT) { this.categoryError.set(`A policy can contain up to ${MAX_MEDIA_CATEGORY_COUNT} categories.`); return; }
    this.newMediaCategories.update((categories) => [...categories, category]);
    event.chipInput.clear(); this.categoryError.set(null);
  }
  removeNewCategory(category: string): void { this.newMediaCategories.update((categories) => categories.filter((item) => item !== category)); this.categoryError.set(null); }

  async saveSite(): Promise<void> {
    if (this.siteForm.invalid) { this.siteForm.markAllAsTouched(); this.managerSearchControl.markAsTouched(); return; }
    const value = this.siteForm.getRawValue();
    const request: UpdateDashboardSiteRequest = { id: this.site.id, name: value.name, address: value.address, managerId: value.managerId, startDate: this.toDateOnly(value.startDate!), endDate: value.endDate ? this.toDateOnly(value.endDate) : null, status: value.status, mediaPolicyPreset: this.site.mediaPolicy.preset, mediaCategoriesToAdd: this.newMediaCategories() };
    try { await this.dashboardSitesService.updateSite(request); this.dialogRef.close(true); } catch { /* Keep the dialog open so the user can retry. */ }
  }

  private async searchManagers(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim(); const searchRevision = ++this.managerSearchRevision; this.managerSearchResults.set([]);
    if (!searchTerm) return;
    try { const managers = await this.dashboardUsersService.searchUsers(searchTerm); if (searchRevision === this.managerSearchRevision) this.managerSearchResults.set(managers); }
    catch { if (searchRevision === this.managerSearchRevision) this.managerSearchResults.set([]); }
  }
  private fromDateOnly(value: string | null): Date | null { return value ? new Date(`${value}T00:00:00`) : null; }
  private toDateOnly(value: Date): string { return `${value.getFullYear()}-${String(value.getMonth() + 1).padStart(2, '0')}-${String(value.getDate()).padStart(2, '0')}`; }
}
