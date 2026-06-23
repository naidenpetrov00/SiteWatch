import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  signal
} from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

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
  AddPersonLegalFormOption,
  AddPersonTypeOption,
  PERSON_TYPES,
  AddPersonVatCountryCodeOption,
  AddPersonWizardTabId
} from '../add-person-dialog/add-person-dialog.types';
import { AddPersonAddressesSectionComponent } from '../add-person-dialog/sections/add-person-addresses-section.component';
import { AddPersonBankAccountsSectionComponent } from '../add-person-dialog/sections/add-person-bank-accounts-section.component';
import { AddPersonContactsSectionComponent } from '../add-person-dialog/sections/add-person-contacts-section.component';
import { AddPersonPrimaryInfoSectionComponent } from '../add-person-dialog/sections/add-person-primary-info-section.component';
import {
  ADD_PERSON_VALIDATION_LIMITS,
  createRepeatableRowValidator,
  createAtMostOnePrimaryValidator,
  companyNameValidator,
  digitsOnlyValidator,
  lettersOnlyValidator,
  websiteValidator
} from '../add-person-dialog/add-person-dialog.validators';
import { DashboardPerson } from '../../models/dashboard-person.model';

@Component({
  selector: 'app-edit-person-dialog',
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
  templateUrl: './edit-person-dialog.component.html',
  styleUrl: './edit-person-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditPersonDialogComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);
  private readonly dialogRef = inject(MatDialogRef<EditPersonDialogComponent>);
  private readonly dialogData = inject(MAT_DIALOG_DATA, { optional: true }) as DashboardPerson | null;

  readonly wizardTabs = ADD_PERSON_WIZARD_TABS;
  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = this.dialogData ? `Edit Person #${this.dialogData.numberId}` : 'Edit Person';
  readonly dialogSubtitle =
    'Review the current details, update the fields you need, and save the record.';
  readonly selectedTabId = signal<AddPersonWizardTabId>(this.wizardTabs[0].id);
  readonly personTypes = PERSON_TYPES;
  readonly validationLimits = ADD_PERSON_VALIDATION_LIMITS;
  readonly submitLabel = 'Save and close';
  readonly secondarySubmitLabel = 'Save';
  readonly showSecondarySubmit = true;

  readonly personForm = this.createPersonForm(this.dialogData);

  ngOnInit(): void {
    this.configurePrimaryInfoValidators();
  }

  closeDialog(): void {
    this.dialogRef.close();
  }

  savePerson(): void {
    if (this.personForm.invalid) {
      this.personForm.markAllAsTouched();
      return;
    }

    console.log('Person edit payload', this.buildPersonEditPayload());
  }

  savePersonAndClose(): void {
    if (this.personForm.invalid) {
      this.personForm.markAllAsTouched();
      return;
    }

    console.log('Person edit payload', this.buildPersonEditPayload());
    this.dialogRef.close(true);
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
    this.personForm.controls.addresses.removeAt(index);
  }

  addContact(): void {
    this.personForm.controls.contacts.push(this.createContactGroup());
  }

  removeContact(index: number): void {
    this.personForm.controls.contacts.removeAt(index);
  }

  addBankAccount(): void {
    this.personForm.controls.bankAccounts.push(this.createBankAccountGroup());
  }

  removeBankAccount(index: number): void {
    this.personForm.controls.bankAccounts.removeAt(index);
  }

  private configurePrimaryInfoValidators(): void {
    const typeControl = this.personForm.controls.type;

    this.applyPrimaryInfoValidators(typeControl.value);
    typeControl.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe((type) => {
      this.applyPrimaryInfoValidators(type);
    });
  }

  private applyPrimaryInfoValidators(type: AddPersonTypeOption): void {
    const {
      firstName,
      middleName,
      lastName,
      companyName,
      legalForm,
      egn,
      eik,
      vatNumber
    } = this.personForm.controls;

    if (type === this.personTypes.individual) {
      firstName.setValidators([
        Validators.required,
        lettersOnlyValidator(),
        Validators.maxLength(this.validationLimits.firstName)
      ]);
      middleName.setValidators([
        lettersOnlyValidator(),
        Validators.maxLength(this.validationLimits.middleName)
      ]);
      lastName.setValidators([
        Validators.required,
        lettersOnlyValidator(),
        Validators.maxLength(this.validationLimits.lastName)
      ]);
      companyName.clearValidators();
      legalForm.clearValidators();
      egn.setValidators([
        Validators.required,
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
      companyName.setValidators([
        Validators.required,
        companyNameValidator(),
        Validators.maxLength(this.validationLimits.companyName)
      ]);
      legalForm.setValidators([Validators.required]);
      egn.clearValidators();
      eik.setValidators([
        Validators.required,
        digitsOnlyValidator(),
        Validators.minLength(this.validationLimits.eikMinLength),
        Validators.maxLength(this.validationLimits.eikMaxLength)
      ]);
      vatNumber.setValidators([Validators.maxLength(this.validationLimits.vatNumberLength)]);
    }

    for (const control of [firstName, middleName, lastName, companyName, legalForm, egn, eik, vatNumber]) {
      control.updateValueAndValidity({ emitEvent: false });
    }
  }

  private createPersonForm(initialPerson: DashboardPerson | null): AddPersonDialogFormGroup {
    const isCompany = this.isCompanyPerson(initialPerson);
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
      type: this.formBuilder.nonNullable.control<AddPersonTypeOption>({
        value: isCompany ? this.personTypes.company : this.personTypes.individual,
        disabled: true
      }),
      firstName: this.formBuilder.nonNullable.control(initialPerson?.firstName ?? ''),
      middleName: this.formBuilder.nonNullable.control(initialPerson?.middleName ?? ''),
      lastName: this.formBuilder.nonNullable.control(initialPerson?.lastName ?? ''),
      companyName: this.formBuilder.nonNullable.control(initialPerson?.companyName ?? ''),
      legalForm: this.formBuilder.nonNullable.control<AddPersonLegalFormOption | ''>({
        value: (initialPerson?.legalForm as AddPersonLegalFormOption | null | undefined) ?? '',
        disabled: true
      }),
      egn: this.formBuilder.nonNullable.control({
        value: initialPerson?.egn ?? '',
        disabled: true
      }),
      eik: this.formBuilder.nonNullable.control({
        value: initialPerson?.eik ?? '',
        disabled: true
      }),
      vatCountryCode: this.formBuilder.nonNullable.control<AddPersonVatCountryCodeOption>({
        value: 'BG',
        disabled: true
      }),
      vatNumber: this.formBuilder.nonNullable.control({
        value: initialPerson?.vatNumber ?? '',
        disabled: true
      }),
      addresses,
      contacts,
      bankAccounts
    }) as AddPersonDialogFormGroup;
  }

  private isCompanyPerson(person: DashboardPerson | null): boolean {
    return (person?.type ?? '').toLowerCase() === 'company';
  }

  private buildPersonEditPayload(): Readonly<Record<string, unknown>> {
    const {
      type,
      egn,
      legalForm,
      eik,
      vatCountryCode,
      vatNumber,
      ...editablePerson
    } = this.personForm.getRawValue();
    void type;

    return {
      id: this.dialogData?.id ?? null,
      numberId: this.dialogData?.numberId ?? null,
      ...editablePerson
    };
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
          validators: [
            digitsOnlyValidator(),
            Validators.maxLength(this.validationLimits.postalCode)
          ]
        }),
        country: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.country)]
        }),
        details: this.formBuilder.nonNullable.control('', {
          validators: [Validators.maxLength(this.validationLimits.details)]
        }),
        isPrimary: this.formBuilder.nonNullable.control(false),
        isActive: this.formBuilder.nonNullable.control(true)
      }
    );
  }

  private createContactGroup(): AddPersonContactFormGroup {
    const contactGroup = this.formBuilder.nonNullable.group(
      {
        contactType: this.formBuilder.nonNullable.control<AddPersonContactTypeOption>(
          CONTACT_TYPES.phone
        ),
        value: this.formBuilder.nonNullable.control(''),
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

    this.configureContactValueValidators(contactGroup);
    return contactGroup;
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

  private configureContactValueValidators(contactGroup: AddPersonContactFormGroup): void {
    const applyValidators = (contactType: AddPersonContactTypeOption): void => {
      const valueControl = contactGroup.controls.value;

      if (contactType === CONTACT_TYPES.phone) {
        valueControl.setValidators([
          digitsOnlyValidator(),
          Validators.minLength(this.validationLimits.phoneValueMinLength),
          Validators.maxLength(this.validationLimits.phoneValueMaxLength)
        ]);
      } else if (contactType === CONTACT_TYPES.email) {
        valueControl.setValidators([
          Validators.email,
          Validators.maxLength(this.validationLimits.contactValue)
        ]);
      } else if (contactType === CONTACT_TYPES.website) {
        valueControl.setValidators([
          websiteValidator(),
          Validators.maxLength(this.validationLimits.contactValue)
        ]);
      } else {
        valueControl.setValidators([Validators.maxLength(this.validationLimits.contactValue)]);
      }

      valueControl.updateValueAndValidity({ emitEvent: false });
    };

    applyValidators(contactGroup.controls.contactType.value);
    contactGroup.controls.contactType.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((contactType) => applyValidators(contactType));
  }
}
