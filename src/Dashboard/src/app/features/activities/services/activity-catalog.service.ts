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
  CreateActivityProductRequirementRequest,
  CreateActivityFolderRequest,
  CreateActivityRequest,
  MoveActivityRequirementRequest,
  MoveCatalogNodeRequest,
  UpdateActivityProductRequirementRequest,
  UpsertActivityRequirementSectionRequest,
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
  | { operation: 'delete-activity'; id: string }
  | {
      operation: 'create-requirement-section';
      activityId: string;
      request: UpsertActivityRequirementSectionRequest;
    }
  | {
      operation: 'update-requirement-section';
      activityId: string;
      sectionId: string;
      request: UpsertActivityRequirementSectionRequest;
    }
  | {
      operation: 'move-requirement-section';
      activityId: string;
      sectionId: string;
      request: MoveActivityRequirementRequest;
    }
  | {
      operation: 'delete-requirement-section';
      activityId: string;
      sectionId: string;
    }
  | {
      operation: 'create-product-requirement';
      activityId: string;
      sectionId: string;
      request: CreateActivityProductRequirementRequest;
    }
  | {
      operation: 'update-product-requirement';
      activityId: string;
      sectionId: string;
      requirementId: string;
      request: UpdateActivityProductRequirementRequest;
    }
  | {
      operation: 'move-product-requirement';
      activityId: string;
      sectionId: string;
      requirementId: string;
      request: MoveActivityRequirementRequest;
    }
  | {
      operation: 'delete-product-requirement';
      activityId: string;
      sectionId: string;
      requirementId: string;
    };

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

  createRequirementSection(
    activityId: string,
    request: UpsertActivityRequirementSectionRequest
  ): Promise<unknown> {
    return this.mutation.mutateAsync({
      operation: 'create-requirement-section',
      activityId,
      request
    });
  }

  updateRequirementSection(
    activityId: string,
    sectionId: string,
    request: UpsertActivityRequirementSectionRequest
  ): Promise<unknown> {
    return this.mutation.mutateAsync({
      operation: 'update-requirement-section',
      activityId,
      sectionId,
      request
    });
  }

  moveRequirementSection(
    activityId: string,
    sectionId: string,
    request: MoveActivityRequirementRequest
  ): Promise<unknown> {
    return this.mutation.mutateAsync({
      operation: 'move-requirement-section',
      activityId,
      sectionId,
      request
    });
  }

  deleteRequirementSection(
    activityId: string,
    sectionId: string
  ): Promise<unknown> {
    return this.mutation.mutateAsync({
      operation: 'delete-requirement-section',
      activityId,
      sectionId
    });
  }

  createProductRequirement(
    activityId: string,
    sectionId: string,
    request: CreateActivityProductRequirementRequest
  ): Promise<unknown> {
    return this.mutation.mutateAsync({
      operation: 'create-product-requirement',
      activityId,
      sectionId,
      request
    });
  }

  updateProductRequirement(
    activityId: string,
    sectionId: string,
    requirementId: string,
    request: UpdateActivityProductRequirementRequest
  ): Promise<unknown> {
    return this.mutation.mutateAsync({
      operation: 'update-product-requirement',
      activityId,
      sectionId,
      requirementId,
      request
    });
  }

  moveProductRequirement(
    activityId: string,
    sectionId: string,
    requirementId: string,
    request: MoveActivityRequirementRequest
  ): Promise<unknown> {
    return this.mutation.mutateAsync({
      operation: 'move-product-requirement',
      activityId,
      sectionId,
      requirementId,
      request
    });
  }

  deleteProductRequirement(
    activityId: string,
    sectionId: string,
    requirementId: string
  ): Promise<unknown> {
    return this.mutation.mutateAsync({
      operation: 'delete-product-requirement',
      activityId,
      sectionId,
      requirementId
    });
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
      case 'create-requirement-section':
        return firstValueFrom(
          this.http.post<CreatedResponse>(
            buildApiUrl(
              `/activities/${mutation.activityId}/requirement-sections`
            ),
            mutation.request
          )
        );
      case 'update-requirement-section':
        return firstValueFrom(
          this.http.put<void>(
            buildApiUrl(
              `/activities/${mutation.activityId}/requirement-sections/${mutation.sectionId}`
            ),
            mutation.request
          )
        );
      case 'move-requirement-section':
        return firstValueFrom(
          this.http.patch<void>(
            buildApiUrl(
              `/activities/${mutation.activityId}/requirement-sections/${mutation.sectionId}/move`
            ),
            mutation.request
          )
        );
      case 'delete-requirement-section':
        return firstValueFrom(
          this.http.delete<void>(
            buildApiUrl(
              `/activities/${mutation.activityId}/requirement-sections/${mutation.sectionId}`
            )
          )
        );
      case 'create-product-requirement':
        return firstValueFrom(
          this.http.post<CreatedResponse>(
            buildApiUrl(
              `/activities/${mutation.activityId}/requirement-sections/${mutation.sectionId}/products`
            ),
            mutation.request
          )
        );
      case 'update-product-requirement':
        return firstValueFrom(
          this.http.put<void>(
            buildApiUrl(
              `/activities/${mutation.activityId}/requirement-sections/${mutation.sectionId}/products/${mutation.requirementId}`
            ),
            mutation.request
          )
        );
      case 'move-product-requirement':
        return firstValueFrom(
          this.http.patch<void>(
            buildApiUrl(
              `/activities/${mutation.activityId}/requirement-sections/${mutation.sectionId}/products/${mutation.requirementId}/move`
            ),
            mutation.request
          )
        );
      case 'delete-product-requirement':
        return firstValueFrom(
          this.http.delete<void>(
            buildApiUrl(
              `/activities/${mutation.activityId}/requirement-sections/${mutation.sectionId}/products/${mutation.requirementId}`
            )
          )
        );
    }

    throw new Error('Unsupported activity catalog operation.');
  }
}
