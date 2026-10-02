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
  RetailerExtractionOverview,
  RetailerExtractionProfileDetails,
  RetailerExtractionProfileSummary,
  RetailerExtractionTestResult,
  TestRetailerExtractionProfileRequest,
  SaveRetailerExtractionRuleRequest
} from '../models/retailer-extraction-profile.models';

export const retailerExtractionProfileKeys = {
  root: (companyPersonId: string) =>
    ['retailer-extraction-profiles', companyPersonId] as const,
  versions: (companyPersonId: string) =>
    ['retailer-extraction-profiles', companyPersonId, 'versions'] as const,
  current: (companyPersonId: string) =>
    ['retailer-extraction-profiles', companyPersonId, 'current'] as const,
  overview: (companyPersonId: string) =>
    ['retailer-extraction-profiles', companyPersonId, 'overview'] as const,
  detail: (companyPersonId: string, profileId: string) =>
    ['retailer-extraction-profiles', companyPersonId, 'detail', profileId] as const
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

  getVersions(companyPersonId: string): Promise<readonly RetailerExtractionProfileSummary[]> {
    return firstValueFrom(
      this.http.get<readonly RetailerExtractionProfileSummary[]>(this.baseUrl(companyPersonId))
    );
  }

  getCurrent(companyPersonId: string): Promise<RetailerExtractionCurrentProfiles> {
    return firstValueFrom(
      this.http.get<RetailerExtractionCurrentProfiles>(
        `${this.baseUrl(companyPersonId)}/current`
      )
    );
  }

  getOverview(companyPersonId: string): Promise<RetailerExtractionOverview> {
    return firstValueFrom(
      this.http.get<RetailerExtractionOverview>(
        `${this.baseUrl(companyPersonId)}/overview`
      )
    );
  }

  getById(
    companyPersonId: string,
    profileId: string
  ): Promise<RetailerExtractionProfileDetails> {
    return firstValueFrom(
      this.http.get<RetailerExtractionProfileDetails>(
        `${this.baseUrl(companyPersonId)}/${profileId}`
      )
    );
  }

  async createDraft(
    companyPersonId: string,
    sourcePublishedProfileId?: string
  ): Promise<RetailerExtractionProfileDetails> {
    const profile = await this.profileMutation.mutateAsync(
      () => firstValueFrom(
        this.http.post<RetailerExtractionProfileDetails>(
          `${this.baseUrl(companyPersonId)}/drafts`,
          sourcePublishedProfileId ? { sourcePublishedProfileId } : {}
        )
      )
    );
    await this.afterProfileMutation(companyPersonId, profile);
    return profile;
  }

  async deleteDraft(companyPersonId: string, profileId: string): Promise<void> {
    await this.deleteMutation.mutateAsync(
      () => firstValueFrom(
        this.http.delete<void>(`${this.baseUrl(companyPersonId)}/${profileId}`)
      )
    );
    this.queryClient.removeQueries({
      queryKey: retailerExtractionProfileKeys.detail(companyPersonId, profileId),
      exact: true
    });
    await this.invalidateOverview(companyPersonId);
  }

  async updateAllowedHosts(
    companyPersonId: string,
    profileId: string,
    allowedHosts: readonly string[]
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      companyPersonId,
      () => firstValueFrom(
        this.http.put<RetailerExtractionProfileDetails>(
          `${this.baseUrl(companyPersonId)}/${profileId}/allowed-hosts`,
          { allowedHosts }
        )
      )
    );
  }

  async addRule(
    companyPersonId: string,
    profileId: string,
    request: SaveRetailerExtractionRuleRequest
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      companyPersonId,
      () => firstValueFrom(
        this.http.post<RetailerExtractionProfileDetails>(
          `${this.baseUrl(companyPersonId)}/${profileId}/rules`,
          request
        )
      )
    );
  }

  async updateRule(
    companyPersonId: string,
    profileId: string,
    ruleId: string,
    request: SaveRetailerExtractionRuleRequest
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      companyPersonId,
      () => firstValueFrom(
        this.http.put<RetailerExtractionProfileDetails>(
          `${this.baseUrl(companyPersonId)}/${profileId}/rules/${ruleId}`,
          request
        )
      )
    );
  }

  async deleteRule(
    companyPersonId: string,
    profileId: string,
    ruleId: string
  ): Promise<void> {
    await this.deleteMutation.mutateAsync(
      () => firstValueFrom(
        this.http.delete<void>(
          `${this.baseUrl(companyPersonId)}/${profileId}/rules/${ruleId}`
        )
      )
    );
    await this.invalidateProfile(companyPersonId, profileId);
  }

  async setRuleEnabled(
    companyPersonId: string,
    profileId: string,
    ruleId: string,
    isEnabled: boolean
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      companyPersonId,
      () => firstValueFrom(
        this.http.patch<RetailerExtractionProfileDetails>(
          `${this.baseUrl(companyPersonId)}/${profileId}/rules/${ruleId}/${isEnabled ? 'enable' : 'disable'}`,
          {}
        )
      )
    );
  }

  async reorderRules(
    companyPersonId: string,
    profileId: string,
    orderedRuleIds: readonly string[]
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      companyPersonId,
      () => firstValueFrom(
        this.http.put<RetailerExtractionProfileDetails>(
          `${this.baseUrl(companyPersonId)}/${profileId}/rules/order`,
          { orderedRuleIds }
        )
      )
    );
  }

  async publish(
    companyPersonId: string,
    profileId: string
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      companyPersonId,
      () => firstValueFrom(
        this.http.patch<RetailerExtractionProfileDetails>(
          `${this.baseUrl(companyPersonId)}/${profileId}/publish`,
          {}
        )
      ),
      true
    );
  }

  async test(
    companyPersonId: string,
    profileId: string,
    request: TestRetailerExtractionProfileRequest
  ): Promise<RetailerExtractionTestResult> {
    const result = await firstValueFrom(
      this.http.post<RetailerExtractionTestResult>(
        `${this.baseUrl(companyPersonId)}/${profileId}/test`,
        request
      )
    );
    await this.invalidateProfile(companyPersonId, profileId);
    return result;
  }

  async activate(
    companyPersonId: string,
    profileId: string
  ): Promise<RetailerExtractionProfileDetails> {
    return this.updateProfile(
      companyPersonId,
      () => firstValueFrom(
        this.http.patch<RetailerExtractionProfileDetails>(
          `${this.baseUrl(companyPersonId)}/${profileId}/activate`,
          {}
        )
      ),
      true
    );
  }

  private async updateProfile(
    companyPersonId: string,
    request: () => Promise<RetailerExtractionProfileDetails>,
    invalidateAllDetails = false
  ): Promise<RetailerExtractionProfileDetails> {
    const profile = await this.profileMutation.mutateAsync(request);
    await this.afterProfileMutation(companyPersonId, profile, invalidateAllDetails);
    return profile;
  }

  private async afterProfileMutation(
    companyPersonId: string,
    profile: RetailerExtractionProfileDetails,
    invalidateAllDetails = false
  ): Promise<void> {
    this.queryClient.setQueryData(
      retailerExtractionProfileKeys.detail(companyPersonId, profile.summary.id),
      profile
    );
    if (invalidateAllDetails) {
      await this.queryClient.invalidateQueries({
        queryKey: [...retailerExtractionProfileKeys.root(companyPersonId), 'detail']
      });
    }
    await this.invalidateOverview(companyPersonId);
  }

  private async invalidateProfile(
    companyPersonId: string,
    profileId: string
  ): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: retailerExtractionProfileKeys.detail(companyPersonId, profileId),
        exact: true
      }),
      this.invalidateOverview(companyPersonId)
    ]);
  }

  private async invalidateOverview(companyPersonId: string): Promise<void> {
    await Promise.all([
      this.queryClient.invalidateQueries({
        queryKey: retailerExtractionProfileKeys.versions(companyPersonId),
        exact: true
      }),
      this.queryClient.invalidateQueries({
        queryKey: retailerExtractionProfileKeys.current(companyPersonId),
        exact: true
      }),
      this.queryClient.invalidateQueries({
        queryKey: retailerExtractionProfileKeys.overview(companyPersonId),
        exact: true
      })
    ]);
  }

  private baseUrl(companyPersonId: string): string {
    return buildApiUrl(`/persons/${companyPersonId}/price-extraction-profiles`);
  }
}
