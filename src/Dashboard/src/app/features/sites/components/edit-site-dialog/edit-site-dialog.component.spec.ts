import { TestBed } from '@angular/core/testing';
import { MatChipInputEvent } from '@angular/material/chips';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardSite } from '../../models/dashboard-site.model';
import { DashboardSitesService } from '../../services/dashboard-sites.service';
import { EditSiteDialogComponent } from './edit-site-dialog.component';

const site: DashboardSite = {
  id: 'site-42',
  numberId: 42,
  name: 'House 42',
  address: '42 Main Street',
  mediaPolicy: {
    preset: 'HouseBuild',
    categories: ['Design', 'Foundation', 'Structure', 'Other']
  }
};

describe('EditSiteDialogComponent', () => {
  const sitesService = {
    updateSiteMutation: { isPending: () => false },
    updateSite: vi.fn()
  };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [EditSiteDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useValue: site },
        { provide: DashboardSitesService, useValue: sitesService },
        { provide: MatDialogRef, useValue: dialogRef }
      ]
    }).compileComponents();
  });

  it('keeps saved categories immutable and submits only new additions', async () => {
    sitesService.updateSite.mockResolvedValue(undefined);
    const fixture = TestBed.createComponent(EditSiteDialogComponent);
    const component = fixture.componentInstance;

    component.addCategory(chipEvent(' foundation '));
    expect(component.newMediaCategories()).toEqual([]);
    expect(component.savedCategories).toEqual(site.mediaPolicy.categories);

    component.addCategory(chipEvent('  HVAC   Controls '));
    component.addCategory(chipEvent('Access Control'));
    component.removeNewCategory('Access Control');
    await component.saveSite();

    expect(component.savedCategories).toEqual(site.mediaPolicy.categories);
    expect(sitesService.updateSite).toHaveBeenCalledWith({
      id: 'site-42',
      name: 'House 42',
      address: '42 Main Street',
      mediaCategoriesToAdd: ['HVAC Controls']
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('renders the formatted policy and no remove control for saved categories', async () => {
    const fixture = TestBed.createComponent(EditSiteDialogComponent);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;

    expect(compiled.textContent).toContain('House Build');
    expect(compiled.querySelector('button[aria-label="Remove Design"]')).toBeNull();
    expect(compiled.textContent).toContain('Other');
  });
});

function chipEvent(value: string): MatChipInputEvent {
  return {
    value,
    chipInput: { clear: vi.fn() }
  } as unknown as MatChipInputEvent;
}
