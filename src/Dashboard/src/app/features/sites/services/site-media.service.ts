import { HttpClient, HttpEvent } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { injectQuery, QueryClient } from '@tanstack/angular-query-experimental';
import { firstValueFrom, Observable } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import { SiteMediaCollection, SiteMediaKind } from '../models/site-media.model';

@Injectable({ providedIn: 'root' })
export class SiteMediaService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly siteId = signal<string | null>(null);

  readonly mediaQuery = injectQuery<SiteMediaCollection>(() => {
    const siteId = this.siteId();

    return {
      queryKey: ['sites', 'media', siteId] as const,
      enabled: siteId !== null,
      queryFn: async () => this.getMedia(siteId!)
    };
  });

  setSiteId(siteId: string): void {
    this.siteId.set(siteId);
  }

  upload(siteId: string, kind: SiteMediaKind, file: File, classification: string): Observable<HttpEvent<unknown>> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    formData.append(kind === 'files' ? 'documentType' : 'category', classification);

    return this.http.post(
      buildApiUrl(this.uploadPath(kind, siteId)),
      formData,
      { observe: 'events', reportProgress: true }
    );
  }

  async delete(kind: SiteMediaKind, id: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(buildApiUrl(this.itemPath(kind, id))));
    await this.invalidate();
  }

  getBlob(kind: SiteMediaKind, id: string, snapshot = false): Promise<Blob> {
    return firstValueFrom(this.http.get(buildApiUrl(this.itemPath(kind, id, snapshot)), {
      responseType: 'blob'
    }));
  }

  async invalidate(): Promise<void> {
    await this.queryClient.invalidateQueries({ queryKey: ['sites', 'media', this.siteId()] });
  }

  private getMedia(siteId: string): Promise<SiteMediaCollection> {
    return Promise.all([
      firstValueFrom(this.http.get<SiteMediaCollection['images']>(buildApiUrl(`/images/images${siteId}`))),
      firstValueFrom(this.http.get<SiteMediaCollection['videos']>(buildApiUrl(`/videos/site/${siteId}`))),
      firstValueFrom(this.http.get<SiteMediaCollection['files']>(buildApiUrl(`/files/files${siteId}`)))
    ]).then(([images, videos, files]) => ({ images, videos, files }));
  }

  private uploadPath(kind: SiteMediaKind, siteId: string): string {
    return `/${kind === 'images' ? 'images' : kind === 'videos' ? 'videos' : 'files'}/${siteId}`;
  }

  private itemPath(kind: SiteMediaKind, id: string, snapshot = false): string {
    if (kind === 'videos' && snapshot) return `/videos/snapshot/${id}`;
    return `/${kind === 'images' ? 'images' : kind === 'videos' ? 'videos' : 'files'}/${id}`;
  }
}
