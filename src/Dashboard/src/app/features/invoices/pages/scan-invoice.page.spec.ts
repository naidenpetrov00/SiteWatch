import { TestBed } from '@angular/core/testing';

import { ScanInvoicePage } from './scan-invoice.page';

describe('ScanInvoicePage', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ScanInvoicePage]
    }).compileComponents();
  });

  it('renders the scan invoice heading', () => {
    const fixture = TestBed.createComponent(ScanInvoicePage);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Scan Invoice');
  });
});
