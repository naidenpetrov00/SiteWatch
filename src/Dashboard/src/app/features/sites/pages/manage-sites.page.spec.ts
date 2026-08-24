import { TestBed } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { vi } from 'vitest';

import { DashboardSite } from '../models/dashboard-site.model';
import { DashboardSitesService } from '../services/dashboard-sites.service';
import { ManageSitesPage } from './manage-sites.page';

describe('ManageSitesPage', () => {
  const sitesService = {
    dashboardSitesQuery: { data: () => undefined },
    setTableState: vi.fn()
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageSitesPage],
      providers: [
        { provide: DashboardSitesService, useValue: sitesService },
        { provide: MatDialog, useValue: { open: vi.fn() } }
      ]
    }).compileComponents();
  });

  it('formats the nested media policy preset for the site table', () => {
    const fixture = TestBed.createComponent(ManageSitesPage);
    const mediaPolicyColumn = fixture.componentInstance.columns.find(
      (column) => column.key === 'mediaPolicy'
    )!;
    const site = {
      id: 'site-42',
      numberId: 42,
      name: 'Commercial Site',
      address: '42 Main Street',
      managerId: 'manager-1',
      managerDisplayName: 'Manager One',
      startDate: '2026-03-03',
      endDate: null,
      status: 'Planning',
      mediaPolicy: { preset: 'CommercialBuild', categories: ['Structure', 'Other'] }
    } satisfies DashboardSite;

    const value = mediaPolicyColumn.valueAccessor?.(site);
    const display = mediaPolicyColumn.displayFormatter?.(value, site);

    expect(value).toBe('CommercialBuild');
    expect(display).toBe('Commercial Build');
  });
});
