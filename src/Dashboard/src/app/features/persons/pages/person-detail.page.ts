import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  signal
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import { CompanyExtractionProfilesComponent } from '../../retailers/components/retailer-extraction-profiles/retailer-extraction-profiles.component';
import { EditPersonDialogComponent } from '../components/edit-person-dialog/edit-person-dialog.component';
import { DeletePersonDialogComponent } from '../components/delete-person-dialog/delete-person-dialog.component';
import { DashboardPersonsService } from '../services/dashboard-persons.service';
import { getPersonError } from '../utils/person-error';

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
  private readonly router = inject(Router);

  readonly personId = input.required<string>();
  readonly runId = input<string>();
  readonly deleteError = signal<string | null>(null);
  readonly isDeleting = () => this.personsService.deletePersonMutation.isPending();
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

  async deletePerson(): Promise<void> {
    const person = this.personQuery.data();
    if (!person) return;

    const confirmed = await firstValueFrom(
      this.dialog.open(DeletePersonDialogComponent, {
        autoFocus: false,
        width: '28rem',
        maxWidth: 'calc(100vw - 2rem)',
        data: {
          displayName: person.displayName,
          numberId: person.numberId
        }
      }).afterClosed()
    );

    if (confirmed !== true) return;

    this.deleteError.set(null);
    try {
      await this.personsService.deletePerson(person.id);
      await this.router.navigate(['/manage-persons']);
    } catch (error) {
      this.deleteError.set(getPersonError(error, 'The person could not be deleted. Please try again.'));
    }
  }
}
