import { TestBed } from '@angular/core/testing';

import { InvoicesPage } from './invoices.page';

describe('InvoicesPage', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [InvoicesPage]
    }).compileComponents();
  });

  it('renders the invoices heading', () => {
    const fixture = TestBed.createComponent(InvoicesPage);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('h1')?.textContent).toContain('Invoices');
  });
});
