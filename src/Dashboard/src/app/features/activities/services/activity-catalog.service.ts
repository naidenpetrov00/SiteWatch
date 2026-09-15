import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import {
  QueryClient,
  injectMutation,
  injectQuery
} from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import {
  ActivityCatalogNode,
  ActivityDetails,
  CreateActivityFolderRequest,
  CreateActivityRequest,
  MoveCatalogNodeRequest,
  UpdateActivityRequest
} from '../models/activity-catalog.models';

interface CreatedResponse {
  id: string;
}

type CatalogMutation =
  | { operation: 'create-folder'; request: CreateActivityFolderRequest }
  | { operation: 'rename-folder'; id: string; request: { name: string } }
  | { operation: 'move-folder'; id: string; request: MoveCatalogNodeRequest }
  | { operation: 'delete-folder'; id: string }
  | { operation: 'create-activity'; request: CreateActivityRequest }
  | { operation: 'update-activity'; id: string; request: UpdateActivityRequest }
  | { operation: 'move-activity'; id: string; request: MoveCatalogNodeRequest }
  | { operation: 'archive-activity'; id: string }
  | { operation: 'restore-activity'; id: string }
  | { operation: 'delete-activity'; id: string };

@Injectable({ providedIn: 'root' })
export class ActivityCatalogService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);

  readonly catalogQuery = injectQuery<readonly ActivityCatalogNode[]>(() => ({
    queryKey: ['activity-catalog'] as const,
    queryFn: async () =>
      firstValueFrom(
        this.http.get<readonly ActivityCatalogNode[]>(
          buildApiUrl('/dashboard/activity-catalog')
        )
      )
  }));

  readonly mutation = injectMutation<unknown, Error, CatalogMutation>(() => ({
    mutationKey: ['activity-catalog', 'mutation'],
    mutationFn: (mutation) => this.executeMutation(mutation),
    onSuccess: async () => {
      await this.queryClient.invalidateQueries({ queryKey: ['activity-catalog'] });
    }
  }));

  createFolder(request: CreateActivityFolderRequest): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'create-folder', request });
  }

  renameFolder(id: string, name: string): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'rename-folder', id, request: { name } });
  }

  moveFolder(id: string, request: MoveCatalogNodeRequest): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'move-folder', id, request });
  }

  deleteFolder(id: string): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'delete-folder', id });
  }

  createActivity(request: CreateActivityRequest): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'create-activity', request });
  }

  updateActivity(id: string, request: UpdateActivityRequest): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'update-activity', id, request });
  }

  moveActivity(id: string, request: MoveCatalogNodeRequest): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'move-activity', id, request });
  }

  archiveActivity(id: string): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'archive-activity', id });
  }

  restoreActivity(id: string): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'restore-activity', id });
  }

  deleteActivity(id: string): Promise<unknown> {
    return this.mutation.mutateAsync({ operation: 'delete-activity', id });
  }

  getActivity(id: string): Promise<ActivityDetails> {
    return firstValueFrom(
      this.http.get<ActivityDetails>(buildApiUrl(`/activities/${id}`))
    );
  }

  private executeMutation(mutation: CatalogMutation): Promise<unknown> {
    switch (mutation.operation) {
      case 'create-folder':
        return firstValueFrom(
          this.http.post<CreatedResponse>(
            buildApiUrl('/activity-folders'),
            mutation.request
          )
        );
      case 'rename-folder':
        return firstValueFrom(
          this.http.put<void>(
            buildApiUrl(`/activity-folders/${mutation.id}`),
            mutation.request
          )
        );
      case 'move-folder':
        return firstValueFrom(
          this.http.patch<void>(
            buildApiUrl(`/activity-folders/${mutation.id}/move`),
            mutation.request
          )
        );
      case 'delete-folder':
        return firstValueFrom(
          this.http.delete<void>(buildApiUrl(`/activity-folders/${mutation.id}`))
        );
      case 'create-activity':
        return firstValueFrom(
          this.http.post<CreatedResponse>(buildApiUrl('/activities'), mutation.request)
        );
      case 'update-activity':
        return firstValueFrom(
          this.http.put<void>(buildApiUrl(`/activities/${mutation.id}`), mutation.request)
        );
      case 'move-activity':
        return firstValueFrom(
          this.http.patch<void>(
            buildApiUrl(`/activities/${mutation.id}/move`),
            mutation.request
          )
        );
      case 'archive-activity':
        return firstValueFrom(
          this.http.patch<void>(buildApiUrl(`/activities/${mutation.id}/archive`), {})
        );
      case 'restore-activity':
        return firstValueFrom(
          this.http.patch<void>(buildApiUrl(`/activities/${mutation.id}/restore`), {})
        );
      case 'delete-activity':
        return firstValueFrom(
          this.http.delete<void>(buildApiUrl(`/activities/${mutation.id}`))
        );
    }

    throw new Error('Unsupported activity catalog operation.');
  }
}
