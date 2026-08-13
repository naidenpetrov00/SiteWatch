import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { MatDialogRef } from '@angular/material/dialog';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DatepickerComponent } from '../../../../shared/ui/datepicker/datepicker.component';
import { SITE_MEDIA_POLICY_PRESETS } from '../../models/site-media-policy-presets';
import { SITE_STATUSES } from '../../models/site-statuses';
import { DashboardSitesService } from '../../services/dashboard-sites.service';
import { DashboardUsersService } from '../../../users/services/dashboard-users.service';
import { DashboardUserLookup } from '../../../users/models/dashboard-user-lookup.model';

@Component({
  selector: 'app-add-site-dialog',
  imports: [
    DialogActionBarComponent,
    DialogShellComponent,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatAutocompleteModule,
    DatepickerComponent,
    ReactiveFormsModule
  ],
  templateUrl: './add-site-dialog.component.html',
  styleUrl: './add-site-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddSiteDialogComponent {
  private managerSearchRevision = 0;
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<AddSiteDialogComponent>);
  private readonly dashboardSitesService = inject(DashboardSitesService);
  private readonly dashboardUsersService = inject(DashboardUsersService);

  readonly mediaPolicyPresets = SITE_MEDIA_POLICY_PRESETS;
  readonly siteStatuses = SITE_STATUSES;
  readonly managerSearchResults = signal<readonly DashboardUserLookup[]>([]);
  readonly managerSearchControl = this.formBuilder.control<string | DashboardUserLookup | null>('');
  readonly formId = 'add-site-dialog-form';
  readonly isCreating = () => this.dashboardSitesService.createSiteMutation.isPending();
  readonly siteForm = this.formBuilder.nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
    address: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(200)]],
    managerId: ['', [Validators.required]],
    startDate: this.formBuilder.control<Date | null>(new Date(), [Validators.required]),
    endDate: this.formBuilder.control<Date | null>(null),
    status: ['Planning', [Validators.required]],
    mediaPolicyPreset: [this.mediaPolicyPresets[0], [Validators.required]]
  });

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = 'Add Site';
  readonly dialogSubtitle = 'Enter the details for the new site.';

  readonly displayManagerSearchValue = (value: string | DashboardUserLookup | null): string =>
    typeof value === 'string' ? value : value?.displayName ?? '';

  constructor() {
    this.managerSearchControl.valueChanges
      .pipe(debounceTime(250), distinctUntilChanged(), takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') {
          return;
        }

        this.siteForm.controls.managerId.setValue('', { emitEvent: false });
        void this.searchManagers(value);
      });
  }

  onManagerSelected(event: MatAutocompleteSelectedEvent): void {
    const manager = event.option.value as DashboardUserLookup;
    this.managerSearchRevision += 1;
    this.managerSearchResults.set([]);
    this.siteForm.controls.managerId.setValue(manager.id, { emitEvent: false });
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  async submitSite(): Promise<void> {
    if (this.siteForm.invalid) {
      this.siteForm.markAllAsTouched();
      this.managerSearchControl.markAsTouched();
      return;
    }

    if (this.hasInvalidDateRange()) {
      this.siteForm.controls.endDate.setErrors({ dateRange: true });
      this.siteForm.controls.endDate.markAsTouched();
      return;
    }

    try {
      const value = this.siteForm.getRawValue();
      await this.dashboardSitesService.createSite({
        name: value.name,
        address: value.address,
        managerId: value.managerId,
        startDate: this.toDateOnly(value.startDate!),
        endDate: value.endDate ? this.toDateOnly(value.endDate) : null,
        status: value.status,
        mediaPolicyPreset: value.mediaPolicyPreset
      });
      this.dialogRef.close(true);
    } catch {
      // Keep the dialog open so the user can retry.
    }
  }

  private async searchManagers(rawSearchTerm: string): Promise<void> {
    const searchTerm = rawSearchTerm.trim();
    const searchRevision = ++this.managerSearchRevision;
    this.managerSearchResults.set([]);

    if (!searchTerm) {
      return;
    }

    try {
      const managers = await this.dashboardUsersService.searchUsers(searchTerm);

      if (searchRevision === this.managerSearchRevision) {
        this.managerSearchResults.set(managers);
      }
    } catch {
      if (searchRevision === this.managerSearchRevision) {
        this.managerSearchResults.set([]);
      }
    }
  }

  private toDateOnly(value: Date): string {
    const year = value.getFullYear();
    const month = String(value.getMonth() + 1).padStart(2, '0');
    const day = String(value.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }

  private hasInvalidDateRange(): boolean {
    const { startDate, endDate } = this.siteForm.getRawValue();
    return startDate !== null && endDate !== null && endDate < startDate;
  }
}
