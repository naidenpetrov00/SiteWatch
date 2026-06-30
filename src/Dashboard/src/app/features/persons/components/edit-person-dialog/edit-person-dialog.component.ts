import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { DashboardPersonDetails } from '../../models/dashboard-person-details.model';
import { DashboardPersonsService } from '../../services/dashboard-persons.service';
import { toUpdateDashboardPersonRequest } from '../../utils/dashboard-person-request.mapper';
import { EditPersonDialogContentComponent } from './edit-person-dialog-content.component';
import {
  EDIT_PERSON_DIALOG_FORM_ID,
  createEditPersonDialogForm
} from './edit-person-dialog.helpers';

@Component({
  selector: 'app-edit-person-dialog',
  imports: [DialogActionBarComponent, DialogShellComponent, EditPersonDialogContentComponent],
  templateUrl: './edit-person-dialog.component.html',
  styleUrl: './edit-person-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class EditPersonDialogComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly dialogRef = inject(MatDialogRef<EditPersonDialogComponent>);
  private readonly dashboardPersonsService = inject(DashboardPersonsService);
  private readonly dialogData = inject(MAT_DIALOG_DATA) as DashboardPersonDetails;

  readonly dialogEyebrow = 'Administration';
  readonly dialogTitle = `Edit Person #${this.dialogData.numberId}`;
  readonly dialogSubtitle = 'Update the enabled fields and keep the repeatable sections in sync.';
  readonly submitLabel = 'Save and close';
  readonly secondarySubmitLabel = 'Save';
  readonly showSecondarySubmit = true;
  readonly formId = EDIT_PERSON_DIALOG_FORM_ID;
  readonly isSaving = () => this.dashboardPersonsService.updatePersonMutation.isPending();
  readonly personForm = createEditPersonDialogForm(this.formBuilder, this.dialogData);

  closeDialog(): void {
    this.dialogRef.close();
  }

  savePerson(): Promise<void> {
    return this.submitPerson(false);
  }

  savePersonAndClose(): Promise<void> {
    return this.submitPerson(true);
  }

  private async submitPerson(closeAfterSave: boolean): Promise<void> {
    if (this.personForm.invalid) {
      this.personForm.markAllAsTouched();
      return;
    }

    try {
      const request = toUpdateDashboardPersonRequest(this.dialogData, this.personForm);
      await this.dashboardPersonsService.updatePerson(request);

      if (closeAfterSave) {
        this.dialogRef.close(true);
      }
    } catch {
      // Keep the dialog open and let the user correct the form or retry.
    }
  }
}
