import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatChipInputEvent, MatChipsModule } from '@angular/material/chips';
import { MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { DatepickerComponent } from '../../../../shared/ui/datepicker/datepicker.component';
import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardUserLookup } from '../../../users/models/dashboard-user-lookup.model';
import { DashboardUsersService } from '../../../users/services/dashboard-users.service';
import {
  ALL_MEDIA_FILTER,
  MAX_MEDIA_CATEGORY_COUNT,
  MAX_MEDIA_CATEGORY_LENGTH,
  normalizeMediaCategory,
  OTHER_MEDIA_CATEGORY,
  SiteMediaPolicyPreset,
  SiteMediaPolicyPresetDefinition
} from '../../models/site-media-policy-presets';
import { SITE_STATUSES } from '../../models/site-statuses';
import { DashboardSitesService } from '../../services/dashboard-sites.service';
import { getSiteSaveError } from '../../utils/site-save-error';
import { siteDateRangeValidator } from '../site-date-range.validator';

@Component({
  selector: 'app-add-site-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent, MatButtonModule, MatChipsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatAutocompleteModule, DatepickerComponent, ReactiveFormsModule],
  templateUrl: './add-site-dialog.component.html',
  styleUrl: './add-site-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddSiteDialogComponent {
  private managerSearchRevision = 0;
  private primaryClientSearchRevision = 0;
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<AddSiteDialogComponent>);
  private readonly dashboardSitesService = inject(DashboardSitesService);
  private readonly dashboardUsersService = inject(DashboardUsersService);

  readonly siteStatuses = SITE_STATUSES;
  readonly managerSearchResults = signal<readonly DashboardUserLookup[]>([]);
  readonly managerSearchControl = this.formBuilder.control<string | DashboardUserLookup | null>('');
  readonly primaryClientSearchResults = signal<readonly DashboardUserLookup[]>([]);
  readonly primaryClientSearchControl = this.formBuilder.control<string | DashboardUserLookup | null>('');
  readonly accessUserSearchResults = signal<readonly DashboardUserLookup[]>([]);
  readonly accessUserSearchControl = this.formBuilder.control<string | DashboardUserLookup | null>('');
  readonly accessUsers = signal<readonly DashboardUserLookup[]>([]);
  readonly mediaPolicyPresets = signal<readonly SiteMediaPolicyPresetDefinition[]>([]);
  readonly mediaCategories = signal<readonly string[]>([OTHER_MEDIA_CATEGORY]);
  readonly isLoadingPresets = signal(true);
  readonly presetLoadFailed = signal(false);
  readonly categoryError = signal<string | null>(null);
  readonly saveError = signal<string | null>(null);
  readonly separatorKeyCodes = [ENTER, COMMA] as const;
  readonly otherCategory = OTHER_MEDIA_CATEGORY;
  readonly maxCategoryLength = MAX_MEDIA_CATEGORY_LENGTH;
  readonly maxCategoryCount = MAX_MEDIA_CATEGORY_COUNT;
  readonly formId = 'add-site-dialog-form';
  readonly isCreating = () => this.dashboardSitesService.createSiteMutation.isPending();
  readonly siteForm = this.formBuilder.nonNullable.group(
    {
      name: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
      address: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
      managerId: ['', [Validators.required]],
      primaryClientUserId: this.formBuilder.control<string | null>(null),
      startDate: this.formBuilder.control<Date | null>(new Date(), [Validators.required]),
      endDate: this.formBuilder.control<Date | null>(null),
      status: ['Planning', [Validators.required]],
      mediaPolicyPreset: ['ApartmentRenovation' as SiteMediaPolicyPreset, [Validators.required]]
    },
    { validators: siteDateRangeValidator() }
  );

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = 'Add Site';
  readonly dialogSubtitle = 'Enter the details for the new site.';
  readonly displayManagerSearchValue = (value: string | DashboardUserLookup | null): string =>
    typeof value === 'string' ? value : value?.displayName ?? '';

  constructor() {
    this.managerSearchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') return;
        this.siteForm.controls.managerId.setValue('', { emitEvent: false });
        void this.searchManagers(value);
      });
    this.accessUserSearchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value === 'string') void this.searchAccessUsers(value);
      });
    this.primaryClientSearchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') return;
        this.primaryClientSearchRevision += 1;
        this.primaryClientSearchResults.set([]);
        this.siteForm.controls.primaryClientUserId.setValue(null, { emitEvent: false });
        void this.searchPrimaryClientUsers(value);
      });
    void this.loadMediaPolicyPresets();
  }

  onManagerSelected(event: MatAutocompleteSelectedEvent): void {
    const manager = event.option.value as DashboardUserLookup;
    this.managerSearchRevision += 1;
    this.managerSearchResults.set([]);
    this.siteForm.controls.managerId.setValue(manager.id, { emitEvent: false });
  }

  onAccessUserSelected(event: MatAutocompleteSelectedEvent): void {
    const user = event.option.value as DashboardUserLookup;
    this.addAccessUser(user);
    this.accessUserSearchControl.setValue('', { emitEvent: false });
    this.accessUserSearchResults.set([]);
  }

  removeAccessUser(user: DashboardUserLookup): void {
    if (this.isProtectedAccessUser(user)) return;
    this.accessUsers.update((users) => users.filter((item) => item.id !== user.id));
  }

  isProtectedAccessUser(user: DashboardUserLookup): boolean {
    return user.id === this.siteForm.controls.managerId.value
      || user.id === this.siteForm.controls.primaryClientUserId.value;
  }

  onPrimaryClientSelected(event: MatAutocompleteSelectedEvent): void {
    const user = event.option.value as DashboardUserLookup;
    this.primaryClientSearchRevision += 1;
    this.primaryClientSearchResults.set([]);
    this.siteForm.controls.primaryClientUserId.setValue(user.id, { emitEvent: false });
    this.addAccessUser(user);
  }

  clearPrimaryClient(): void {
    this.primaryClientSearchRevision += 1;
    this.primaryClientSearchResults.set([]);
    this.primaryClientSearchControl.setValue('', { emitEvent: false });
    this.siteForm.controls.primaryClientUserId.setValue(null);
  }

  selectPreset(preset: SiteMediaPolicyPreset): void {
    const definition = this.mediaPolicyPresets().find((item) => item.preset === preset);
    if (!definition) return;
    this.siteForm.controls.mediaPolicyPreset.setValue(preset);
    this.mediaCategories.set([...definition.categories]);
    this.categoryError.set(null);
  }

  addCategory(event: MatChipInputEvent): void {
    const category = normalizeMediaCategory(event.value);
    if (!category) { event.chipInput.clear(); return; }
    if (category.length > MAX_MEDIA_CATEGORY_LENGTH) { this.categoryError.set(`Categories cannot exceed ${MAX_MEDIA_CATEGORY_LENGTH} characters.`); return; }
    if (category.toLowerCase() === ALL_MEDIA_FILTER.toLowerCase()) { this.categoryError.set(`${ALL_MEDIA_FILTER} is reserved for the filter that shows every item.`); return; }
    const categories = this.mediaCategories();
    if (categories.some((item) => item.toLowerCase() === category.toLowerCase())) { event.chipInput.clear(); this.categoryError.set(null); return; }
    if (categories.length >= MAX_MEDIA_CATEGORY_COUNT) { this.categoryError.set(`A policy can contain up to ${MAX_MEDIA_CATEGORY_COUNT} categories.`); return; }
    this.markPolicyCustom();
    this.mediaCategories.set([...categories.filter((item) => item !== OTHER_MEDIA_CATEGORY), category, OTHER_MEDIA_CATEGORY]);
    event.chipInput.clear();
    this.categoryError.set(null);
  }

  removeCategory(category: string): void {
    if (category === OTHER_MEDIA_CATEGORY) return;
    this.markPolicyCustom();
    this.mediaCategories.update((categories) => categories.filter((item) => item !== category));
    this.categoryError.set(null);
  }

  closeDialog(): void { this.dialogRef.close(); }

  async submitSite(): Promise<void> {
    if (this.siteForm.invalid) { this.siteForm.markAllAsTouched(); this.managerSearchControl.markAsTouched(); return; }
    this.saveError.set(null);
    try {
      const value = this.siteForm.getRawValue();
      const accessUserIds = this.accessUsers().map((user) => user.id);
      await this.dashboardSitesService.createSite({ name: value.name, address: value.address, managerId: value.managerId, primaryClientUserId: value.primaryClientUserId, ...(accessUserIds.length > 0 ? { userIds: accessUserIds } : {}), startDate: this.toDateOnly(value.startDate!), endDate: value.endDate ? this.toDateOnly(value.endDate) : null, status: value.status, mediaPolicyPreset: value.mediaPolicyPreset, mediaCategories: this.mediaCategories() });
      this.dialogRef.close(true);
    } catch (error) { this.saveError.set(getSiteSaveError(error)); }
  }

  private async searchManagers(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const searchRevision = ++this.managerSearchRevision;
    this.managerSearchResults.set([]);
    if (!searchTerm) return;
    try {
      const managers = await this.dashboardUsersService.searchUsers(searchTerm);
      if (searchRevision === this.managerSearchRevision) this.managerSearchResults.set(managers);
    } catch { if (searchRevision === this.managerSearchRevision) this.managerSearchResults.set([]); }
  }

  private async searchAccessUsers(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    this.accessUserSearchResults.set([]);
    if (!searchTerm) return;
    try {
      this.accessUserSearchResults.set(await this.dashboardUsersService.searchUsers(searchTerm));
    } catch {
      this.accessUserSearchResults.set([]);
    }
  }

  private async searchPrimaryClientUsers(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const searchRevision = ++this.primaryClientSearchRevision;
    this.primaryClientSearchResults.set([]);
    if (!searchTerm) return;
    try {
      const users = await this.dashboardUsersService.searchUsers(searchTerm);
      if (searchRevision === this.primaryClientSearchRevision) this.primaryClientSearchResults.set(users);
    } catch {
      if (searchRevision === this.primaryClientSearchRevision) this.primaryClientSearchResults.set([]);
    }
  }

  private addAccessUser(user: DashboardUserLookup): void {
    if (!this.accessUsers().some((item) => item.id === user.id)) {
      this.accessUsers.update((users) => [...users, user]);
    }
  }

  private toDateOnly(value: Date): string { return `${value.getFullYear()}-${String(value.getMonth() + 1).padStart(2, '0')}-${String(value.getDate()).padStart(2, '0')}`; }

  private async loadMediaPolicyPresets(): Promise<void> {
    try {
      const presets = await this.dashboardSitesService.getMediaPolicyPresets();
      this.mediaPolicyPresets.set(presets);
      const defaultPreset = presets[0];
      if (defaultPreset) this.selectPreset(defaultPreset.preset); else this.presetLoadFailed.set(true);
    } catch { this.presetLoadFailed.set(true); } finally { this.isLoadingPresets.set(false); }
  }

  private markPolicyCustom(): void {
    if (this.siteForm.controls.mediaPolicyPreset.value !== 'Custom') this.siteForm.controls.mediaPolicyPreset.setValue('Custom');
  }
}
