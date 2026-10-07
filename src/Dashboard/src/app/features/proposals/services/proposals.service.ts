import { DOCUMENT } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import {
  QueryClient,
  injectMutation,
  injectQuery
} from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { buildApiUrl } from '../../../core/api/api-url';
import {
  CreateProposalResponse,
  ProposalDetails,
  ProposalPdfAccess,
  ProposalSummary,
  UpdateProposalMetadataRequest
} from '../models/proposal.models';

interface ProposalHistoryRoute {
  siteId: string;
  offerId: string;
}

interface ProposalDetailRoute {
  siteId: string;
  proposalId: string;
}

@Injectable({ providedIn: 'root' })
export class ProposalsService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);
  private readonly document = inject(DOCUMENT);
  private readonly historyRoute = signal<ProposalHistoryRoute>({
    siteId: '',
    offerId: ''
  });
  private readonly detailRoute = signal<ProposalDetailRoute>({
    siteId: '',
    proposalId: ''
  });

  readonly historyQuery = injectQuery<readonly ProposalSummary[]>(() => {
    const route = this.historyRoute();
    return {
      queryKey: this.historyKey(route.siteId, route.offerId),
      queryFn: () =>
        firstValueFrom(
          this.http.get<readonly ProposalSummary[]>(
            buildApiUrl(`/sites/${route.siteId}/offers/${route.offerId}/proposals`)
          )
        ),
      enabled: route.siteId.length > 0 && route.offerId.length > 0
    };
  });

  readonly detailQuery = injectQuery<ProposalDetails>(() => {
    const route = this.detailRoute();
    return {
      queryKey: this.detailKey(route.siteId, route.proposalId),
      queryFn: () =>
        firstValueFrom(
          this.http.get<ProposalDetails>(
            buildApiUrl(`/sites/${route.siteId}/proposals/${route.proposalId}`)
          )
        ),
      enabled: route.siteId.length > 0 && route.proposalId.length > 0
    };
  });

  readonly createMutation = injectMutation<
    CreateProposalResponse,
    Error,
    ProposalHistoryRoute
  >(() => ({
    mutationKey: ['proposals', 'create'],
    mutationFn: ({ siteId, offerId }) =>
      firstValueFrom(
        this.http.post<CreateProposalResponse>(
          buildApiUrl(`/sites/${siteId}/offers/${offerId}/proposals`),
          {}
        )
      ),
    onSuccess: async (_, route) =>
      this.queryClient.invalidateQueries({
        queryKey: this.historyKey(route.siteId, route.offerId)
      })
  }));

  readonly updateMetadataMutation = injectMutation<
    void,
    Error,
    UpdateProposalMetadataRequest
  >(() => ({
    mutationKey: ['proposals', 'update-metadata'],
    mutationFn: ({ siteId, proposalId, validUntil, publicNotes, paymentTerms }) =>
      firstValueFrom(
        this.http.put<void>(
          buildApiUrl(`/sites/${siteId}/proposals/${proposalId}`),
          { validUntil, publicNotes, paymentTerms }
        )
      ),
    onSuccess: async (_, request) =>
      this.queryClient.invalidateQueries({
        queryKey: this.detailKey(request.siteId, request.proposalId)
      })
  }));

  readonly issueMutation = injectMutation<void, Error, ProposalDetailRoute>(
    () => ({
      mutationKey: ['proposals', 'issue'],
      mutationFn: ({ siteId, proposalId }) =>
        firstValueFrom(
          this.http.patch<void>(
            buildApiUrl(`/sites/${siteId}/proposals/${proposalId}/issue`),
            null
          )
        ),
      onSuccess: async (_, route) => {
        await this.queryClient.invalidateQueries({
          queryKey: this.detailKey(route.siteId, route.proposalId)
        });
        await this.queryClient.invalidateQueries({
          queryKey: ['proposals', 'site', route.siteId, 'offer']
        });
      }
    })
  );

  configureHistory(siteId: string, offerId: string): void {
    this.historyRoute.set({ siteId, offerId });
  }

  configureDetail(siteId: string, proposalId: string): void {
    this.detailRoute.set({ siteId, proposalId });
  }

  create(siteId: string, offerId: string): Promise<CreateProposalResponse> {
    return this.createMutation.mutateAsync({ siteId, offerId });
  }

  updateMetadata(request: UpdateProposalMetadataRequest): Promise<void> {
    return this.updateMetadataMutation.mutateAsync(request);
  }

  issue(siteId: string, proposalId: string): Promise<void> {
    return this.issueMutation.mutateAsync({ siteId, proposalId });
  }

  async downloadPdf(siteId: string, proposalId: string): Promise<void> {
    const access = await firstValueFrom(
      this.http.get<ProposalPdfAccess>(
        buildApiUrl(`/sites/${siteId}/proposals/${proposalId}/pdf-access`)
      )
    );
    const anchor = this.document.createElement('a');
    anchor.href = buildApiUrl(access.url);
    anchor.download = access.fileName;
    anchor.rel = 'noopener';
    this.document.body.appendChild(anchor);
    anchor.click();
    anchor.remove();
  }

  private historyKey(siteId: string, offerId: string) {
    return ['proposals', 'site', siteId, 'offer', offerId, 'history'] as const;
  }

  private detailKey(siteId: string, proposalId: string) {
    return ['proposals', 'site', siteId, 'detail', proposalId] as const;
  }
}
