import { FormBuilder } from '@angular/forms';
import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { TerritorialEntry, TerritorialEntryKind } from '../../../shared/territorial-structure/territorial-structure.models';
import { TerritorialStructureService } from '../../../shared/territorial-structure/territorial-structure.service';
import { ManageTerritorialStructurePage } from './manage-territorial-structure.page';

describe('ManageTerritorialStructurePage', () => {
  const service = {
    getEntries: vi.fn<(
      kind: TerritorialEntryKind,
      parentId?: string | null,
      searchTerm?: string,
      includeInactive?: boolean
    ) => Promise<readonly TerritorialEntry[]>>(),
    create: vi.fn<(request: { kind: TerritorialEntryKind; parentId: string | null; name: string }) => Promise<void>>(),
    update: vi.fn<(id: string, request: { kind: TerritorialEntryKind; name: string; isActive: boolean }) => Promise<void>>(),
    delete: vi.fn<(id: string, kind: TerritorialEntryKind) => Promise<void>>()
  };
  const dialog = { open: vi.fn(() => ({ afterClosed: () => of(true) })) };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        FormBuilder,
        { provide: TerritorialStructureService, useValue: service },
        { provide: MatDialog, useValue: dialog }
      ]
    });
    service.getEntries.mockReset();
    service.create.mockReset().mockResolvedValue(undefined);
    service.update.mockReset().mockResolvedValue(undefined);
    service.delete.mockReset().mockResolvedValue(undefined);
    dialog.open.mockReset().mockReturnValue({ afterClosed: () => of(true) });
  });

  it('loads each hierarchy branch lazily and caches loaded children', async () => {
    service.getEntries.mockImplementation(async (kind, parentId) => {
      if (kind === 0) return [entry('municipality-1', 'Stolichna', null)];
      if (kind === 1) return [entry('settlement-1', 'Sofia', 'municipality-1')];
      if (kind === 2) return [entry('district-1', 'Oborishte', 'settlement-1')];
      if (kind === 3) return [entry('quarter-1', 'Geo Milev', 'settlement-1')];
      return [];
    });
    const component = createPage();

    await component.ngOnInit();

    expect(service.getEntries).toHaveBeenCalledTimes(1);
    expect(service.getEntries).toHaveBeenCalledWith(0, null, '', true);
    let municipality = findNode(component.treeNodes(), 'municipality-1');
    expect(municipality).toBeDefined();

    component.expansionChanged(municipality!, true);
    await settlePromises();
    municipality = findNode(component.treeNodes(), 'municipality-1');
    expect(municipality?.children.map((node) => node.id)).toEqual(['settlement-1']);

    component.expansionChanged(municipality!, true);
    await settlePromises();
    expect(service.getEntries.mock.calls.filter(([kind]) => kind === 1)).toHaveLength(1);

    const settlement = findNode(component.treeNodes(), 'settlement-1')!;
    component.expansionChanged(settlement, true);
    await settlePromises();

    expect(findNode(component.treeNodes(), 'settlement-1')?.children.map((node) => node.id))
      .toEqual(['district-1', 'quarter-1']);
    expect(service.getEntries).toHaveBeenCalledWith(2, 'settlement-1', '', true);
    expect(service.getEntries).toHaveBeenCalledWith(3, 'settlement-1', '', true);
  });

  it('ignores an older branch response when a newer refresh finishes first', async () => {
    service.getEntries.mockResolvedValueOnce([entry('municipality-1', 'Stolichna', null)]);
    const component = createPage();
    await component.ngOnInit();
    const municipality = findNode(component.treeNodes(), 'municipality-1')!;
    const first = deferred<readonly TerritorialEntry[]>();
    const second = deferred<readonly TerritorialEntry[]>();
    service.getEntries
      .mockImplementationOnce(() => first.promise)
      .mockImplementationOnce(() => second.promise);

    component.expansionChanged(municipality, true);
    component.expansionChanged(municipality, true);
    second.resolve([entry('settlement-new', 'New Sofia', 'municipality-1')]);
    await settlePromises();
    first.resolve([entry('settlement-old', 'Old Sofia', 'municipality-1')]);
    await settlePromises();

    expect(findNode(component.treeNodes(), 'municipality-1')?.children.map((node) => node.id))
      .toEqual(['settlement-new']);
  });

  it('creates by refreshing the parent and updates renamed and active state locally', async () => {
    const municipality = entry('municipality-1', 'Stolichna', null);
    const secondMunicipality = entry('municipality-2', 'Plovdiv', null);
    service.getEntries
      .mockResolvedValueOnce([municipality])
      .mockResolvedValueOnce([municipality, secondMunicipality]);
    const component = createPage();
    await component.ngOnInit();

    component.openCreate(0, null);
    component.entryForm.setValue({ name: 'Plovdiv' });
    await component.save();

    expect(service.create).toHaveBeenCalledWith({ kind: 0, parentId: null, name: 'Plovdiv' });
    expect(findNode(component.treeNodes(), 'municipality-2')?.name).toBe('Plovdiv');

    let node = findNode(component.treeNodes(), 'municipality-2')!;
    component.openRename(node);
    component.entryForm.setValue({ name: 'Plovdiv Municipality' });
    await component.save();

    expect(service.update).toHaveBeenCalledWith('municipality-2', {
      kind: 0,
      name: 'Plovdiv Municipality',
      isActive: true
    });
    expect(findNode(component.treeNodes(), 'municipality-2')?.name).toBe('Plovdiv Municipality');

    node = findNode(component.treeNodes(), 'municipality-2')!;
    await component.toggleActive(node);

    expect(service.update).toHaveBeenLastCalledWith('municipality-2', {
      kind: 0,
      name: 'Plovdiv Municipality',
      isActive: false
    });
    expect(findNode(component.treeNodes(), 'municipality-2')?.isActive).toBe(false);
  });

  it('deletes only after confirmation and selects the removed node parent', async () => {
    service.getEntries.mockResolvedValue([entry('municipality-1', 'Stolichna', null)]);
    const component = createPage();
    await component.ngOnInit();
    const municipality = findNode(component.treeNodes(), 'municipality-1')!;
    dialog.open.mockReturnValueOnce({ afterClosed: () => of(false) });

    await component.deleteNode(municipality);

    expect(service.delete).not.toHaveBeenCalled();
    expect(findNode(component.treeNodes(), 'municipality-1')).toBeDefined();

    dialog.open.mockReturnValueOnce({ afterClosed: () => of(true) });
    await component.deleteNode(municipality);

    expect(service.delete).toHaveBeenCalledWith('municipality-1', 0);
    expect(findNode(component.treeNodes(), 'municipality-1')).toBeUndefined();
    expect(component.selectedNodeId()).toBe('bulgaria');
  });

  it('shows API problem details and keeps the editor state for correction', async () => {
    service.getEntries.mockResolvedValue([]);
    service.create.mockRejectedValue({ error: { detail: 'A Municipality named Stolichna already exists.' } });
    const component = createPage();
    await component.ngOnInit();
    component.openCreate(0, null);
    component.entryForm.setValue({ name: 'Stolichna' });

    await component.save();

    expect(component.pageError()).toBe('A Municipality named Stolichna already exists.');
    expect(component.editor()?.mode).toBe('create');
  });

  function createPage(): ManageTerritorialStructurePage {
    return TestBed.runInInjectionContext(() => new ManageTerritorialStructurePage());
  }
});

type TreeNode = ReturnType<ManageTerritorialStructurePage['treeNodes']>[number];

function findNode(nodes: readonly TreeNode[], id: string): TreeNode | undefined {
  for (const node of nodes) {
    if (node.id === id) return node;
    const child = findNode(node.children, id);
    if (child) return child;
  }
  return undefined;
}

function entry(
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
  await Promise.resolve();
}
