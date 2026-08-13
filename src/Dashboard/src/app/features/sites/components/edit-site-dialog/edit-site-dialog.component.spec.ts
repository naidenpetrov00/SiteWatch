import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardUsersService } from '../../../users/services/dashboard-users.service';
import { DashboardSite } from '../../models/dashboard-site.model';
import { DashboardSitesService } from '../../services/dashboard-sites.service';
import { EditSiteDialogComponent } from './edit-site-dialog.component';

describe('EditSiteDialogComponent', () => {
  const site: DashboardSite = {
    id: 'site-1', numberId: 42, name: 'North Gate', address: '1420 Industrial Park',
    managerId: 'manager-1', managerDisplayName: 'Ada Lovelace', startDate: '2026-03-03',
    endDate: null, status: 'Planning', mediaPolicy: 'Regular'
  };
  const sitesService = { updateSiteMutation: { isPending: () => false }, updateSite: vi.fn() };
  const usersService = { searchUsers: vi.fn() };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditSiteDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: site },
        { provide: DashboardSitesService, useValue: sitesService },
        { provide: DashboardUsersService, useValue: usersService },
        { provide: MatDialogRef, useValue: dialogRef }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('preserves an ongoing schedule as a null end date when saving site metadata', async () => {
    sitesService.updateSite.mockResolvedValue(undefined);
    const component = TestBed.createComponent(EditSiteDialogComponent).componentInstance;
    component.onManagerSelected({ option: { value: { id: 'manager-2', displayName: 'Grace Hopper', email: null } } } as never);
    component.siteForm.patchValue({ status: 'Scheduled' });

    await component.saveSite();

    expect(sitesService.updateSite).toHaveBeenCalledWith({
      id: 'site-1', name: 'North Gate', address: '1420 Industrial Park', managerId: 'manager-2',
      startDate: '2026-03-03', endDate: null, status: 'Scheduled', mediaPolicyPreset: 'Regular'
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });
});
