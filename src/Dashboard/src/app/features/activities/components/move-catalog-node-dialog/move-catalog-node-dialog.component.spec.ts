import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { ActivityCatalogNode } from '../../models/activity-catalog.models';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { MoveCatalogNodeDialogComponent, MoveCatalogNodeDialogData } from './move-catalog-node-dialog.component';

describe('MoveCatalogNodeDialogComponent', () => {
  let dialogData: MoveCatalogNodeDialogData;
  const catalogService = {
    mutation: { isPending: () => false }, moveFolder: vi.fn(), moveActivity: vi.fn()
  };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    dialogData = { node: nodes[1], nodes };
    await TestBed.configureTestingModule({
      imports: [MoveCatalogNodeDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useFactory: () => dialogData },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: ActivityCatalogService, useValue: catalogService }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('excludes a moving folder and its descendants from destination options', () => {
    const component = TestBed.createComponent(MoveCatalogNodeDialogComponent).componentInstance;

    expect(component.folderOptions).toEqual([
      { id: null, label: 'Root' },
      { id: 'other', label: 'Other folder' },
      { id: 'root', label: 'Root folder' }
    ]);
  });

  it('converts the selected preceding sibling into the destination index and moves a folder', async () => {
    catalogService.moveFolder.mockResolvedValue(undefined);
    const component = TestBed.createComponent(MoveCatalogNodeDialogComponent).componentInstance;
    component.onParentChange({ value: null } as never);
    component.form.controls.precedingSiblingId.setValue('root');

    await component.submit();

    expect(catalogService.moveFolder).toHaveBeenCalledWith('selected', {
      targetParentFolderId: 'root', targetIndex: 1
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('rejects a stale position instead of issuing an activity move', async () => {
    dialogData = { node: nodes[4], nodes };
    const component = TestBed.createComponent(MoveCatalogNodeDialogComponent).componentInstance;
    component.form.patchValue({ precedingSiblingId: 'missing' });

    await component.submit();

    expect(catalogService.moveActivity).not.toHaveBeenCalled();
    expect(component.errorMessage()).toBe('The selected position is no longer available.');
  });

  it('dispatches an activity move to the activity contract', async () => {
    dialogData = { node: nodes[4], nodes };
    catalogService.moveActivity.mockResolvedValue(undefined);
    const component = TestBed.createComponent(MoveCatalogNodeDialogComponent).componentInstance;
    component.form.controls.precedingSiblingId.setValue('root');

    await component.submit();

    expect(catalogService.moveActivity).toHaveBeenCalledWith('activity', {
      targetParentFolderId: null, targetIndex: 1
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });
});

const nodes: readonly ActivityCatalogNode[] = [
  { id: 'root', kind: 'folder', parentFolderId: null, name: 'Root folder', sortOrder: 0, numberId: null, status: null },
  { id: 'selected', kind: 'folder', parentFolderId: 'root', name: 'Selected', sortOrder: 0, numberId: null, status: null },
  { id: 'child', kind: 'folder', parentFolderId: 'selected', name: 'Child', sortOrder: 0, numberId: null, status: null },
  { id: 'other', kind: 'folder', parentFolderId: null, name: 'Other folder', sortOrder: 1, numberId: null, status: null },
  { id: 'activity', kind: 'activity', parentFolderId: null, name: 'Inspection', sortOrder: 2, numberId: 1, status: 'Active' }
];
