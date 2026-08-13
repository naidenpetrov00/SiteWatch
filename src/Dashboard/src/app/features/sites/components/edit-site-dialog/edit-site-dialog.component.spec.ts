import { TestBed } from '@angular/core/testing';
import { MatChipInputEvent } from '@angular/material/chips';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardUsersService } from '../../../users/services/dashboard-users.service';
import { DashboardSite } from '../../models/dashboard-site.model';
import { DashboardSitesService } from '../../services/dashboard-sites.service';
import { EditSiteDialogComponent } from './edit-site-dialog.component';

const site: DashboardSite = { id: 'site-42', numberId: 42, name: 'House 42', address: '42 Main Street', managerId: 'manager-1', managerDisplayName: 'Ada Lovelace', startDate: '2026-03-03', endDate: null, status: 'Planning', mediaPolicy: { preset: 'HouseBuild', categories: ['Design', 'Foundation', 'Structure', 'Other'] } };

describe('EditSiteDialogComponent', () => {
  const sitesService = { updateSiteMutation: { isPending: () => false }, updateSite: vi.fn() };
  const usersService = { searchUsers: vi.fn() };
  const dialogRef = { close: vi.fn() };
  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({ imports: [EditSiteDialogComponent], providers: [{ provide: MAT_DIALOG_DATA, useValue: site }, { provide: DashboardSitesService, useValue: sitesService }, { provide: DashboardUsersService, useValue: usersService }, { provide: MatDialogRef, useValue: dialogRef }] }).compileComponents();
  });
  it('keeps saved categories immutable and submits new additions with site metadata', async () => {
    sitesService.updateSite.mockResolvedValue(undefined);
    const component = TestBed.createComponent(EditSiteDialogComponent).componentInstance;
    component.onManagerSelected({ option: { value: { id: 'manager-2', displayName: 'Grace Hopper', email: null } } } as never);
    component.addCategory(chipEvent(' foundation '));
    component.addCategory(chipEvent(' HVAC Controls '));
    component.siteForm.patchValue({ status: 'Scheduled' });
    await component.saveSite();
    expect(component.savedCategories).toEqual(site.mediaPolicy.categories);
    expect(sitesService.updateSite).toHaveBeenCalledWith({ id: 'site-42', name: 'House 42', address: '42 Main Street', managerId: 'manager-2', startDate: '2026-03-03', endDate: null, status: 'Scheduled', mediaPolicyPreset: 'HouseBuild', mediaCategoriesToAdd: ['HVAC Controls'] });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });
  it('renders the formatted policy and no remove control for saved categories', async () => {
    const fixture = TestBed.createComponent(EditSiteDialogComponent); await fixture.whenStable();
    expect((fixture.nativeElement as HTMLElement).textContent).toContain('House Build');
    expect((fixture.nativeElement as HTMLElement).querySelector('button[aria-label="Remove Design"]')).toBeNull();
  });
});

function chipEvent(value: string): MatChipInputEvent { return { value, chipInput: { clear: vi.fn() } } as unknown as MatChipInputEvent; }
