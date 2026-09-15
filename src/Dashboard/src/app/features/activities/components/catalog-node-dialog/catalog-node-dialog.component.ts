import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import { ActivityDetails } from '../../models/activity-catalog.models';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { getActivityCatalogError } from '../../utils/activity-catalog-error';

export type CatalogNodeDialogData =
  | {
      kind: 'folder';
      mode: 'create' | 'edit';
      parentFolderId: string | null;
      id?: string;
      name?: string;
    }
  | {
      kind: 'activity';
      mode: 'create' | 'edit' | 'view';
      parentFolderId: string | null;
      id?: string;
      activity?: ActivityDetails;
    };

@Component({
  selector: 'app-catalog-node-dialog',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './catalog-node-dialog.component.html',
  styleUrl: './catalog-node-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CatalogNodeDialogComponent {
  readonly data = inject<CatalogNodeDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<CatalogNodeDialogComponent>);
  private readonly formBuilder = inject(FormBuilder);
  private readonly catalogService = inject(ActivityCatalogService);

  readonly errorMessage = signal<string | null>(null);
  readonly isViewMode = this.data.kind === 'activity' && this.data.mode === 'view';
  readonly formId = 'activity-catalog-node-form';
  readonly activity = this.data.kind === 'activity' ? this.data.activity : undefined;
  readonly title = this.resolveTitle();
  readonly subtitle = this.resolveSubtitle();
  readonly submitLabel = this.data.mode === 'create' ? 'Add' : 'Save';
  readonly form = this.formBuilder.group({
    name: this.formBuilder.nonNullable.control(this.initialName(), [
      Validators.required,
      Validators.maxLength(200)
    ]),
    description: this.formBuilder.nonNullable.control(this.activity?.description ?? '', [
      Validators.maxLength(2000)
    ])
  });

  readonly isSaving = (): boolean => this.catalogService.mutation.isPending();

  close(): void {
    this.dialogRef.close(false);
  }

  async submit(): Promise<void> {
    if (this.isViewMode) {
      this.close();
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    const value = this.form.getRawValue();
    const name = value.name.trim().replace(/\s+/g, ' ');
    const description = value.description.trim() || null;

    try {
      if (this.data.kind === 'folder') {
        if (this.data.mode === 'create') {
          await this.catalogService.createFolder({
            name,
            parentFolderId: this.data.parentFolderId
          });
        } else {
          await this.catalogService.renameFolder(this.requireExistingId(), name);
        }
      } else if (this.data.mode === 'create') {
        await this.catalogService.createActivity({
          name,
          description,
          parentFolderId: this.data.parentFolderId
        });
      } else {
        await this.catalogService.updateActivity(this.requireExistingId(), {
          name,
          description
        });
      }

      this.dialogRef.close(true);
    } catch (error) {
      this.errorMessage.set(getActivityCatalogError(error));
    }
  }

  private initialName(): string {
    return this.data.kind === 'folder'
      ? (this.data.name ?? '')
      : (this.data.activity?.name ?? '');
  }

  private requireExistingId(): string {
    if (this.data.id) {
      return this.data.id;
    }
    if (this.data.kind === 'activity' && this.data.activity) {
      return this.data.activity.id;
    }
    throw new Error('An existing catalog node ID is required.');
  }

  private resolveTitle(): string {
    if (this.data.kind === 'folder') {
      return this.data.mode === 'create' ? 'Add Folder' : 'Rename Folder';
    }
    if (this.data.mode === 'create') return 'Add Activity';
    if (this.data.mode === 'view') return 'Activity Details';
    return 'Edit Activity';
  }

  private resolveSubtitle(): string {
    if (this.isViewMode && this.activity) {
      return `Activity #${this.activity.numberId}`;
    }
    return this.data.mode === 'create'
      ? 'The new item will be added at the end of this location.'
      : 'Names must be unique within this location.';
  }
}
