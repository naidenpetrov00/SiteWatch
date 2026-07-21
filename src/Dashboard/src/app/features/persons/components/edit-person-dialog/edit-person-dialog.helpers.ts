import { FormBuilder, Validators } from '@angular/forms';

import {
  CONTACT_TYPES,
  AddPersonAddressFormGroup,
  AddPersonBankAccountFormGroup,
  AddPersonContactFormGroup,
  AddPersonContactTypeOption,
  AddPersonDialogFormGroup,
  AddPersonLegalFormOption,
  AddPersonTypeOption,
  AddPersonVatCountryCodeOption,
  PERSON_TYPES
} from '../add-person-dialog/add-person-dialog.types';
import {
  ADD_PERSON_VALIDATION_LIMITS,
  createAtMostOnePrimaryValidator,
  createRepeatableRowValidator,
  digitsOnlyValidator,
  websiteValidator
} from '../add-person-dialog/add-person-dialog.validators';
import { DashboardPersonDetails } from '../../models/dashboard-person-details.model';

export const EDIT_PERSON_DIALOG_FORM_ID = 'edit-person-dialog-form';

export function createEditPersonDialogForm(
  formBuilder: FormBuilder,
  initialPerson: DashboardPersonDetails
): AddPersonDialogFormGroup {
  const isCompany = initialPerson.type.toLowerCase() === 'company';
  const addresses = formBuilder.array(createAddressGroups(formBuilder, initialPerson.addresses), {
    validators: [createAtMostOnePrimaryValidator('addressMultiplePrimary')]
  });
  const contacts = formBuilder.array(createContactGroups(formBuilder, initialPerson.contacts), {
    validators: [createAtMostOnePrimaryValidator('contactMultiplePrimary')]
  });
  const bankAccounts = formBuilder.array(
    createBankAccountGroups(formBuilder, initialPerson.bankAccounts),
    {
      validators: [createAtMostOnePrimaryValidator('bankAccountMultiplePrimary')]
    }
  );

  return formBuilder.nonNullable.group({
    type: formBuilder.nonNullable.control<AddPersonTypeOption>({
      value: isCompany ? PERSON_TYPES.company : PERSON_TYPES.individual,
      disabled: true
    }),
    firstName: formBuilder.nonNullable.control(initialPerson.firstName ?? ''),
    middleName: formBuilder.nonNullable.control(initialPerson.middleName ?? ''),
    lastName: formBuilder.nonNullable.control(initialPerson.lastName ?? ''),
    companyName: formBuilder.nonNullable.control(initialPerson.companyName ?? ''),
    legalForm: formBuilder.nonNullable.control<AddPersonLegalFormOption | ''>({
      value: (initialPerson.legalForm as AddPersonLegalFormOption | null | undefined) ?? '',
      disabled: true
    }),
    egn: formBuilder.nonNullable.control({
      value: initialPerson.egn ?? '',
      disabled: true
    }),
    eik: formBuilder.nonNullable.control({
      value: initialPerson.eik ?? '',
      disabled: true
    }),
    vatCountryCode: formBuilder.nonNullable.control<AddPersonVatCountryCodeOption>({
      value: 'BG',
      disabled: true
    }),
    vatNumber: formBuilder.nonNullable.control({
      value: initialPerson.vatNumber ?? '',
      disabled: true
    }),
    addresses,
    contacts,
    bankAccounts
  }) as AddPersonDialogFormGroup;
}

export function createEditPersonAddressGroup(
  formBuilder: FormBuilder,
  initialValue: DashboardPersonDetails['addresses'][number] | null = null
): AddPersonAddressFormGroup {
  return formBuilder.nonNullable.group({
    addressLine: formBuilder.nonNullable.control(initialValue?.addressLine ?? '', {
      validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.addressLine)]
    }),
    additionalLine: formBuilder.nonNullable.control(initialValue?.additionalLine ?? '', {
      validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.additionalLine)]
    }),
    city: formBuilder.nonNullable.control(initialValue?.city ?? '', {
      validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.city)]
    }),
    postalCode: formBuilder.nonNullable.control(initialValue?.postalCode ?? '', {
      validators: [
        digitsOnlyValidator(),
        Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.postalCode)
      ]
    }),
    country: formBuilder.nonNullable.control(initialValue?.country ?? '', {
      validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.country)]
    }),
    details: formBuilder.nonNullable.control(initialValue?.details ?? '', {
      validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.details)]
    }),
    isPrimary: formBuilder.nonNullable.control(initialValue?.isPrimary ?? false),
    isActive: formBuilder.nonNullable.control(initialValue?.isActive ?? true)
  });
}

