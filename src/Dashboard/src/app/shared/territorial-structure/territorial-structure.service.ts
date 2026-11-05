import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../core/api/api-url';
import { SaveTerritorialEntryRequest, TerritorialEntry, TerritorialEntryKind, UpdateTerritorialEntryRequest } from './territorial-structure.models';

@Injectable({ providedIn: 'root' })
export class TerritorialStructureService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);

  async getEntries(kind: TerritorialEntryKind, parentId?: string | null, searchTerm = '', includeInactive = false): Promise<readonly TerritorialEntry[]> {
    let params = new HttpParams().set('kind', kind).set('includeInactive', includeInactive);
    if (parentId) params = params.set('parentId', parentId);
    if (searchTerm.trim()) params = params.set('searchTerm', searchTerm.trim());
    return firstValueFrom(this.http.get<readonly TerritorialEntry[]>(buildApiUrl('/territorial-structure'), { params }));
  }

  async create(request: SaveTerritorialEntryRequest): Promise<void> {
    await firstValueFrom(this.http.post(buildApiUrl('/territorial-structure'), request));
    await this.invalidate();
  }

  async update(id: string, request: UpdateTerritorialEntryRequest): Promise<void> {
    await firstValueFrom(this.http.put<void>(buildApiUrl(`/territorial-structure/${id}`), request));
    await this.invalidate();
  }

  async delete(id: string, kind: TerritorialEntryKind): Promise<void> {
    await firstValueFrom(this.http.delete<void>(buildApiUrl(`/territorial-structure/${id}`), {
      params: new HttpParams().set('kind', kind)
    }));
    await this.invalidate();
  }

  private async invalidate(): Promise<void> {
    await this.queryClient.invalidateQueries({ queryKey: ['territorial-structure'] });
  }
}
