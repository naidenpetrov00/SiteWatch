import { TestBed } from '@angular/core/testing';
import { MatChipInputEvent } from '@angular/material/chips';
import { MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { SiteMediaPolicyPresetDefinition } from '../../models/site-media-policy-presets';
import { DashboardSitesService } from '../../services/dashboard-sites.service';
import { AddSiteDialogComponent } from './add-site-dialog.component';

const presets: readonly SiteMediaPolicyPresetDefinition[] = [
  {
    preset: 'ApartmentRenovation',
    displayName: 'Apartment Renovation',
    categories: ['Design', 'Demolition', 'Electricity', 'Pipes', 'Finishes', 'Other']
  },
  {
    preset: 'HouseBuild',
    displayName: 'House Build',
    categories: ['Design', 'Foundation', 'Structure', 'Roof', 'Electricity', 'Pipes', 'Exterior', 'Other']
  },
  { preset: 'Custom', displayName: 'Custom', categories: ['Other'] }
];

describe('AddSiteDialogComponent', () => {
  const sitesService = {
    createSiteMutation: { isPending: () => false },
    getMediaPolicyPresets: vi.fn(),
    createSite: vi.fn()
  };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    vi.clearAllMocks();
    sitesService.getMediaPolicyPresets.mockResolvedValue(presets);
    await TestBed.configureTestingModule({
      imports: [AddSiteDialogComponent],
      providers: [
        { provide: DashboardSitesService, useValue: sitesService },
        { provide: MatDialogRef, useValue: dialogRef }
      ]
    }).compileComponents();
  });

  it('loads the first preset and submits normalized custom categories', async () => {
    sitesService.createSite.mockResolvedValue({ id: 'site-42' });
    const fixture = TestBed.createComponent(AddSiteDialogComponent);
    await fixture.whenStable();
    const component = fixture.componentInstance;

    expect(component.siteForm.controls.mediaPolicyPreset.value).toBe('ApartmentRenovation');
    expect(component.mediaCategories()).toEqual(presets[0].categories);

    component.addCategory(chipEvent('  HVAC   Controls '));
    component.siteForm.patchValue({ name: 'Apartment 42', address: '42 Main Street' });
    await component.submitSite();

    expect(component.siteForm.controls.mediaPolicyPreset.value).toBe('Custom');
    expect(component.mediaCategories()).toEqual([
      'Design', 'Demolition', 'Electricity', 'Pipes', 'Finishes', 'HVAC Controls', 'Other'
    ]);
    expect(sitesService.createSite).toHaveBeenCalledWith({
      name: 'Apartment 42',
      address: '42 Main Street',
      mediaPolicyPreset: 'Custom',
      mediaCategories: [
        'Design', 'Demolition', 'Electricity', 'Pipes', 'Finishes', 'HVAC Controls', 'Other'
      ]
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('selecting a preset replaces custom categories with its definition', async () => {
    const fixture = TestBed.createComponent(AddSiteDialogComponent);
    await fixture.whenStable();

    fixture.componentInstance.addCategory(chipEvent('Access Control'));
    fixture.componentInstance.selectPreset('HouseBuild');

    expect(fixture.componentInstance.siteForm.controls.mediaPolicyPreset.value).toBe('HouseBuild');
    expect(fixture.componentInstance.mediaCategories()).toEqual(presets[1].categories);
  });

  it.each([
    ['All', 'All is reserved for the filter that shows every item.'],
    ['x'.repeat(51), 'Categories cannot exceed 50 characters.']
  ])('rejects invalid category %s', async (category, message) => {
    const fixture = TestBed.createComponent(AddSiteDialogComponent);
    await fixture.whenStable();

    fixture.componentInstance.addCategory(chipEvent(category));

    expect(fixture.componentInstance.categoryError()).toBe(message);
    expect(fixture.componentInstance.mediaCategories()).toEqual(presets[0].categories);
  });

  it('ignores duplicates and rejects a category beyond the policy limit', async () => {
    const fixture = TestBed.createComponent(AddSiteDialogComponent);
    await fixture.whenStable();
    const component = fixture.componentInstance;

    component.addCategory(chipEvent(' design '));
    expect(component.categoryError()).toBeNull();
    expect(component.mediaCategories()).toEqual(presets[0].categories);

    component.mediaCategories.set([
      ...Array.from({ length: 19 }, (_, index) => `Category ${index + 1}`),
      'Other'
    ]);
    component.addCategory(chipEvent('One Too Many'));

    expect(component.categoryError()).toBe('A policy can contain up to 20 categories.');
    expect(component.mediaCategories()).toHaveLength(20);
  });

  it('shows preset loading failures and disables submission', async () => {
    sitesService.getMediaPolicyPresets.mockRejectedValue(new Error('offline'));
    const fixture = TestBed.createComponent(AddSiteDialogComponent);
    await fixture.whenStable();
    const compiled = fixture.nativeElement as HTMLElement;
    const submit = Array.from(compiled.querySelectorAll('button'))
      .find((button) => button.textContent?.trim() === 'Submit') as HTMLButtonElement;

    expect(compiled.querySelector('[role="alert"]')?.textContent).toContain(
      'Media policies could not be loaded'
    );
    expect(submit.disabled).toBe(true);
    submit.click();
    await fixture.whenStable();
    expect(sitesService.createSite).not.toHaveBeenCalled();
  });
});

function chipEvent(value: string): MatChipInputEvent {
  return {
    value,
    chipInput: { clear: vi.fn() }
  } as unknown as MatChipInputEvent;
}
