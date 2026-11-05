import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { provideRouter } from '@angular/router';
import { vi } from 'vitest';

import { TerritorialEntry, TerritorialEntryKind } from './territorial-structure.models';
import { TerritorialSelectorComponent } from './territorial-selector.component';
import { TerritorialStructureService } from './territorial-structure.service';

describe('TerritorialSelectorComponent', () => {
  const service = {
    getEntries: vi.fn<(
      kind: TerritorialEntryKind,
      parentId?: string | null,
      searchTerm?: string,
      includeInactive?: boolean
    ) => Promise<readonly TerritorialEntry[]>>()
  };
  let fixture: ComponentFixture<TerritorialSelectorComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TerritorialSelectorComponent],
      providers: [
        provideRouter([]),
        { provide: TerritorialStructureService, useValue: service }
      ]
    }).compileComponents();
    service.getEntries.mockReset();
  });

  afterEach(() => {
    fixture?.destroy();
    vi.useRealTimers();
  });

  it('writes the selected entry id and marks the control touched', async () => {
    fixture = await createComponent(1, 'municipality-1');
    const onChange = vi.fn();
    const onTouched = vi.fn();
    const entry = territorialEntry('settlement-1', 'Sofia', 'municipality-1');
    fixture.componentInstance.registerOnChange(onChange);
    fixture.componentInstance.registerOnTouched(onTouched);

    fixture.componentInstance.select(
      { option: { value: entry } } as unknown as MatAutocompleteSelectedEvent);

    expect(fixture.componentInstance.value).toBe('settlement-1');
    expect(fixture.componentInstance.inputControl.value).toEqual(entry);
    expect(onChange).toHaveBeenCalledWith('settlement-1');
    expect(onTouched).toHaveBeenCalledOnce();
  });

  it('clears the selected id when the user types or the parent changes', async () => {
    fixture = await createComponent(1, 'municipality-1');
    const onChange = vi.fn();
    fixture.componentInstance.registerOnChange(onChange);
    fixture.componentInstance.value = 'settlement-1';

    fixture.componentInstance.handleInput('new value');

    expect(fixture.componentInstance.value).toBe('');
    expect(onChange).toHaveBeenLastCalledWith('');

    fixture.componentInstance.value = 'settlement-2';
    fixture.componentInstance.inputControl.setValue(
      territorialEntry('settlement-2', 'Bankya', 'municipality-1'));
    fixture.componentRef.setInput('parentId', 'municipality-2');
    await fixture.whenStable();

    expect(fixture.componentInstance.value).toBe('');
    expect(fixture.componentInstance.inputControl.value).toBe('');
    expect(fixture.componentInstance.results()).toEqual([]);
    expect(onChange).toHaveBeenLastCalledWith('');
  });

  it('debounces search and lets only the newest request update the options', async () => {
    fixture = await createComponent(0, null);
    vi.useFakeTimers();
    const first = deferred<readonly TerritorialEntry[]>();
    const second = deferred<readonly TerritorialEntry[]>();
    service.getEntries.mockImplementation((_kind, _parentId, term) =>
      term === 'so' ? first.promise : second.promise);

    fixture.componentInstance.search('so');
    await vi.advanceTimersByTimeAsync(199);
    expect(service.getEntries).not.toHaveBeenCalled();
    await vi.advanceTimersByTimeAsync(1);
    expect(service.getEntries).toHaveBeenCalledWith(0, null, 'so', true);

    fixture.componentInstance.search('sof');
    await vi.advanceTimersByTimeAsync(200);
    second.resolve([territorialEntry('municipality-2', 'Sofia Municipality', null)]);
    await settlePromises();

    expect(fixture.componentInstance.results().map((entry) => entry.id)).toEqual(['municipality-2']);

    first.resolve([territorialEntry('municipality-1', 'Old result', null)]);
    await settlePromises();

    expect(fixture.componentInstance.results().map((entry) => entry.id)).toEqual(['municipality-2']);
    expect(fixture.componentInstance.loading()).toBe(false);
  });

  it('hides inactive entries except for the currently selected value', async () => {
    fixture = await createComponent(0, null);
    vi.useFakeTimers();
    service.getEntries.mockResolvedValue([
      territorialEntry('active', 'Active municipality', null),
      territorialEntry('selected-inactive', 'Selected inactive', null, false),
      territorialEntry('other-inactive', 'Other inactive', null, false)
    ]);

    fixture.componentInstance.writeValue('selected-inactive');
    await vi.advanceTimersByTimeAsync(200);
    await settlePromises();

    expect(fixture.componentInstance.results().map((entry) => entry.id)).toEqual([
      'active',
      'selected-inactive'
    ]);
    expect(fixture.componentInstance.inputControl.value).toEqual(
      territorialEntry('selected-inactive', 'Selected inactive', null, false));
  });

  it('recovers from failed searches and renders a secure management link', async () => {
    fixture = await createComponent(0, null);
    vi.useFakeTimers();
    service.getEntries.mockRejectedValue(new Error('offline'));

    fixture.componentInstance.search('sof');
    await vi.advanceTimersByTimeAsync(200);
    await settlePromises();

    expect(fixture.componentInstance.results()).toEqual([]);
    expect(fixture.componentInstance.loading()).toBe(false);
    const link = fixture.nativeElement.querySelector(
      'a[aria-label="Manage territorial structure in a new tab"]') as HTMLAnchorElement;
    expect(link.getAttribute('href')).toBe('/territorial-structure');
    expect(link.target).toBe('_blank');
    expect(link.rel).toBe('noopener noreferrer');
  });

  async function createComponent(
    kind: TerritorialEntryKind,
    parentId: string | null
  ): Promise<ComponentFixture<TerritorialSelectorComponent>> {
    const created = TestBed.createComponent(TerritorialSelectorComponent);
    created.componentRef.setInput('kind', kind);
    created.componentRef.setInput('parentId', parentId);
    created.componentRef.setInput('label', 'Territorial entry');
    await created.whenStable();
    return created;
  }
});

function territorialEntry(
  id: string,
  name: string,
  parentId: string | null,
  isActive = true
): TerritorialEntry {
  return { id, name, parentId, isActive };
}

function deferred<T>(): {
  promise: Promise<T>;
  resolve: (value: T) => void;
} {
  let resolve!: (value: T) => void;
  const promise = new Promise<T>((complete) => { resolve = complete; });
  return { promise, resolve };
}

async function settlePromises(): Promise<void> {
  await Promise.resolve();
  await Promise.resolve();
}
