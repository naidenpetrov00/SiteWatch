import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { injectQuery } from '@tanstack/angular-query-experimental';

import { buildApiUrl } from '../../../core/api/api-url';
import { DashboardUser } from '../models/dashboard-user.model';

@Injectable({
  providedIn: 'root'
})
export class DashboardUsersService {
  private readonly http = inject(HttpClient);

  readonly dashboardUsersQuery = injectQuery<readonly DashboardUser[]>(() => ({
    queryKey: ['users', 'dashboard'] as const,
    queryFn: async () =>
      firstValueFrom(
        this.http.get<readonly DashboardUser[]>(buildApiUrl('/dashboard/users'))
      )
  }));
}
