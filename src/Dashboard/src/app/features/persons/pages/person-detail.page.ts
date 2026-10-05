import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input
} from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { CompanyExtractionProfilesComponent } from '../../retailers/components/retailer-extraction-profiles/retailer-extraction-profiles.component';
import { EditPersonDialogComponent } from '../components/edit-person-dialog/edit-person-dialog.component';
import { DashboardPersonsService } from '../services/dashboard-persons.service';

@Component({
  selector: 'app-person-detail-page',
  imports: [RouterLink, MatButtonModule, CompanyExtractionProfilesComponent],
  templateUrl: './person-detail.page.html',
  styleUrl: '../../products/pages/catalog-detail.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PersonDetailPage {
  private readonly personsService = inject(DashboardPersonsService);
  private readonly dialog = inject(MatDialog);

  readonly personId = input.required<string>();
  readonly runId = input<string>();
  readonly personQuery = injectQuery(() => {
    const personId = this.personId();
    return {
      queryKey: ['persons', 'detail', personId] as const,
      queryFn: () => this.personsService.getPersonById(personId),
      enabled: personId.length > 0
    };
  });

  async editPerson(): Promise<void> {
    const person = this.personQuery.data();
    if (!person) return;

    const dialogRef = this.dialog.open(EditPersonDialogComponent, {
      autoFocus: false,
      width: '72rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: person
    });
    if (await firstValueFrom(dialogRef.afterClosed())) {
      await this.personQuery.refetch();
    }
  }
}
