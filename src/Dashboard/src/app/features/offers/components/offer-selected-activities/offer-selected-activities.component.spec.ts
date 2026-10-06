import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { OfferActivity } from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { OfferSelectedActivitiesComponent } from './offer-selected-activities.component';

describe('OfferSelectedActivitiesComponent commercial pricing', () => {
  const offersService = {
    updateMeasurementsMutation: { isPending: () => false },
    updateActivityPricingMutation: { isPending: () => false },
    removeActivityMutation: { isPending: () => false },
    updateActivityMeasurements: vi.fn(),
    updateActivitySectionPricing: vi.fn(),
    removeActivity: vi.fn()
  };
  const dialog = { open: vi.fn(() => ({ afterClosed: () => of(false) })) };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OfferSelectedActivitiesComponent],
      providers: [{ provide: OffersService, useValue: offersService }]
    }).overrideProvider(MatDialog, { useValue: dialog }).compileComponents();
    vi.clearAllMocks();
    offersService.updateActivitySectionPricing.mockResolvedValue(undefined);
  });

  it('saves exactly one pricing entry for every snapshotted section', async () => {
    const fixture = TestBed.createComponent(OfferSelectedActivitiesComponent);
    fixture.componentRef.setInput('siteId', 'site-1');
    fixture.componentRef.setInput('offerId', 'offer-1');
    fixture.componentRef.setInput('activities', [activity]);
    fixture.componentRef.setInput('editable', true);
    fixture.detectChanges();
    await fixture.whenStable();

    const component = fixture.componentInstance;
    const pricing = component.formFor(activity.id).controls.pricing;
    pricing.controls[0].controls.pricingMode.setValue('per-measurement');
    component.onPricingModeChange(activity.id, 0);
    pricing.controls[0].controls.priceAmount.setValue(12.5);
    pricing.controls[1].controls.pricingMode.setValue('free');
    component.onPricingModeChange(activity.id, 1);

    await component.savePricing(activity);

    expect(offersService.updateActivitySectionPricing).toHaveBeenCalledWith({
      siteId: 'site-1',
      offerId: 'offer-1',
      offerActivityId: 'activity-1',
      sectionPricing: [
        {
          sectionId: 'section-1',
          pricingMode: 'per-measurement',
          priceAmount: 12.5
        },
        { sectionId: 'section-2', pricingMode: 'free', priceAmount: null }
      ]
    });
  });

  it('renders finalized activity pricing as read-only', async () => {
    const fixture = TestBed.createComponent(OfferSelectedActivitiesComponent);
    fixture.componentRef.setInput('siteId', 'site-1');
    fixture.componentRef.setInput('offerId', 'offer-1');
    fixture.componentRef.setInput('activities', [activity]);
    fixture.componentRef.setInput('editable', false);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    const text = fixture.nativeElement.textContent;
    expect(text).toContain('Saved snapshot · read-only');
    expect(text).toContain('Fixed section price');
    expect(text).toContain('Included / free');
    expect(text).not.toContain('Save Activity Pricing');
    expect(fixture.nativeElement.querySelector('mat-select')).toBeNull();
  });
});

const activity: OfferActivity = {
  id: 'activity-1',
  sourceActivityId: 'source-activity-1',
  activityNumberId: 7,
  name: 'Installation',
  description: null,
  sortOrder: 0,
  sections: [
    {
      id: 'section-1',
      sourceSectionId: 'source-section-1',
      name: 'Wall area',
      basisQuantity: 1,
      measurementUnit: 'm2',
      requestedMeasurement: 10,
      pricingMode: 'fixed',
      priceAmount: 100,
      priceTotal: 100,
      sortOrder: 0
    },
    {
      id: 'section-2',
      sourceSectionId: 'source-section-2',
      name: 'Cleanup',
      basisQuantity: 1,
      measurementUnit: 'piece',
      requestedMeasurement: 1,
      pricingMode: 'free',
      priceAmount: null,
      priceTotal: 0,
      sortOrder: 1
    }
  ]
};
