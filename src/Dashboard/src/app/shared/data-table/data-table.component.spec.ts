import { TestBed } from '@angular/core/testing';

import { DataTableComponent } from './data-table.component';

describe('DataTableComponent', () => {
  it('marks only rows selected by the supplied error predicate', async () => {
    await TestBed.configureTestingModule({ imports: [DataTableComponent] }).compileComponents();
    const fixture = TestBed.createComponent(DataTableComponent<{ id: number; incomplete: boolean }>);
    fixture.componentRef.setInput('errorRowPredicate', (row) => row.incomplete);

    expect(fixture.componentInstance.isErrorRow({ id: 1, incomplete: true })).toBeTrue();
    expect(fixture.componentInstance.isErrorRow({ id: 2, incomplete: false })).toBeFalse();
  });

  it('does not mark rows when no predicate is supplied', async () => {
    await TestBed.configureTestingModule({ imports: [DataTableComponent] }).compileComponents();
    const fixture = TestBed.createComponent(DataTableComponent<{ id: number }>);

    expect(fixture.componentInstance.isErrorRow({ id: 1 })).toBeFalse();
  });
});
