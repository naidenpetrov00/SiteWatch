import { ChangeDetectionStrategy, Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

import { DashboardPersonsService } from '../../services/dashboard-persons.service';
import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  DialogWizardTabsComponent
} from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.component';
import {
  ADD_PERSON_WIZARD_TABS,
  CONTACT_TYPES,
  AddPersonAddressFormGroup,
  AddPersonBankAccountFormGroup,
  AddPersonContactFormGroup,
  AddPersonContactTypeOption,
  AddPersonDialogFormGroup,
  AddPersonTypeOption,
  PERSON_TYPES,
  AddPersonVatCountryCodeOption,
  AddPersonWizardTabId
} from './add-person-dialog.types';
import { AddPersonAddressesSectionComponent } from './sections/add-person-addresses-section.component';
import { AddPersonBankAccountsSectionComponent } from './sections/add-person-bank-accounts-section.component';
import { AddPersonContactsSectionComponent } from './sections/add-person-contacts-section.component';
import { AddPersonPrimaryInfoSectionComponent } from './sections/add-person-primary-info-section.component';
import {
  ADD_PERSON_VALIDATION_LIMITS,
  createRepeatableRowValidator,
  createAtMostOnePrimaryValidator,
  digitsOnlyValidator,
  requiredTextValidator
} from './add-person-dialog.validators';
import { toCreateDashboardPersonRequest } from '../../utils/dashboard-person-request.mapper';

