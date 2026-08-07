import { TestBed } from '@angular/core/testing';

import { DataTableComponent } from './data-table.component';

describe('DataTableComponent', () => {
  it('marks only rows selected by the supplied error predicate', async () => {
    await TestBed.configureTestingModule({ imports: [DataTableComponent] }).compileComponents();
    const fixture = TestBed.createComponent(DataTableComponent<{ id: number; incomplete: boolean }>);
    fixture.componentRef.setInput(
      'errorRowPredicate',
      (row: { id: number; incomplete: boolean }) => row.incomplete
    );

    expect(fixture.componentInstance.isErrorRow({ id: 1, incomplete: true })).toBe(true);
    expect(fixture.componentInstance.isErrorRow({ id: 2, incomplete: false })).toBe(false);
  });

  it('does not mark rows when no predicate is supplied', async () => {
    await TestBed.configureTestingModule({ imports: [DataTableComponent] }).compileComponents();
    const fixture = TestBed.createComponent(DataTableComponent<{ id: number }>);

    expect(fixture.componentInstance.isErrorRow({ id: 1 })).toBe(false);
  });

  it('renders the error-row class only for rows selected by the predicate', async () => {
    await TestBed.configureTestingModule({ imports: [DataTableComponent] }).compileComponents();
    const fixture = TestBed.createComponent(DataTableComponent<{ id: number; incomplete: boolean }>);
    fixture.componentRef.setInput('columns', [{ key: 'id', label: 'Id' }]);
    fixture.componentRef.setInput('rows', [{ id: 1, incomplete: true }, { id: 2, incomplete: false }]);
    fixture.componentRef.setInput('filteredRowsTotal', 2);
    fixture.componentRef.setInput('overallRowsTotal', 2);
    fixture.componentRef.setInput(
      'errorRowPredicate',
      (row: { id: number; incomplete: boolean }) => row.incomplete
    );

    fixture.detectChanges();
    await fixture.whenStable();

    expect(fixture.nativeElement.querySelectorAll('.data-table__row--error')).toHaveLength(1);
  });
});
