import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';

import { DashboardPersonsService } from '../../services/dashboard-persons.service';
import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  DialogWizardTabsComponent
} from '../../../../shared/ui/dialog-wizard-tabs/dialog-wizard-tabs.component';
import {
  ADD_PERSON_WIZARD_TABS,
  AddPersonAddressFormGroup,
  AddPersonBankAccountFormGroup,
  AddPersonContactFormGroup,
  AddPersonContactTypeOption,
  AddPersonDialogFormGroup,
  AddPersonTypeOption,
  AddPersonVatCountryCodeOption,
  AddPersonWizardTabId
} from './add-person-dialog.types';
import { AddPersonAddressesSectionComponent } from './sections/add-person-addresses-section.component';
import { AddPersonBankAccountsSectionComponent } from './sections/add-person-bank-accounts-section.component';
import { AddPersonContactsSectionComponent } from './sections/add-person-contacts-section.component';
import { AddPersonPrimaryInfoSectionComponent } from './sections/add-person-primary-info-section.component';
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
export class AddPersonDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<AddPersonDialogComponent>);
  private readonly dashboardPersonsService = inject(DashboardPersonsService);

  readonly wizardTabs = ADD_PERSON_WIZARD_TABS;
  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = 'Add Person';
  readonly dialogSubtitle =
    'Capture the primary identity first, then move through addresses, contacts, and bank accounts.';
  readonly selectedTabId = signal<AddPersonWizardTabId>(this.wizardTabs[0].id);
  readonly isCreatingPerson = () => this.dashboardPersonsService.createPersonMutation.isPending();

  readonly personForm = this.createPersonForm();

  closeDialog(): void {
    this.dialogRef.close();
  }

  canSubmit(): boolean {
    return toCreateDashboardPersonRequest(this.personForm) !== null;
  }

  async submitPerson(): Promise<void> {
    const request = toCreateDashboardPersonRequest(this.personForm);

    if (!request) {
      this.personForm.markAllAsTouched();
      return;
    }

    try {
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

  private createPersonForm(): AddPersonDialogFormGroup {
    const addresses = this.formBuilder.array([this.createAddressGroup()]);
    const contacts = this.formBuilder.array([this.createContactGroup()]);
    const bankAccounts = this.formBuilder.array([this.createBankAccountGroup()]);

    return this.formBuilder.nonNullable.group({
      type: this.formBuilder.nonNullable.control<AddPersonTypeOption>('individual'),
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
    return this.formBuilder.nonNullable.group({
      addressLine: this.formBuilder.nonNullable.control(''),
      additionalLine: this.formBuilder.nonNullable.control(''),
      city: this.formBuilder.nonNullable.control(''),
      postalCode: this.formBuilder.nonNullable.control(''),
      country: this.formBuilder.nonNullable.control(''),
      details: this.formBuilder.nonNullable.control(''),
      isPrimary: this.formBuilder.nonNullable.control(false),
      isActive: this.formBuilder.nonNullable.control(true)
    });
  }

  private createContactGroup(): AddPersonContactFormGroup {
    return this.formBuilder.nonNullable.group({
      contactType: this.formBuilder.nonNullable.control<AddPersonContactTypeOption>('phone'),
      value: this.formBuilder.nonNullable.control(''),
      details: this.formBuilder.nonNullable.control(''),
      isPrimary: this.formBuilder.nonNullable.control(false),
      isActive: this.formBuilder.nonNullable.control(true)
    });
  }

  private createBankAccountGroup(): AddPersonBankAccountFormGroup {
    return this.formBuilder.nonNullable.group({
      iban: this.formBuilder.nonNullable.control(''),
      bic: this.formBuilder.nonNullable.control(''),
      bankName: this.formBuilder.nonNullable.control(''),
      details: this.formBuilder.nonNullable.control(''),
      isPrimary: this.formBuilder.nonNullable.control(false),
      isActive: this.formBuilder.nonNullable.control(true)
    });
  }
}