@Component({
  selector: 'app-add-person-dialog',
  imports: [
    ReactiveFormsModule,
    DialogActionBarComponent,
    DialogShellComponent,
    DialogWizardTabsComponent,
    AddPersonPrimaryInfoSectionComponent,
    AddPersonAddressesSectionComponent,
    AddPersonContactsSectionComponent,
    AddPersonBankAccountsSectionComponent
  ],
  templateUrl: './add-person-dialog.component.html',
  styleUrl: './add-person-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AddPersonDialogComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<AddPersonDialogComponent>);
  private readonly dashboardPersonsService = inject(DashboardPersonsService);

  readonly wizardTabs = ADD_PERSON_WIZARD_TABS;
  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = 'Add Person';
  readonly dialogSubtitle =
    'Capture the primary identity first, then move through addresses, contacts, and bank accounts.';
  readonly selectedTabId = signal<AddPersonWizardTabId>(this.wizardTabs[0].id);
  readonly isCreatingPerson = () => this.dashboardPersonsService.createPersonMutation.isPending();
  readonly personTypes = PERSON_TYPES;
  readonly validationLimits = ADD_PERSON_VALIDATION_LIMITS;

  readonly personForm = this.createPersonForm();

  ngOnInit(): void {
    this.configurePrimaryInfoValidators();
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  async submitPerson(): Promise<void> {
    if (this.personForm.invalid) {
      this.personForm.markAllAsTouched();
      return;
    }

    try {
      const request = toCreateDashboardPersonRequest(this.personForm);
      await this.dashboardPersonsService.createPerson(request);
      this.dialogRef.close(true);
    } catch {
      // Keep the dialog open and let the form state remain intact.
    }
  }

  setSelectedTab(tabId: string): void {
    if (!this.wizardTabs.some((tab) => tab.id === tabId)) {
      return;
    }

    this.selectedTabId.set(tabId as AddPersonWizardTabId);
  }

  addAddress(): void {
    this.personForm.controls.addresses.push(this.createAddressGroup());
  }

  removeAddress(index: number): void {
    const addresses = this.personForm.controls.addresses;

    if (addresses.length <= 1) {
      return;
    }

    addresses.removeAt(index);
  }

  addContact(): void {
    this.personForm.controls.contacts.push(this.createContactGroup());
  }

  removeContact(index: number): void {
    const contacts = this.personForm.controls.contacts;

    if (contacts.length <= 1) {
      return;
    }

    contacts.removeAt(index);
  }

  addBankAccount(): void {
    this.personForm.controls.bankAccounts.push(this.createBankAccountGroup());
  }

  removeBankAccount(index: number): void {
    const bankAccounts = this.personForm.controls.bankAccounts;

    if (bankAccounts.length <= 1) {
      return;
    }

    bankAccounts.removeAt(index);
  }

  private configurePrimaryInfoValidators(): void {
    const typeControl = this.personForm.controls.type;

    this.applyPrimaryInfoValidators(typeControl.value);
    typeControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((type) => {
      this.applyPrimaryInfoValidators(type);
    });
  }

  private applyPrimaryInfoValidators(type: AddPersonTypeOption): void {
    const { firstName, middleName, lastName, companyName, egn, eik, vatNumber } = this.personForm.controls;

    if (type === this.personTypes.individual) {
      firstName.setValidators([requiredTextValidator(), Validators.maxLength(this.validationLimits.firstName)]);
      middleName.setValidators([Validators.maxLength(this.validationLimits.middleName)]);
      lastName.setValidators([requiredTextValidator(), Validators.maxLength(this.validationLimits.lastName)]);
      companyName.clearValidators();
      egn.setValidators([
        requiredTextValidator(),
        digitsOnlyValidator(),
        Validators.minLength(this.validationLimits.egnLength),
        Validators.maxLength(this.validationLimits.egnLength)
      ]);
      eik.clearValidators();
      vatNumber.setValidators([Validators.maxLength(this.validationLimits.vatNumberLength)]);
    } else {
      firstName.clearValidators();
      middleName.clearValidators();
      lastName.clearValidators();
      companyName.setValidators([requiredTextValidator(), Validators.maxLength(this.validationLimits.companyName)]);
      egn.clearValidators();
      eik.setValidators([
        requiredTextValidator(),
        digitsOnlyValidator(),
        Validators.minLength(this.validationLimits.eikMinLength),
        Validators.maxLength(this.validationLimits.eikMaxLength)
      ]);
      vatNumber.setValidators([Validators.maxLength(this.validationLimits.vatNumberLength)]);
    }

    for (const control of [firstName, middleName, lastName, companyName, egn, eik, vatNumber]) {
      control.updateValueAndValidity({ emitEvent: false });
    }
  }

  private createPersonForm(): AddPersonDialogFormGroup {
    const addresses = this.formBuilder.array([this.createAddressGroup()], {
      validators: [createAtMostOnePrimaryValidator('addressMultiplePrimary')]
    });
    const contacts = this.formBuilder.array([this.createContactGroup()], {
      validators: [createAtMostOnePrimaryValidator('contactMultiplePrimary')]
    });
    const bankAccounts = this.formBuilder.array([this.createBankAccountGroup()], {
      validators: [createAtMostOnePrimaryValidator('bankAccountMultiplePrimary')]
    });

    return this.formBuilder.nonNullable.group({
      type: this.formBuilder.nonNullable.control<AddPersonTypeOption>(this.personTypes.individual),
      firstName: this.formBuilder.nonNullable.control(''),
      middleName: this.formBuilder.nonNullable.control(''),
      lastName: this.formBuilder.nonNullable.control(''),
      companyName: this.formBuilder.nonNullable.control(''),
      egn: this.formBuilder.nonNullable.control(''),
      eik: this.formBuilder.nonNullable.control(''),
      vatCountryCode: this.formBuilder.nonNullable.control<AddPersonVatCountryCodeOption>('BG'),
      vatNumber: this.formBuilder.nonNullable.control(''),
      addresses,
      contacts,
      bankAccounts
    }) as AddPersonDialogFormGroup;
  }

  private createAddressGroup(): AddPersonAddressFormGroup {
    return this.formBuilder.nonNullable.group(
      {
        addressLine: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.addressLine)]
        }),
        additionalLine: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.additionalLine)]
        }),
        city: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.city)]
        }),
        postalCode: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.postalCode)]
        }),
        country: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.country)]
        }),
        details: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.details)]
        }),
        isPrimary: this.formBuilder.nonNullable.control(false),
        isActive: this.formBuilder.nonNullable.control(true)
      },
      {
        validators: [createRepeatableRowValidator({
          errorKey: 'addressRowIncomplete',
          requiredField: 'addressLine',
          presenceDefaults: {
            addressLine: '',
            additionalLine: '',
            city: '',
            postalCode: '',
            country: '',
            details: '',
            isPrimary: false,
            isActive: true
          }
        })]
      }
    );
  }

  private createContactGroup(): AddPersonContactFormGroup {
    return this.formBuilder.nonNullable.group(
      {
        contactType: this.formBuilder.nonNullable.control<AddPersonContactTypeOption>(
          CONTACT_TYPES.phone
        ),
        value: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.contactValue)]
        }),
        details: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.contactDetails)]
        }),
        isPrimary: this.formBuilder.nonNullable.control(false),
        isActive: this.formBuilder.nonNullable.control(true)
      },
      {
        validators: [createRepeatableRowValidator({
          errorKey: 'contactRowIncomplete',
          requiredField: 'value',
          presenceDefaults: {
            contactType: CONTACT_TYPES.phone,
            value: '',
            details: '',
            isPrimary: false,
            isActive: true
          }
        })]
      }
    );
  }

  private createBankAccountGroup(): AddPersonBankAccountFormGroup {
    return this.formBuilder.nonNullable.group(
      {
        iban: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.iban)]
        }),
        bic: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.bic)]
        }),
        bankName: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.bankName)]
        }),
        details: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.bankDetails)]
        }),
        isPrimary: this.formBuilder.nonNullable.control(false),
        isActive: this.formBuilder.nonNullable.control(true)
      },
      {
        validators: [createRepeatableRowValidator({
          errorKey: 'bankAccountRowIncomplete',
          requiredField: 'iban',
          presenceDefaults: {
            iban: '',
            bic: '',
            bankName: '',
            details: '',
            isPrimary: false,
            isActive: true
          }
        })]
      }
    );
  }
}
