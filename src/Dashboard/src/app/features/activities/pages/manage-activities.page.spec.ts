import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { QueryClient, provideTanStackQuery } from '@tanstack/angular-query-experimental';
import { of } from 'rxjs';
import { vi } from 'vitest';

import { ActivityCatalogNode } from '../models/activity-catalog.models';
import { ActivityCatalogService } from '../services/activity-catalog.service';
import { ManageActivitiesPage } from './manage-activities.page';

describe('ManageActivitiesPage', () => {
  const catalogService = {
    catalogQuery: { data: () => undefined, isPending: () => false, isError: () => false },
    mutation: { isPending: () => false },
    getActivity: vi.fn(), moveFolder: vi.fn(), moveActivity: vi.fn(),
    archiveActivity: vi.fn(), restoreActivity: vi.fn(), deleteFolder: vi.fn(), deleteActivity: vi.fn()
  };
  const dialog = { open: vi.fn(() => ({ afterClosed: () => of(true) })) };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageActivitiesPage],
      providers: [
        { provide: ActivityCatalogService, useValue: catalogService },
        provideTanStackQuery(new QueryClient())
      ]
    }).overrideProvider(MatDialog, { useValue: dialog }).compileComponents();
    vi.clearAllMocks();
    dialog.open.mockReturnValue({ afterClosed: () => of(true) });
  });

  it('builds a sorted tree and breadcrumbs from the flat catalog', () => {
    const component = TestBed.createComponent(ManageActivitiesPage).componentInstance;
    component.nodes.set(catalog);
    component.selectNode(catalog[2]);

    expect(component.treeNodes().map((node) => node.id)).toEqual(['activity-root', 'folder']);
    expect(component.treeNodes()[1].children.map((node) => node.id)).toEqual([
      'activity-child', 'activity-child-last'
    ]);
    expect(component.breadcrumbs()).toEqual([
      { id: null, label: 'Root' },
      { id: 'folder', label: 'Safety' },
      { id: 'activity-child', label: 'Inspection' }
    ]);
  });

  it('routes quick movement to the node kind and respects sibling bounds', async () => {
    catalogService.moveActivity.mockResolvedValue(undefined);
    const component = TestBed.createComponent(ManageActivitiesPage).componentInstance;
    component.nodes.set(catalog);

    await component.moveQuickly(catalog[0], -1);
    await component.moveQuickly(catalog[2], 1);

    expect(catalogService.moveActivity).toHaveBeenCalledWith('activity-child', {
      targetParentFolderId: 'folder', targetIndex: 1
    });
    expect(catalogService.moveFolder).not.toHaveBeenCalled();
    expect(component.canMoveUp(catalog[0])).toBe(false);
    expect(component.canMoveDown(catalog[0])).toBe(true);
  });

  it('opens activity details and surfaces failed detail reads', async () => {
    catalogService.getActivity.mockResolvedValue({ id: 'activity-child', numberId: 2, name: 'Inspection', description: null, status: 'Active', parentFolderId: 'folder', sortOrder: 0 });
    const component = TestBed.createComponent(ManageActivitiesPage).componentInstance;

    await component.openActivity(catalog[2], 'view');
    expect(dialog.open).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({
      data: expect.objectContaining({ kind: 'activity', mode: 'view', id: 'activity-child' })
    }));

    catalogService.getActivity.mockRejectedValue(new Error('offline'));
    await component.openActivity(catalog[2], 'edit');
    expect(component.pageError()).toBe('The activity details could not be loaded.');
  });

  it('confirms archive and deletion, then updates the selected location', async () => {
    catalogService.archiveActivity.mockResolvedValue(undefined);
    catalogService.deleteActivity.mockResolvedValue(undefined);
    const component = TestBed.createComponent(ManageActivitiesPage).componentInstance;
    component.nodes.set(catalog);
    component.selectNode(catalog[2]);

    await component.toggleArchive(catalog[2]);
    await component.deleteNode(catalog[2]);

    expect(catalogService.archiveActivity).toHaveBeenCalledWith('activity-child');
    expect(catalogService.deleteActivity).toHaveBeenCalledWith('activity-child');
    expect(component.selectedNodeId()).toBe('folder');
  });
});

const catalog: readonly ActivityCatalogNode[] = [
  { id: 'activity-root', kind: 'activity', parentFolderId: null, name: 'Root activity', sortOrder: 0, numberId: 1, status: 'Active' },
  { id: 'folder', kind: 'folder', parentFolderId: null, name: 'Safety', sortOrder: 1, numberId: null, status: null },
  { id: 'activity-child', kind: 'activity', parentFolderId: 'folder', name: 'Inspection', sortOrder: 0, numberId: 2, status: 'Active' },
  { id: 'activity-child-last', kind: 'activity', parentFolderId: 'folder', name: 'Last', sortOrder: 1, numberId: 3, status: 'Active' }
];
