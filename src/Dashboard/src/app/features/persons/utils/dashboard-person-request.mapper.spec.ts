import { FormBuilder } from '@angular/forms';

import {
  createEditPersonAddressGroup,
  createEditPersonDialogForm
} from '../components/edit-person-dialog/edit-person-dialog.helpers';
import { DashboardPersonDetails } from '../models/dashboard-person-details.model';
import {
  toCreateDashboardPersonRequest,
  toUpdateDashboardPersonRequest
} from './dashboard-person-request.mapper';

describe('dashboard person territorial address mapping', () => {
  const formBuilder = new FormBuilder();

  it('defaults a new address to Bulgaria without inventing territorial identifiers', () => {
    const address = createEditPersonAddressGroup(formBuilder);

    expect(address.getRawValue()).toMatchObject({
      country: 'Bulgaria',
      municipalityId: '',
      settlementId: '',
      districtId: '',
      quarterId: ''
    });
  });

  it('restores territorial identifiers and includes them in create and update requests', () => {
    const person = personDetails();
    const form = createEditPersonDialogForm(formBuilder, person);
    const address = form.controls.addresses.at(0);

    expect(address.getRawValue()).toMatchObject({
      country: 'Bulgaria',
      municipalityId: 'municipality-1',
      settlementId: 'settlement-1',
      districtId: 'district-1',
      quarterId: 'quarter-1'
    });

    expect(toCreateDashboardPersonRequest(form).addresses?.[0]).toMatchObject({
      country: 'Bulgaria',
      municipalityId: 'municipality-1',
      settlementId: 'settlement-1',
      districtId: 'district-1',
      quarterId: 'quarter-1'
    });
    expect(toUpdateDashboardPersonRequest(person, form).addresses[0]).toMatchObject({
      country: 'Bulgaria',
      municipalityId: 'municipality-1',
      settlementId: 'settlement-1',
      districtId: 'district-1',
      quarterId: 'quarter-1'
    });
  });
});

function personDetails(): DashboardPersonDetails {
  return {
    id: 'person-1',
    numberId: 1,
    type: 'Individual',
    displayName: 'Ivan Petrov',
    firstName: 'Ivan',
    middleName: null,
    lastName: 'Petrov',
    companyName: null,
    legalForm: null,
    egn: '9001010000',
    eik: null,
    vatNumber: 'BG9001010000',
    addresses: [{
      country: 'Bulgaria',
      municipalityId: 'municipality-1',
      settlementId: 'settlement-1',
      districtId: 'district-1',
      quarterId: 'quarter-1',
      postalCode: '1504',
      street: 'Knyaz Alexander Dondukov',
      number: '68',
      isPrimary: true,
      isActive: true
    }],
    contacts: [],
    bankAccounts: [],
    retailers: []
  };
}
