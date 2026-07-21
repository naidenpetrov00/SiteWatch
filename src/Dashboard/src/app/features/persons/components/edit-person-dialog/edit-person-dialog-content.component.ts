import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  OnInit,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';

import { DialogWizardTabsComponent } from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.component';
import {
  ADD_PERSON_WIZARD_TABS,
  AddPersonContactFormGroup,
  AddPersonContactTypeOption,
  AddPersonDialogFormGroup,
  AddPersonTypeOption,
  AddPersonWizardTabId,
  CONTACT_TYPES,
  PERSON_TYPES
} from '../add-person-dialog/add-person-dialog.types';
import {
  ADD_PERSON_VALIDATION_LIMITS,
  companyNameValidator,
  digitsOnlyValidator,
  lettersOnlyValidator,
  websiteValidator
} from '../add-person-dialog/add-person-dialog.validators';
import {
  createEditPersonAddressGroup,
  createEditPersonBankAccountGroup,
  createEditPersonContactGroup,
  EDIT_PERSON_DIALOG_FORM_ID
} from './edit-person-dialog.helpers';
import { AddPersonAddressesSectionComponent } from '../add-person-dialog/sections/add-person-addresses-section.component';
import { AddPersonBankAccountsSectionComponent } from '../add-person-dialog/sections/add-person-bank-accounts-section.component';
import { AddPersonContactsSectionComponent } from '../add-person-dialog/sections/add-person-contacts-section.component';
import { AddPersonPrimaryInfoSectionComponent } from '../add-person-dialog/sections/add-person-primary-info-section.component';

@Component({
  selector: 'app-edit-person-dialog-content',
  imports: [
    ReactiveFormsModule,
    DialogWizardTabsComponent,
    AddPersonPrimaryInfoSectionComponent,
    AddPersonAddressesSectionComponent,
    AddPersonContactsSectionComponent,
    AddPersonBankAccountsSectionComponent
  ],
  templateUrl: './edit-person-dialog-content.component.html',
  styleUrl: './edit-person-dialog-content.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditPersonDialogContentComponent implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly destroyRef = inject(DestroyRef);

  readonly personForm = input.required<AddPersonDialogFormGroup>();
  readonly saveRequested = output<void>();
  readonly wizardTabs = ADD_PERSON_WIZARD_TABS;
  readonly selectedTabId = signal<AddPersonWizardTabId>(this.wizardTabs[0].id);
  readonly validationLimits = ADD_PERSON_VALIDATION_LIMITS;
  readonly personTypes = PERSON_TYPES;
  readonly formId = EDIT_PERSON_DIALOG_FORM_ID;

  ngOnInit(): void {
    this.configurePrimaryInfoValidators();
    this.configureContactGroupValidators();
  }

  setSelectedTab(tabId: string): void {
    if (!this.wizardTabs.some((tab) => tab.id === tabId)) {
      return;
    }

    this.selectedTabId.set(tabId as AddPersonWizardTabId);
  }

  addAddress(): void {
    this.personForm().controls.addresses.push(createEditPersonAddressGroup(this.formBuilder));
  }

  removeAddress(index: number): void {
    this.personForm().controls.addresses.removeAt(index);
  }

  addContact(): void {
    const contactGroup = createEditPersonContactGroup(this.formBuilder);
    this.configureContactValueValidators(contactGroup);
    this.personForm().controls.contacts.push(contactGroup);
  }

  removeContact(index: number): void {
    this.personForm().controls.contacts.removeAt(index);
  }

  addBankAccount(): void {
    this.personForm().controls.bankAccounts.push(createEditPersonBankAccountGroup(this.formBuilder));
  }

  removeBankAccount(index: number): void {
    this.personForm().controls.bankAccounts.removeAt(index);
  }

  onSubmit(): void {
    this.saveRequested.emit();
  }

  private configurePrimaryInfoValidators(): void {
    const typeControl = this.personForm().controls.type;

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
    } = this.personForm().controls;

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

  private configureContactGroupValidators(): void {
    for (const contactGroup of this.personForm().controls.contacts.controls) {
      this.configureContactValueValidators(contactGroup);
    }
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
