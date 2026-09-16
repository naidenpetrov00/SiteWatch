import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { ActivityCatalogService } from '../../services/activity-catalog.service';
import {
  RequirementSectionDialogComponent,
  RequirementSectionDialogData
} from './requirement-section-dialog.component';

describe('RequirementSectionDialogComponent', () => {
  let data: RequirementSectionDialogData;
  const catalogService = {
    mutation: { isPending: () => false },
    createRequirementSection: vi.fn(), updateRequirementSection: vi.fn()
  };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    data = { activityId: 'activity-1' };
    await TestBed.configureTestingModule({
      imports: [RequirementSectionDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useFactory: () => data },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: ActivityCatalogService, useValue: catalogService }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('normalizes and submits a new section', async () => {
    catalogService.createRequirementSection.mockResolvedValue({ id: 'section-1' });
    const component = TestBed.createComponent(RequirementSectionDialogComponent).componentInstance;
    component.form.patchValue({ name: '  Exterior   works ', basisQuantity: 10, measurementUnit: 'm2' });

    await component.submit();

    expect(catalogService.createRequirementSection).toHaveBeenCalledWith('activity-1', {
      name: 'Exterior works', basisQuantity: 10, measurementUnit: 'm2'
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('rejects excessive decimal precision and keeps the dialog open after failure', async () => {
    const component = TestBed.createComponent(RequirementSectionDialogComponent).componentInstance;
    component.form.controls.basisQuantity.setValue(1.00001);
    await component.submit();
    expect(catalogService.createRequirementSection).not.toHaveBeenCalled();

    component.form.controls.basisQuantity.setValue(1);
    catalogService.createRequirementSection.mockRejectedValue(new Error('offline'));
    await component.submit();
    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(component.errorMessage()).toContain('could not be updated');
  });

  it('uses the selected section identifier for edits', async () => {
    data = { activityId: 'activity-1', section: { id: 'section-1', name: null, basisQuantity: 1, measurementUnit: 'piece', sortOrder: 0, productRequirements: [] } };
    catalogService.updateRequirementSection.mockResolvedValue(undefined);
    const component = TestBed.createComponent(RequirementSectionDialogComponent).componentInstance;

    await component.submit();

    expect(catalogService.updateRequirementSection).toHaveBeenCalledWith('activity-1', 'section-1', {
      name: null, basisQuantity: 1, measurementUnit: 'piece'
    });
  });
});
