import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  AbstractControl,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn,
  Validators,
  FormBuilder
} from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent
} from '@angular/material/autocomplete';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { debounceTime, distinctUntilChanged } from 'rxjs';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardCompanyPersonLookup } from '../../../persons/models/dashboard-company-person-lookup.model';
import { DashboardPersonsService } from '../../../persons/services/dashboard-persons.service';
import {
  CreateDashboardRetailerRequest,
  DashboardRetailerDetails
} from '../../models/dashboard-retailer.models';
import { DashboardRetailersService } from '../../services/dashboard-retailers.service';
import { getRetailerError } from '../../utils/retailer-error';

@Component({
  selector: 'app-retailer-dialog',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './retailer-dialog.component.html',
  styleUrl: './retailer-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerDialogComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<RetailerDialogComponent>);
  private readonly personsService = inject(DashboardPersonsService);
  private readonly retailersService = inject(DashboardRetailersService);
  readonly retailer = inject<DashboardRetailerDetails | null>(
    MAT_DIALOG_DATA,
    { optional: true }
  );
  private companySearchRevision = 0;

  readonly companySearchResults = signal<readonly DashboardCompanyPersonLookup[]>(
    []
  );
  readonly saveError = signal<string | null>(null);
  readonly companySearchControl = this.formBuilder.control<
    string | DashboardCompanyPersonLookup | null
  >(this.retailer?.companyPerson ?? null, Validators.required);
  readonly retailerForm = this.formBuilder.group({
    displayName: this.formBuilder.nonNullable.control(
      this.retailer?.displayName ?? '',
      [
        Validators.required,
        Validators.maxLength(200),
        meaningfulDisplayNameValidator()
      ]
    ),
    companyPersonId: this.formBuilder.control<string | null>(
      this.retailer?.companyPerson.id ?? null,
      Validators.required
    ),
    baseWebsiteUrl: this.formBuilder.nonNullable.control(
      this.retailer?.baseWebsiteUrl ?? '',
      [Validators.required, Validators.maxLength(2048), websiteOriginValidator()]
    ),
    notes: this.formBuilder.nonNullable.control(this.retailer?.notes ?? '', [
      Validators.maxLength(2000)
    ])
  });

  readonly formId = this.retailer
    ? 'edit-retailer-dialog-form'
    : 'add-retailer-dialog-form';
  readonly dialogTitle = this.retailer ? 'Edit Retailer' : 'Add Retailer';
  readonly dialogSubtitle = this.retailer
    ? `Update ${this.retailer.displayName} and its legal company.`
    : 'Connect a storefront or commercial website to an existing company Person.';
  readonly submitLabel = this.retailer ? 'Save Retailer' : 'Add Retailer';
  readonly isSaving = (): boolean =>
    this.retailer
      ? this.retailersService.updateRetailerMutation.isPending()
      : this.retailersService.createRetailerMutation.isPending();
  readonly displayCompany = (
    value: string | DashboardCompanyPersonLookup | null
  ): string => (typeof value === 'string' ? value : value?.displayName ?? '');

  ngOnInit(): void {
    this.companySearchControl.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((value) => {
        if (typeof value !== 'string') {
          return;
        }

        this.companySearchRevision += 1;
        this.companySearchResults.set([]);
        this.retailerForm.controls.companyPersonId.setValue(null);
      });

    this.companySearchControl.valueChanges
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((value) => {
        if (typeof value !== 'string') {
          return;
        }

        void this.searchCompanies(value, this.companySearchRevision);
      });
  }

  onCompanySelected(event: MatAutocompleteSelectedEvent): void {
    const company = event.option.value as DashboardCompanyPersonLookup;
    this.companySearchRevision += 1;
    this.companySearchResults.set([]);
    this.companySearchControl.setValue(company, { emitEvent: false });
    this.retailerForm.controls.companyPersonId.setValue(company.id);
    this.companySearchControl.markAsTouched();
  }

  formatCompanySubtitle(company: DashboardCompanyPersonLookup): string {
    const legalForm = company.legalForm ? ` · ${company.legalForm}` : '';
    return `EIK: ${company.eik} · VAT: ${company.vatNumber}${legalForm}`;
  }

  closeDialog(): void {
    this.dialogRef.close(false);
  }

  async submitRetailer(): Promise<void> {
    if (this.retailerForm.invalid || this.companySearchControl.invalid) {
      this.retailerForm.markAllAsTouched();
      this.companySearchControl.markAsTouched();
      return;
    }

    this.saveError.set(null);
    const request = this.toRequest();
    try {
      if (this.retailer) {
        await this.retailersService.updateRetailer({
          id: this.retailer.id,
          ...request
        });
      } else {
        await this.retailersService.createRetailer(request);
      }
      this.dialogRef.close(true);
    } catch (error) {
      this.saveError.set(getRetailerError(error));
    }
  }

  private async searchCompanies(
    searchTerm: string,
    revision: number
  ): Promise<void> {
    const normalizedSearchTerm = searchTerm.trim();
    if (!normalizedSearchTerm) {
      this.companySearchResults.set([]);
      return;
    }

    try {
      const results = await this.personsService.searchCompanies(
        normalizedSearchTerm
      );
      if (revision === this.companySearchRevision) {
        this.companySearchResults.set(results);
      }
    } catch {
      if (revision === this.companySearchRevision) {
        this.companySearchResults.set([]);
      }
    }
  }

  private toRequest(): CreateDashboardRetailerRequest {
    const value = this.retailerForm.getRawValue();
    return {
      displayName: value.displayName.trim().replace(/\s+/g, ' '),
      companyPersonId: value.companyPersonId!,
      baseWebsiteUrl: value.baseWebsiteUrl.trim(),
      notes: value.notes.trim() || null
    };
  }
}

function meaningfulDisplayNameValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null =>
    /[\p{L}\p{N}]/u.test(String(control.value ?? ''))
      ? null
      : { meaningfulName: true };
}

function websiteOriginValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = String(control.value ?? '').trim();
    if (!value) {
      return null;
    }

    try {
      const url = new URL(value);
      const valid =
        (url.protocol === 'http:' || url.protocol === 'https:') &&
        Boolean(url.hostname) &&
        !url.username &&
        !url.password &&
        !value.includes('?') &&
        !value.includes('#') &&
        url.pathname === '/' &&
        !url.search &&
        !url.hash;
      return valid ? null : { websiteOrigin: true };
    } catch {
      return { websiteOrigin: true };
    }
  };
}