export function createEditPersonContactGroup(
  formBuilder: FormBuilder,
  initialValue: DashboardPersonDetails['contacts'][number] | null = null
): AddPersonContactFormGroup {
  const contactGroup = formBuilder.nonNullable.group(
    {
      contactType: formBuilder.nonNullable.control<AddPersonContactTypeOption>(
        (initialValue?.contactType ?? CONTACT_TYPES.phone) as AddPersonContactTypeOption
      ),
      value: formBuilder.nonNullable.control(initialValue?.value ?? ''),
      details: formBuilder.nonNullable.control(initialValue?.details ?? '', {
        validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.contactDetails)]
      }),
      isPrimary: formBuilder.nonNullable.control(initialValue?.isPrimary ?? false),
      isActive: formBuilder.nonNullable.control(initialValue?.isActive ?? true)
    },
    {
      validators: [
        createRepeatableRowValidator({
          errorKey: 'contactRowIncomplete',
          requiredField: 'value',
          presenceDefaults: {
            contactType: CONTACT_TYPES.phone,
            value: '',
            details: '',
            isPrimary: false,
            isActive: true
          }
        })
      ]
    }
  );

  configureContactValueValidators(contactGroup);
  return contactGroup;
}

export function createEditPersonBankAccountGroup(
  formBuilder: FormBuilder,
  initialValue: DashboardPersonDetails['bankAccounts'][number] | null = null
): AddPersonBankAccountFormGroup {
  return formBuilder.nonNullable.group(
    {
      iban: formBuilder.nonNullable.control(initialValue?.iban ?? '', {
        validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.iban)]
      }),
      bic: formBuilder.nonNullable.control(initialValue?.bic ?? '', {
        validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.bic)]
      }),
      bankName: formBuilder.nonNullable.control(initialValue?.bankName ?? '', {
        validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.bankName)]
      }),
      details: formBuilder.nonNullable.control(initialValue?.details ?? '', {
        validators: [Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.bankDetails)]
      }),
      isPrimary: formBuilder.nonNullable.control(initialValue?.isPrimary ?? false),
      isActive: formBuilder.nonNullable.control(initialValue?.isActive ?? true)
    },
    {
      validators: [
        createRepeatableRowValidator({
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
        })
      ]
    }
  );
}

function createAddressGroups(
  formBuilder: FormBuilder,
  addresses: DashboardPersonDetails['addresses']
): AddPersonAddressFormGroup[] {
  return addresses.length > 0
    ? addresses.map((address) => createEditPersonAddressGroup(formBuilder, address))
    : [createEditPersonAddressGroup(formBuilder)];
}

function createContactGroups(
  formBuilder: FormBuilder,
  contacts: DashboardPersonDetails['contacts']
): AddPersonContactFormGroup[] {
  return contacts.length > 0
    ? contacts.map((contact) => createEditPersonContactGroup(formBuilder, contact))
    : [createEditPersonContactGroup(formBuilder)];
}

function createBankAccountGroups(
  formBuilder: FormBuilder,
  bankAccounts: DashboardPersonDetails['bankAccounts']
): AddPersonBankAccountFormGroup[] {
  return bankAccounts.length > 0
    ? bankAccounts.map((bankAccount) => createEditPersonBankAccountGroup(formBuilder, bankAccount))
    : [createEditPersonBankAccountGroup(formBuilder)];
}

function configureContactValueValidators(contactGroup: AddPersonContactFormGroup): void {
  const valueControl = contactGroup.controls.value;
  const contactType = contactGroup.controls.contactType.value;

  if (contactType === CONTACT_TYPES.phone) {
    valueControl.setValidators([
      digitsOnlyValidator(),
      Validators.minLength(ADD_PERSON_VALIDATION_LIMITS.phoneValueMinLength),
      Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.phoneValueMaxLength)
    ]);
  } else if (contactType === CONTACT_TYPES.email) {
    valueControl.setValidators([
      Validators.email,
      Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.contactValue)
    ]);
  } else if (contactType === CONTACT_TYPES.website) {
    valueControl.setValidators([
      websiteValidator(),
      Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.contactValue)
    ]);
  } else {
    valueControl.setValidators([Validators.maxLength(ADD_PERSON_VALIDATION_LIMITS.contactValue)]);
  }

  valueControl.updateValueAndValidity({ emitEvent: false });
}
