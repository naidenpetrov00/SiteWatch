import { TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { CatalogNodeDialogComponent, CatalogNodeDialogData } from './catalog-node-dialog.component';

describe('CatalogNodeDialogComponent', () => {
  let dialogData: CatalogNodeDialogData;
  const catalogService = {
    mutation: { isPending: () => false },
    createFolder: vi.fn(), renameFolder: vi.fn(), createActivity: vi.fn(), updateActivity: vi.fn()
  };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    dialogData = { kind: 'activity', mode: 'create', parentFolderId: 'folder-1' };
    await TestBed.configureTestingModule({
      imports: [CatalogNodeDialogComponent],
      providers: [
        { provide: MAT_DIALOG_DATA, useFactory: () => dialogData },
        { provide: MatDialogRef, useValue: dialogRef },
        { provide: ActivityCatalogService, useValue: catalogService }
      ]
    }).compileComponents();
    vi.clearAllMocks();
  });

  it('normalizes and submits a valid activity creation request', async () => {
    catalogService.createActivity.mockResolvedValue({ id: 'activity-1' });
    const component = TestBed.createComponent(CatalogNodeDialogComponent).componentInstance;
    component.form.patchValue({ name: '  Daily   inspection ', description: '  Check gate  ' });

    await component.submit();

    expect(catalogService.createActivity).toHaveBeenCalledWith({
      name: 'Daily inspection', description: 'Check gate', parentFolderId: 'folder-1'
    });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });

  it('marks invalid input and keeps the editor open while exposing save failures', async () => {
    const component = TestBed.createComponent(CatalogNodeDialogComponent).componentInstance;
    await component.submit();
    expect(component.form.controls.name.touched).toBe(true);
    expect(catalogService.createActivity).not.toHaveBeenCalled();

    component.form.patchValue({ name: 'Inspection', description: '' });
    catalogService.createActivity.mockRejectedValue(new Error('offline'));
    await component.submit();

    expect(dialogRef.close).not.toHaveBeenCalled();
    expect(component.errorMessage()).toContain('could not be updated');
  });

  it('dispatches an existing folder edit through the rename contract', async () => {
    dialogData = { kind: 'folder', mode: 'edit', id: 'folder-1', parentFolderId: null, name: 'Safety' };
    catalogService.renameFolder.mockResolvedValue(undefined);
    const component = TestBed.createComponent(CatalogNodeDialogComponent).componentInstance;
    component.form.controls.name.setValue('  Updated   safety ');

    await component.submit();

    expect(catalogService.renameFolder).toHaveBeenCalledWith('folder-1', 'Updated safety');
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });
});
