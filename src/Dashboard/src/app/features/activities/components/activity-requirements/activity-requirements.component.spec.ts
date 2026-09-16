import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { ActivityDetails } from '../../models/activity-catalog.models';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { ActivityRequirementsComponent } from './activity-requirements.component';

describe('ActivityRequirementsComponent', () => {
  const catalogService = {
    getActivity: vi.fn(),
    moveRequirementSection: vi.fn(), moveProductRequirement: vi.fn(),
    deleteRequirementSection: vi.fn(), deleteProductRequirement: vi.fn()
  };
  const dialog = { open: vi.fn(() => ({ afterClosed: () => of(false) })) };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActivityRequirementsComponent],
      providers: [
        { provide: ActivityCatalogService, useValue: catalogService },
        provideTanStackQuery(new QueryClient())
      ]
    }).overrideProvider(MatDialog, { useValue: dialog }).compileComponents();
    vi.clearAllMocks();
    dialog.open.mockReturnValue({ afterClosed: () => of(false) });
  });

  it('moves sections and products only within their available bounds', async () => {
    catalogService.moveRequirementSection.mockResolvedValue(undefined);
    catalogService.moveProductRequirement.mockResolvedValue(undefined);
    const component = TestBed.createComponent(ActivityRequirementsComponent).componentInstance;

    await component.moveSection(activity, activity.requirementSections[0], -1);
    await component.moveSection(activity, activity.requirementSections[0], 1);
    await component.moveProduct(activity, activity.requirementSections[0], activity.requirementSections[0].productRequirements[0], -1);
    await component.moveProduct(activity, activity.requirementSections[0], activity.requirementSections[0].productRequirements[0], 1);

    expect(catalogService.moveRequirementSection).toHaveBeenCalledWith('activity-1', 'section-1', { targetIndex: 1 });
    expect(catalogService.moveProductRequirement).toHaveBeenCalledWith('activity-1', 'section-1', 'requirement-1', { targetIndex: 1 });
    expect(catalogService.moveRequirementSection).toHaveBeenCalledTimes(1);
    expect(catalogService.moveProductRequirement).toHaveBeenCalledTimes(1);
  });

  it('does not delete on cancellation and deletes only after confirmation', async () => {
    catalogService.deleteRequirementSection.mockResolvedValue(undefined);
    const component = TestBed.createComponent(ActivityRequirementsComponent).componentInstance;

    await component.deleteSection(activity, activity.requirementSections[0]);
    expect(catalogService.deleteRequirementSection).not.toHaveBeenCalled();

    dialog.open.mockReturnValue({ afterClosed: () => of(true) });
    await component.deleteSection(activity, activity.requirementSections[0]);
    expect(catalogService.deleteRequirementSection).toHaveBeenCalledWith('activity-1', 'section-1');
  });

  it('renders archived requirements as read-only with representative labels', async () => {
    catalogService.getActivity.mockResolvedValue({ ...activity, status: 'Archived' });
    const fixture = TestBed.createComponent(ActivityRequirementsComponent);
    fixture.componentRef.setInput('selectedActivityId', 'activity-1');
    fixture.detectChanges();
    await vi.waitFor(() => {
      fixture.detectChanges();
      const text = fixture.nativeElement.textContent;
      expect(text).toContain('Requirements are read-only');
      expect(text).toContain('For 10 m²');
      expect(text).toContain('#12 · Anchor');
      expect(text).not.toContain('Add Section');
    });
  });
});

const activity: ActivityDetails = {
  id: 'activity-1', numberId: 1, name: 'Inspection', description: null,
  status: 'Active', parentFolderId: null, sortOrder: 0,
  requirementSections: [
    {
      id: 'section-1', name: 'Exterior', basisQuantity: 10, measurementUnit: 'm2', sortOrder: 0,
      productRequirements: [
        { id: 'requirement-1', productId: 'product-1', productNumberId: 12, productTitle: 'Anchor', productStatus: 'Active', brand: null, model: null, packageQuantity: null, packageUnit: null, quantity: 2, isRequired: true, quantityBehavior: 'proportional', notes: null, sortOrder: 0 },
        { id: 'requirement-2', productId: 'product-2', productNumberId: 13, productTitle: 'Bolt', productStatus: 'Active', brand: null, model: null, packageQuantity: null, packageUnit: null, quantity: 1, isRequired: false, quantityBehavior: 'fixed', notes: null, sortOrder: 1 }
      ]
    },
    { id: 'section-2', name: null, basisQuantity: 1, measurementUnit: 'piece', sortOrder: 1, productRequirements: [] }
  ]
};
