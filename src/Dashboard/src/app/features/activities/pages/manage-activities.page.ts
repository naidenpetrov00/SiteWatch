import {
  ChangeDetectionStrategy,
  Component,
  ViewChild,
  computed,
  effect,
  inject,
  signal
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTree, MatTreeModule } from '@angular/material/tree';
import { firstValueFrom } from 'rxjs';

import {
  CatalogConfirmDialogComponent,
  CatalogConfirmDialogData
} from '../components/catalog-confirm-dialog/catalog-confirm-dialog.component';
import {
  CatalogNodeDialogComponent,
  CatalogNodeDialogData
} from '../components/catalog-node-dialog/catalog-node-dialog.component';
import {
  MoveCatalogNodeDialogComponent,
  MoveCatalogNodeDialogData
} from '../components/move-catalog-node-dialog/move-catalog-node-dialog.component';
import { ActivityRequirementsComponent } from '../components/activity-requirements/activity-requirements.component';
import {
  ActivityCatalogNode,
  ActivityCatalogTreeNode
} from '../models/activity-catalog.models';
import { ActivityCatalogService } from '../services/activity-catalog.service';
import { getActivityCatalogError } from '../utils/activity-catalog-error';

interface Breadcrumb {
  id: string | null;
  label: string;
}

@Component({
  selector: 'app-manage-activities-page',
  imports: [
    MatButtonModule,
    MatDialogModule,
    MatIconModule,
    MatMenuModule,
    MatTreeModule,
    ActivityRequirementsComponent
  ],
  templateUrl: './manage-activities.page.html',
  styleUrl: './manage-activities.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageActivitiesPage {
  private readonly catalogService = inject(ActivityCatalogService);
  private readonly dialog = inject(MatDialog);

  @ViewChild('catalogTree') private catalogTree?: MatTree<
    ActivityCatalogTreeNode,
    string
  >;

  readonly nodes = signal<readonly ActivityCatalogNode[]>([]);
  readonly selectedNodeId = signal<string | null>(null);
  readonly pageError = signal<string | null>(null);
  readonly selectedNode = computed(() =>
    this.nodes().find((node) => node.id === this.selectedNodeId()) ?? null
  );
  readonly treeNodes = computed(() => this.buildTree(this.nodes()));
  readonly breadcrumbs = computed(() => this.buildBreadcrumbs(this.selectedNode()));
  readonly selectedFolderCount = computed(() => {
    const selected = this.selectedNode();
    return selected?.kind === 'folder'
      ? this.nodes().filter(
          (node) => node.parentFolderId === selected.id && node.kind === 'folder'
        ).length
      : 0;
  });
  readonly selectedActivityCount = computed(() => {
    const selected = this.selectedNode();
    return selected?.kind === 'folder'
      ? this.nodes().filter(
          (node) => node.parentFolderId === selected.id && node.kind === 'activity'
        ).length
      : 0;
  });

  readonly childrenAccessor = (node: ActivityCatalogTreeNode) => node.children;
  readonly expansionKey = (node: ActivityCatalogTreeNode) => node.id;
  readonly trackBy = (_index: number, node: ActivityCatalogTreeNode) => node.id;

  constructor() {
    effect(() => {
      const catalog = this.catalogService.catalogQuery.data();
      if (!catalog) return;

      this.nodes.set(catalog);
      const selectedId = this.selectedNodeId();
      if (selectedId && !catalog.some((node) => node.id === selectedId)) {
        this.selectedNodeId.set(null);
      }
    });
  }

  isLoading(): boolean {
    return this.catalogService.catalogQuery.isPending();
  }

  hasLoadError(): boolean {
    return this.catalogService.catalogQuery.isError();
  }

  selectNode(node: ActivityCatalogNode): void {
    this.selectedNodeId.set(node.id);
  }

  selectBreadcrumb(breadcrumb: Breadcrumb): void {
    this.selectedNodeId.set(breadcrumb.id);
  }

  expandAll(): void {
    this.catalogTree?.expandAll();
  }

  collapseAll(): void {
    this.catalogTree?.collapseAll();
  }

  openCreateFolder(parentFolderId: string | null): void {
    this.openEditor({ kind: 'folder', mode: 'create', parentFolderId });
  }

  openRenameFolder(node: ActivityCatalogNode): void {
    if (node.kind !== 'folder') return;
    this.openEditor({
      kind: 'folder',
      mode: 'edit',
      id: node.id,
      parentFolderId: node.parentFolderId,
      name: node.name
    });
  }

  openCreateActivity(parentFolderId: string | null): void {
    this.openEditor({ kind: 'activity', mode: 'create', parentFolderId });
  }

  async openActivity(node: ActivityCatalogNode, mode: 'view' | 'edit'): Promise<void> {
    if (node.kind !== 'activity') return;
    this.pageError.set(null);
    try {
      const activity = await this.catalogService.getActivity(node.id);
      this.openEditor({
        kind: 'activity',
        mode,
        id: node.id,
        parentFolderId: node.parentFolderId,
        activity
      });
    } catch (error) {
      this.pageError.set(
        getActivityCatalogError(error, 'The activity details could not be loaded.')
      );
    }
  }

  openMove(node: ActivityCatalogNode): void {
    this.dialog.open<
      MoveCatalogNodeDialogComponent,
      MoveCatalogNodeDialogData,
      boolean
    >(MoveCatalogNodeDialogComponent, {
      autoFocus: false,
      width: '40rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { node, nodes: this.nodes() }
    });
  }

  async moveQuickly(node: ActivityCatalogNode, direction: -1 | 1): Promise<void> {
    const siblings = this.nodes()
      .filter((sibling) => sibling.parentFolderId === node.parentFolderId)
      .sort((left, right) => left.sortOrder - right.sortOrder);
    const currentIndex = siblings.findIndex((sibling) => sibling.id === node.id);
    const targetIndex = currentIndex + direction;
    if (currentIndex < 0 || targetIndex < 0 || targetIndex >= siblings.length) return;

    this.pageError.set(null);
    try {
      const request = {
        targetParentFolderId: node.parentFolderId,
        targetIndex
      };
      if (node.kind === 'folder') {
        await this.catalogService.moveFolder(node.id, request);
      } else {
        await this.catalogService.moveActivity(node.id, request);
      }
    } catch (error) {
      this.pageError.set(getActivityCatalogError(error));
    }
  }

  canMoveUp(node: ActivityCatalogNode): boolean {
    return node.sortOrder > 0;
  }

  canMoveDown(node: ActivityCatalogNode): boolean {
    const siblingCount = this.nodes().filter(
      (sibling) => sibling.parentFolderId === node.parentFolderId
    ).length;
    return node.sortOrder < siblingCount - 1;
  }

  async toggleArchive(node: ActivityCatalogNode): Promise<void> {
    if (node.kind !== 'activity') return;
    const isArchived = node.status === 'Archived';
    if (!isArchived) {
      const confirmed = await this.confirm({
        title: 'Archive Activity',
        subtitle: 'The activity remains available for historical records.',
        message: `Archive #${node.numberId} — ${node.name}?`,
        confirmLabel: 'Archive'
      });
      if (!confirmed) return;
    }

    this.pageError.set(null);
    try {
      if (isArchived) {
        await this.catalogService.restoreActivity(node.id);
      } else {
        await this.catalogService.archiveActivity(node.id);
      }
    } catch (error) {
      this.pageError.set(getActivityCatalogError(error));
    }
  }

  async deleteNode(node: ActivityCatalogNode): Promise<void> {
    const confirmed = await this.confirm({
      title: node.kind === 'folder' ? 'Delete Folder' : 'Delete Activity',
      subtitle: 'This action cannot be undone.',
      message: `Delete ${node.name}?`,
      confirmLabel: 'Delete'
    });
    if (!confirmed) return;

    this.pageError.set(null);
    try {
      if (node.kind === 'folder') {
        await this.catalogService.deleteFolder(node.id);
      } else {
        await this.catalogService.deleteActivity(node.id);
      }
      this.selectedNodeId.set(node.parentFolderId);
    } catch (error) {
      this.pageError.set(getActivityCatalogError(error));
    }
  }

  private openEditor(data: CatalogNodeDialogData): void {
    this.dialog.open<CatalogNodeDialogComponent, CatalogNodeDialogData, boolean>(
      CatalogNodeDialogComponent,
      {
        autoFocus: false,
        width: '42rem',
        maxWidth: 'calc(100vw - 2rem)',
        data
      }
    );
  }

  private async confirm(data: CatalogConfirmDialogData): Promise<boolean> {
    const dialogRef = this.dialog.open<
      CatalogConfirmDialogComponent,
      CatalogConfirmDialogData,
      boolean
    >(CatalogConfirmDialogComponent, {
      autoFocus: false,
      width: '32rem',
      maxWidth: 'calc(100vw - 2rem)',
      data
    });
    return (await firstValueFrom(dialogRef.afterClosed())) ?? false;
  }

  private buildTree(nodes: readonly ActivityCatalogNode[]): ActivityCatalogTreeNode[] {
    const treeNodes = new Map<string, ActivityCatalogTreeNode>(
      nodes.map((node) => [node.id, { ...node, children: [] }])
    );
    const roots: ActivityCatalogTreeNode[] = [];

    for (const node of treeNodes.values()) {
      const parent = node.parentFolderId
        ? treeNodes.get(node.parentFolderId)
        : undefined;
      if (parent?.kind === 'folder' && parent.id !== node.id) {
        parent.children.push(node);
      } else {
        roots.push(node);
      }
    }

    const sortNodes = (items: ActivityCatalogTreeNode[], visited: Set<string>): void => {
      items.sort((left, right) => left.sortOrder - right.sortOrder);
      for (const item of items) {
        if (!visited.add(item.id)) {
          item.children = [];
          continue;
        }
        sortNodes(item.children, visited);
      }
    };
    sortNodes(roots, new Set());
    return roots;
  }

  private buildBreadcrumbs(selected: ActivityCatalogNode | null): Breadcrumb[] {
    const breadcrumbs: Breadcrumb[] = [{ id: null, label: 'Root' }];
    if (!selected) return breadcrumbs;

    const nodeById = new Map(this.nodes().map((node) => [node.id, node]));
    const path: Breadcrumb[] = [];
    const visited = new Set<string>();
    let current: ActivityCatalogNode | undefined = selected;
    while (current && visited.add(current.id)) {
      path.unshift({ id: current.id, label: current.name });
      current = current.parentFolderId
        ? nodeById.get(current.parentFolderId)
        : undefined;
    }
    return [...breadcrumbs, ...path];
  }
}
