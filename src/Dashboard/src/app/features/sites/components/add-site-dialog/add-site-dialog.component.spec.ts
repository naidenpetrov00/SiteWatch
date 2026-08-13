import { TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardUsersService } from '../../../users/services/dashboard-users.service';
import { DashboardSitesService } from '../../services/dashboard-sites.service';
import { AddSiteDialogComponent } from './add-site-dialog.component';

describe('AddSiteDialogComponent', () => {
  const sitesService = { createSiteMutation: { isPending: () => false }, createSite: vi.fn() };
  const usersService = { searchUsers: vi.fn() };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AddSiteDialogComponent],
      providers: [
        { provide: DashboardSitesService, useValue: sitesService },
        { provide: DashboardUsersService, useValue: usersService },
        { provide: MatDialogRef, useValue: dialogRef }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('submits the selected manager, date-only schedule, and status in the create request', async () => {
    sitesService.createSite.mockResolvedValue({ id: 'site-1' });
    const component = TestBed.createComponent(AddSiteDialogComponent).componentInstance;
    component.onManagerSelected({ option: { value: { id: 'manager-1', displayName: 'Ada Lovelace', email: null } } } as never);
    component.siteForm.patchValue({
      name: 'North Gate',
      address: '1420 Industrial Park',
      startDate: new Date(2026, 2, 3),
      endDate: new Date(2026, 3, 18),
      status: 'Operational',
      mediaPolicyPreset: 'Regular'
    });

    await component.submitSite();

    expect(sitesService.createSite).toHaveBeenCalledWith({
      name: 'North Gate', address: '1420 Industrial Park', managerId: 'manager-1',
      startDate: '2026-03-03', endDate: '2026-04-18', status: 'Operational', mediaPolicyPreset: 'Regular'
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('does not submit a schedule whose end date is before its start date', async () => {
    const component = TestBed.createComponent(AddSiteDialogComponent).componentInstance;
    component.siteForm.patchValue({
      name: 'North Gate', address: '1420 Industrial Park', managerId: 'manager-1',
      startDate: new Date(2026, 2, 3), endDate: new Date(2026, 2, 2),
      status: 'Planning', mediaPolicyPreset: 'Regular'
    });

    await component.submitSite();

    expect(sitesService.createSite).not.toHaveBeenCalled();
  });
});
