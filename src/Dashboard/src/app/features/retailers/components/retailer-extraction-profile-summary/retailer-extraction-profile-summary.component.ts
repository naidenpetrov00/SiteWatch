import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { injectQuery } from '@tanstack/angular-query-experimental';

import {
  RetailerExtractionProfilesService,
  retailerExtractionProfileKeys
} from '../../services/retailer-extraction-profiles.service';

@Component({
  selector: 'app-retailer-extraction-profile-summary',
  imports: [RouterLink, MatButtonModule],
  templateUrl: './retailer-extraction-profile-summary.component.html',
  styleUrl: './retailer-extraction-profile-summary.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerExtractionProfileSummaryComponent {
  private readonly service = inject(RetailerExtractionProfilesService);

  readonly companyPersonId = input.required<string>();
  readonly overviewQuery = injectQuery(() => {
    const companyPersonId = this.companyPersonId();
    return {
      queryKey: retailerExtractionProfileKeys.overview(companyPersonId),
      queryFn: () => this.service.getOverview(companyPersonId),
      enabled: companyPersonId.length > 0
    };
  });
}
