import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectChange, MatSelectModule } from '@angular/material/select';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  ActivityCatalogNode,
  ActivityFolderNode
} from '../../models/activity-catalog.models';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { getActivityCatalogError } from '../../utils/activity-catalog-error';

export interface MoveCatalogNodeDialogData {
  node: ActivityCatalogNode;
  nodes: readonly ActivityCatalogNode[];
}

interface FolderOption {
  id: string | null;
  label: string;
}

@Component({
  selector: 'app-move-catalog-node-dialog',
  imports: [
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './move-catalog-node-dialog.component.html',
  styleUrl: './move-catalog-node-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MoveCatalogNodeDialogComponent {
  private readonly data = inject<MoveCatalogNodeDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(MatDialogRef<MoveCatalogNodeDialogComponent>);
  private readonly formBuilder = inject(FormBuilder);
  private readonly catalogService = inject(ActivityCatalogService);

  readonly node = this.data.node;
  readonly formId = 'move-activity-catalog-node-form';
  readonly errorMessage = signal<string | null>(null);
  readonly selectedParentFolderId = signal<string | null>(this.node.parentFolderId);
  readonly folderOptions = this.buildFolderOptions();
  readonly positionOptions = computed(() =>
    this.data.nodes
      .filter(
        (node) =>
          node.parentFolderId === this.selectedParentFolderId() && node.id !== this.node.id
      )
      .sort((left, right) => left.sortOrder - right.sortOrder)
  );
  readonly form = this.formBuilder.group({
    targetParentFolderId: this.formBuilder.control<string | null>(
      this.node.parentFolderId
    ),
    precedingSiblingId: this.formBuilder.control<string | null>(
      this.initialPrecedingSiblingId()
    )
  });
  readonly isSaving = (): boolean => this.catalogService.mutation.isPending();

  onParentChange(change: MatSelectChange): void {
    const parentFolderId = (change.value as string | null) ?? null;
    this.selectedParentFolderId.set(parentFolderId);
    this.form.controls.precedingSiblingId.setValue(null);
  }

  close(): void {
    this.dialogRef.close(false);
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);
    const value = this.form.getRawValue();
    const siblings = this.positionOptions();
    const precedingSiblingIndex = value.precedingSiblingId
      ? siblings.findIndex((sibling) => sibling.id === value.precedingSiblingId)
      : -1;
    if (value.precedingSiblingId && precedingSiblingIndex < 0) {
      this.errorMessage.set('The selected position is no longer available.');
      return;
    }
    const targetIndex = precedingSiblingIndex + 1;

    const request = {
      targetParentFolderId: value.targetParentFolderId,
      targetIndex
    };

    try {
      if (this.node.kind === 'folder') {
        await this.catalogService.moveFolder(this.node.id, request);
      } else {
        await this.catalogService.moveActivity(this.node.id, request);
      }
      this.dialogRef.close(true);
    } catch (error) {
      this.errorMessage.set(getActivityCatalogError(error));
    }
  }

  private buildFolderOptions(): readonly FolderOption[] {
    const invalidFolderIds = this.node.kind === 'folder'
      ? this.descendantFolderIds(this.node.id)
      : new Set<string>();
    invalidFolderIds.add(this.node.id);

    const folders = this.data.nodes.filter(
      (node): node is ActivityFolderNode =>
        node.kind === 'folder' && !invalidFolderIds.has(node.id)
    );
    const nodeById = new Map(this.data.nodes.map((node) => [node.id, node]));

    return [
      { id: null, label: 'Root' },
      ...folders
        .map((folder) => ({ id: folder.id, label: this.pathFor(folder, nodeById) }))
        .sort((left, right) => left.label.localeCompare(right.label))
    ];
  }

  private descendantFolderIds(folderId: string): Set<string> {
    const descendants = new Set<string>();
    const pending = [folderId];
    while (pending.length > 0) {
      const parentId = pending.pop()!;
      for (const node of this.data.nodes) {
        if (
          node.kind === 'folder' &&
          node.parentFolderId === parentId &&
          !descendants.has(node.id)
        ) {
          descendants.add(node.id);
          pending.push(node.id);
        }
      }
    }
    return descendants;
  }

  private pathFor(
    folder: ActivityFolderNode,
    nodeById: ReadonlyMap<string, ActivityCatalogNode>
  ): string {
    const names = [folder.name];
    const visited = new Set([folder.id]);
    let parentId = folder.parentFolderId;
    while (parentId) {
      if (!visited.add(parentId)) break;
      const parent = nodeById.get(parentId);
      if (!parent || parent.kind !== 'folder') break;
      names.unshift(parent.name);
      parentId = parent.parentFolderId;
    }
    return names.join(' / ');
  }

  private initialPrecedingSiblingId(): string | null {
    const siblings = this.data.nodes
      .filter(
        (node) => node.parentFolderId === this.node.parentFolderId && node.id !== this.node.id
      )
      .sort((left, right) => left.sortOrder - right.sortOrder);
    const preceding = [...siblings]
      .reverse()
      .find((sibling) => sibling.sortOrder < this.node.sortOrder);
    return preceding?.id ?? null;
  }
}
