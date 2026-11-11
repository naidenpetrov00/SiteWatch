import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  signal
} from '@angular/core';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonToggleModule } from '@angular/material/button-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, distinctUntilChanged, map, merge, startWith, switchMap } from 'rxjs';

import {
  AddPersonDialogFormGroup,
  LEGAL_FORM_OPTIONS,
  PERSON_TYPE_OPTIONS,
  PERSON_TYPES,
  VAT_COUNTRY_CODE_OPTIONS
} from '../add-person-dialog.types';
import { ADD_PERSON_VALIDATION_LIMITS } from '../add-person-dialog.validators';
import { PersonInputSanitizerDirective } from '../directives/person-input-sanitizer.directive';
import { DashboardPersonLookup } from '../../../models/dashboard-person-lookup.model';
import { DashboardPersonsService } from '../../../services/dashboard-persons.service';
import {
  formatDashboardPersonLookupLabel,
  formatDashboardPersonLookupSubtitle
} from '../../../utils/dashboard-person-lookup-formatters';

@Component({
  selector: 'app-add-person-primary-info-section',
  imports: [
    ReactiveFormsModule,
    MatButtonToggleModule,
    MatCardModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    PersonInputSanitizerDirective
  ],
  templateUrl: './add-person-primary-info-section.component.html',
  styleUrl: './add-person-primary-info-section.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPersonPrimaryInfoSectionComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly dashboardPersonsService = inject(DashboardPersonsService);
  private nameSearchRevision = 0;

  readonly personForm = input.required<AddPersonDialogFormGroup>();
  readonly syncVatNumber = input(true);
  readonly showNameMatches = input(false);
  readonly personTypeOptions = PERSON_TYPE_OPTIONS;
  readonly legalFormOptions = LEGAL_FORM_OPTIONS;
  readonly vatCountryCodeOptions = VAT_COUNTRY_CODE_OPTIONS;
  readonly personTypes = PERSON_TYPES;
  readonly validationLimits = ADD_PERSON_VALIDATION_LIMITS;
  readonly nameMatches = signal<readonly DashboardPersonLookup[]>([]);
  readonly formatNameMatchLabel = formatDashboardPersonLookupLabel;
  readonly formatNameMatchSubtitle = formatDashboardPersonLookupSubtitle;

  ngOnInit(): void {
    if (this.syncVatNumber()) {
      this.configureVatNumberSync();
    }

    if (this.showNameMatches()) {
      this.configureNameMatches();
    }
  }

  private configureVatNumberSync(): void {
    const personForm = this.personForm();
    const typeControl = personForm.controls.type;
    const eikControl = personForm.controls.eik;
    const egnControl = personForm.controls.egn;
    const vatNumberControl = personForm.controls.vatNumber;

    typeControl.valueChanges
      .pipe(
        startWith(typeControl.value),
        switchMap((type) => {
          const sourceControl = type === PERSON_TYPES.company ? eikControl : egnControl;

          return sourceControl.valueChanges.pipe(startWith(sourceControl.value));
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((sourceValue) => {
        if (vatNumberControl.value === sourceValue) {
          return;
        }

        vatNumberControl.setValue(sourceValue, { emitEvent: false });
      });
  }

  private configureNameMatches(): void {
    const personForm = this.personForm();
    const nameChanges = merge(
      personForm.controls.type.valueChanges,
      personForm.controls.firstName.valueChanges,
      personForm.controls.middleName.valueChanges,
      personForm.controls.lastName.valueChanges,
      personForm.controls.companyName.valueChanges
    );

    nameChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.nameSearchRevision += 1;
        this.nameMatches.set([]);
      });

    nameChanges
      .pipe(
        startWith(null),
        map(() => this.getNameSearchTerm()),
        debounceTime(250),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((searchTerm) => {
        const revision = this.nameSearchRevision;

        if (searchTerm.length === 0) {
          return;
        }

        void this.loadNameMatches(searchTerm, revision);
      });
  }

  private getNameSearchTerm(): string {
    const personForm = this.personForm();

    if (personForm.controls.type.value === PERSON_TYPES.company) {
      return personForm.controls.companyName.value.trim();
    }

    return [
      personForm.controls.firstName.value,
      personForm.controls.middleName.value,
      personForm.controls.lastName.value
    ]
      .map((part) => part.trim())
      .filter((part) => part.length > 0)
      .join(' ');
  }

  private async loadNameMatches(searchTerm: string, revision: number): Promise<void> {
    try {
      const matches = await this.dashboardPersonsService.searchSuppliers(searchTerm);

      if (revision === this.nameSearchRevision) {
        this.nameMatches.set(matches);
      }
    } catch {
      if (revision === this.nameSearchRevision) {
        this.nameMatches.set([]);
      }
    }
  }
}
