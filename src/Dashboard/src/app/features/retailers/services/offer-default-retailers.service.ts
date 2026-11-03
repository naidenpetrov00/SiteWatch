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
  OfferDefaultRetailer,
  ReplaceOfferDefaultRetailersRequest
} from '../models/offer-default-retailer.models';

export const offerDefaultRetailerKeys = {
  all: ['retailers', 'offer-defaults'] as const
};

@Injectable({ providedIn: 'root' })
export class OfferDefaultRetailersService {
  private readonly http = inject(HttpClient);
  private readonly queryClient = inject(QueryClient);

  readonly defaultsQuery = injectQuery<readonly OfferDefaultRetailer[]>(() => ({
    queryKey: offerDefaultRetailerKeys.all,
    queryFn: async () =>
      firstValueFrom(
        this.http.get<readonly OfferDefaultRetailer[]>(
          buildApiUrl('/dashboard/offer-default-retailers')
        )
      )
  }));

  readonly replaceMutation = injectMutation<
    readonly OfferDefaultRetailer[],
    Error,
    ReplaceOfferDefaultRetailersRequest
  >(() => ({
    mutationKey: ['retailers', 'offer-defaults', 'replace'],
    mutationFn: async (request) =>
      firstValueFrom(
        this.http.put<readonly OfferDefaultRetailer[]>(
          buildApiUrl('/dashboard/offer-default-retailers'),
          request
        )
      ),
    onSuccess: async (defaults) => {
      this.queryClient.setQueryData(offerDefaultRetailerKeys.all, defaults);
      await this.invalidate();
    }
  }));

  replace(retailerIds: readonly string[]): Promise<readonly OfferDefaultRetailer[]> {
    return this.replaceMutation.mutateAsync({ retailerIds });
  }

  async invalidate(): Promise<void> {
    await this.queryClient.invalidateQueries({
      queryKey: offerDefaultRetailerKeys.all,
      exact: true
    });
  }
}
