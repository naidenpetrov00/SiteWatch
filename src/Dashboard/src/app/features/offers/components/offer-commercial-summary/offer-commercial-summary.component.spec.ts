import { TestBed } from '@angular/core/testing';
import { vi } from 'vitest';

import { OfferPricingMatrix } from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { OfferCommercialSummaryComponent } from './offer-commercial-summary.component';

describe('OfferCommercialSummaryComponent', () => {
  const offersService = {
    pricingMatrixQuery: {
      isPending: () => false,
      isError: () => false
    },
    updateDiscountsMutation: { isPending: () => false },
    updateDiscounts: vi.fn()
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [OfferCommercialSummaryComponent],
      providers: [{ provide: OffersService, useValue: offersService }]
    }).compileComponents();
    vi.clearAllMocks();
    offersService.updateDiscounts.mockResolvedValue(undefined);
  });

  it('validates and saves both category-wide discounts together', async () => {
    const fixture = createFixture(true);
    const component = fixture.componentInstance;
    component.discountForm.patchValue({
      activityDiscountPercentage: 12.5,
      productDiscountPercentage: 101
    });

    await component.save();
    expect(offersService.updateDiscounts).not.toHaveBeenCalled();

    component.discountForm.controls.productDiscountPercentage.setValue(20);
    await component.save();

    expect(offersService.updateDiscounts).toHaveBeenCalledWith({
      siteId: 'site-1',
      offerId: 'offer-1',
      activityDiscountPercentage: 12.5,
      productDiscountPercentage: 20
    });
  });

  it('shows all calculated totals and no editable controls after finalization', async () => {
    const fixture = createFixture(false);
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent;

    expect(text).toContain('Finalized pricing · read-only');
    expect(text).toContain('Activity subtotal before discount');
    expect(text).toContain('Activity discount amount');
    expect(text).toContain('Activity total after discount');
    expect(text).toContain('Product subtotal before discount');
    expect(text).toContain('Product discount amount');
    expect(text).toContain('Product total after discount');
    expect(text).toContain('Combined Offer total');
    expect(text).not.toContain('Save Discounts');
    expect(fixture.nativeElement.querySelector('input')).toBeNull();
  });

  function createFixture(editable: boolean) {
    const fixture = TestBed.createComponent(OfferCommercialSummaryComponent);
    fixture.componentRef.setInput('siteId', 'site-1');
    fixture.componentRef.setInput('offerId', 'offer-1');
    fixture.componentRef.setInput('matrix', matrix);
    fixture.componentRef.setInput('editable', editable);
    fixture.detectChanges();
    return fixture;
  }
});

const matrix: OfferPricingMatrix = {
  offerId: 'offer-1',
  status: 'Finalized',
  currencyCode: 'EUR',
  requiredPricingComplete: true,
  optionalPricingComplete: true,
  requiredTotal: 200,
  optionalTotal: 50,
  commercialTotals: {
    activityPricingComplete: true,
    productPricingComplete: true,
    activitySubtotalBeforeDiscount: 100,
    activityDiscountPercentage: 10,
    activityDiscountAmount: 10,
    activityTotalAfterDiscount: 90,
    productSubtotalBeforeDiscount: 250,
    productDiscountPercentage: 20,
    productDiscountAmount: 50,
    productTotalAfterDiscount: 200,
    combinedOfferTotal: 290
  },
  retailers: [],
  products: []
};
