import { TestBed } from '@angular/core/testing';
import { MatDialogRef } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardSitesService } from '../../../sites/services/dashboard-sites.service';
import { DashboardCamerasService } from '../../services/dashboard-cameras.service';
import { AddCameraDialogComponent } from './add-camera-dialog.component';

describe('AddCameraDialogComponent', () => {
  const camerasService = { createCameraMutation: { isPending: () => false }, createCamera: vi.fn() };
  const sitesService = { searchSites: vi.fn() };
  const dialogRef = { close: vi.fn() };

  beforeEach(async () => {
    vi.clearAllMocks();
    await TestBed.configureTestingModule({
      imports: [AddCameraDialogComponent],
      providers: [
        { provide: DashboardCamerasService, useValue: camerasService },
        { provide: DashboardSitesService, useValue: sitesService },
        { provide: MatDialogRef, useValue: dialogRef }
      ]
    }).compileComponents();
  });

  it('submits the selected site and converts blank optional connection fields to null', async () => {
    camerasService.createCamera.mockResolvedValue({ id: 'camera-42' });
    const fixture = TestBed.createComponent(AddCameraDialogComponent);
    const component = fixture.componentInstance;
    component.onSiteSelected({ option: { value: { id: 'site-42', numberId: 42, name: 'Head office', address: '42 Main Street' } } } as never);
    component.cameraForm.patchValue({ name: 'North gate', brand: 'Dahua', model: 'IPC-HDW', username: ' ', password: '', ipAddress: '  ', rtspPort: 554, ptzPort: 443, protocol: 'Https' });

    await component.submitCamera();

    expect(camerasService.createCamera).toHaveBeenCalledWith({ name: 'North gate', brand: 'Dahua', model: 'IPC-HDW', username: null, password: null, ipAddress: null, rtspPort: 554, ptzPort: 443, protocol: 'Https', siteId: 'site-42' });
    expect(dialogRef.close).toHaveBeenCalledWith(true);
  });
});
