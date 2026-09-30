import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import {
  QueryClient,
  injectMutation
} from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import {
  RetailerExtractionCurrentProfiles,
  RetailerExtractionProfileDetails,
  RetailerExtractionProfileSummary,
  SaveRetailerExtractionRuleRequest
} from '../models/retailer-extraction-profile.models';

export const retailerExtractionProfileKeys = {
  root: (retailerId: string) =>
    ['retailer-extraction-profiles', retailerId] as const,
  versions: (retailerId: string) =>
    ['retailer-extraction-profiles', retailerId, 'versions'] as const,
  current: (retailerId: string) =>
    ['retailer-extraction-profiles', retailerId, 'current'] as const,
  detail: (retailerId: string, profileId: string) =>
    ['retailer-extraction-profiles', retailerId, 'detail', profileId] as const
};

@Injectable({ providedIn: 'root' })
export class RetailerExtractionProfilesService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly profileMutation = injectMutation<
    RetailerExtractionProfileDetails,
    Error,
    () => Promise<RetailerExtractionProfileDetails>
  >(() => ({
    mutationKey: ['retailer-extraction-profiles', 'update'],
    mutationFn: (operation) => operation()
  }));
  private readonly deleteMutation = injectMutation<
    void,
    Error,
    () => Promise<void>
  >(() => ({
    mutationKey: ['retailer-extraction-profiles', 'delete'],
    mutationFn: (operation) => operation()
  }));

  getVersions(retailerId: string): Promise<readonly RetailerExtractionProfileSummary[]> {
    return firstValueFrom(
      this.http.get<readonly RetailerExtractionProfileSummary[]>(this.baseUrl(retailerId))
    );
  }

  getCurrent(retailerId: string): Promise<RetailerExtractionCurrentProfiles> {
    return firstValueFrom(
      this.http.get<RetailerExtractionCurrentProfiles>(
        `${this.baseUrl(retailerId)}/current`
      )
    );
  }

  getById(
    retailerId: string,
    profileId: string
  ): Promise<RetailerExtractionProfileDetails> {
    return firstValueFrom(
      this.http.get<RetailerExtractionProfileDetails>(
        `${this.baseUrl(retailerId)}/${profileId}`
      )
    );
  }

  async createDraft(
    retailerId: string,
    sourcePublishedProfileId?: string
  ): Promise<RetailerExtractionProfileDetails> {
    const profile = await this.profileMutation.mutateAsync(
      () => firstValueFrom(
        this.http.post<RetailerExtractionProfileDetails>(
          `${this.baseUrl(retailerId)}/drafts`,
          sourcePublishedProfileId ? { sourcePublishedProfileId } : {}
        )
      )
    );
    await this.afterProfileMutation(retailerId, profile);
    return profile;
  }

  async deleteDraft(retailerId: string, profileId: string): Promise<void> {
    await this.deleteMutation.mutateAsync(
      () => firstValueFrom(
        this.http.delete<void>(`${this.baseUrl(retailerId)}/${profileId}`)
      )
    );
    this.queryClient.removeQueries({
      queryKey: retailerExtractionProfileKeys.detail(retailerId, profileId),
      exact: true
    });
    await this.invalidateOverview(retailerId);
  }

  async updateAllowedHosts(
    retailerId: string,
    profileId: string,
    allowedHosts: readonly string[]
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      retailerId,
      () => firstValueFrom(
        this.http.put<RetailerExtractionProfileDetails>(
          `${this.baseUrl(retailerId)}/${profileId}/allowed-hosts`,
          { allowedHosts }
        )
      )
    );
  }

  async addRule(
    retailerId: string,
    profileId: string,
    request: SaveRetailerExtractionRuleRequest
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      retailerId,
      () => firstValueFrom(
        this.http.post<RetailerExtractionProfileDetails>(
          `${this.baseUrl(retailerId)}/${profileId}/rules`,
          request
        )
      )
    );
  }

  async updateRule(
    retailerId: string,
    profileId: string,
    ruleId: string,
    request: SaveRetailerExtractionRuleRequest
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      retailerId,
      () => firstValueFrom(
        this.http.put<RetailerExtractionProfileDetails>(
          `${this.baseUrl(retailerId)}/${profileId}/rules/${ruleId}`,
          request
        )
      )
    );
  }

  async deleteRule(
    retailerId: string,
    profileId: string,
    ruleId: string
  ): Promise<void> {
    await this.deleteMutation.mutateAsync(
      () => firstValueFrom(
        this.http.delete<void>(
          `${this.baseUrl(retailerId)}/${profileId}/rules/${ruleId}`
        )
      )
    );
    await this.invalidateProfile(retailerId, profileId);
  }

  async setRuleEnabled(
    retailerId: string,
    profileId: string,
    ruleId: string,
    isEnabled: boolean
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      retailerId,
      () => firstValueFrom(
        this.http.patch<RetailerExtractionProfileDetails>(
          `${this.baseUrl(retailerId)}/${profileId}/rules/${ruleId}/${isEnabled ? 'enable' : 'disable'}`,
          {}
        )
      )
    );
  }

  async reorderRules(
    retailerId: string,
    profileId: string,
    orderedRuleIds: readonly string[]
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      retailerId,
      () => firstValueFrom(
        this.http.put<RetailerExtractionProfileDetails>(
          `${this.baseUrl(retailerId)}/${profileId}/rules/order`,
          { orderedRuleIds }
        )
      )
    );
  }

  async publish(
    retailerId: string,
    profileId: string
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      retailerId,
      () => firstValueFrom(
        this.http.patch<RetailerExtractionProfileDetails>(
          `${this.baseUrl(retailerId)}/${profileId}/publish`,
          {}
        )
      ),
      true
    );
  }

  async activate(
    retailerId: string,
    profileId: string
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      retailerId,
      () => firstValueFrom(
        this.http.patch<RetailerExtractionProfileDetails>(
          `${this.baseUrl(retailerId)}/${profileId}/activate`,
          {}
        )
      ),
      true
    );
  }

  private async updateProfile(
    retailerId: string,
    request: () => Promise<RetailerExtractionProfileDetails>,
    invalidateAllDetails = false
  ): Promise<RetailerExtractionProfileDetails> {
    const profile = await this.profileMutation.mutateAsync(request);
    await this.afterProfileMutation(retailerId, profile, invalidateAllDetails);
    return profile;
  }

  private async afterProfileMutation(
    retailerId: string,
    profile: RetailerExtractionProfileDetails,
    invalidateAllDetails = false
  ): Promise<void> {
    this.queryClient.setQueryData(
      retailerExtractionProfileKeys.detail(retailerId, profile.summary.id),
      profile
    );
    if (invalidateAllDetails) {
      await this.queryClient.invalidateQueries({
        queryKey: [...retailerExtractionProfileKeys.root(retailerId), 'detail']
      });
    }
    await this.invalidateOverview(retailerId);
  }

  private async invalidateProfile(
    retailerId: string,
    profileId: string
  ): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: retailerExtractionProfileKeys.detail(retailerId, profileId),
        exact: true
      }),
      this.invalidateOverview(retailerId)
    ]);
  }

  private async invalidateOverview(retailerId: string): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: retailerExtractionProfileKeys.versions(retailerId),
        exact: true
      }),
      this.queryClient.invalidateQueries({
        queryKey: retailerExtractionProfileKeys.current(retailerId),
        exact: true
      })
    ]);
  }

  private baseUrl(retailerId: string): string {
    return buildApiUrl(`/retailers/${retailerId}/price-extraction-profiles`);
  }
}
