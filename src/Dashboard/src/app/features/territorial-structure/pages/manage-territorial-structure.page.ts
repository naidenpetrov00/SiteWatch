import { ChangeDetectionStrategy, Component, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTree, MatTreeModule } from '@angular/material/tree';
import { firstValueFrom } from 'rxjs';

import { CatalogConfirmDialogComponent, CatalogConfirmDialogData } from '../../activities/components/catalog-confirm-dialog/catalog-confirm-dialog.component';
import { TerritorialEntry, TerritorialEntryKind } from '../../../shared/territorial-structure/territorial-structure.models';
import { TerritorialStructureService } from '../../../shared/territorial-structure/territorial-structure.service';

type NodeKind = TerritorialEntryKind | 'country';

interface TerritorialTreeNode extends TerritorialEntry {
  readonly kind: NodeKind;
  readonly children: TerritorialTreeNode[];
  readonly childrenLoaded: boolean;
  readonly childrenLoading: boolean;
}

interface EditorState {
  readonly mode: 'create' | 'rename';
  readonly kind: TerritorialEntryKind;
  readonly parentId: string | null;
  readonly node?: TerritorialTreeNode;
}

@Component({
  selector: 'app-manage-territorial-structure-page',
  imports: [ReactiveFormsModule, MatButtonModule, MatDialogModule, MatFormFieldModule, MatIconModule, MatInputModule, MatTreeModule],
  templateUrl: './manage-territorial-structure.page.html',
  styleUrl: './manage-territorial-structure.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageTerritorialStructurePage implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly service = inject(TerritorialStructureService);
  private readonly dialog = inject(MatDialog);
  private readonly branchRequestVersions = new Map<string, number>();

  @ViewChild('territorialTree') private territorialTree?: MatTree<TerritorialTreeNode, string>;

  readonly treeNodes = signal<TerritorialTreeNode[]>([this.countryNode()]);
  readonly selectedNodeId = signal<string>('bulgaria');
  readonly pageError = signal<string | null>(null);
  readonly editor = signal<EditorState | null>(null);
  readonly selectedNode = computed(() => this.findNode(this.treeNodes(), this.selectedNodeId()));
  readonly entryForm = this.formBuilder.nonNullable.group({ name: ['', [Validators.required, Validators.maxLength(100)]] });
  readonly childrenAccessor = (node: TerritorialTreeNode) => node.children;
  readonly expansionKey = (node: TerritorialTreeNode) => node.id;
  readonly trackBy = (_index: number, node: TerritorialTreeNode) => node.id;

  async ngOnInit(): Promise<void> {
    await this.refreshChildren('bulgaria', true);
  }

  selectNode(node: TerritorialTreeNode): void { this.selectedNodeId.set(node.id); this.pageError.set(null); }

  canExpand(node: TerritorialTreeNode): boolean {
    return this.canHaveChildren(node.kind) && (!node.childrenLoaded || node.children.length > 0);
  }

  isExpanded(node: TerritorialTreeNode): boolean { return this.territorialTree?.isExpanded(node) ?? false; }
  expansionChanged(node: TerritorialTreeNode, expanded: boolean): void {
    if (expanded && !node.childrenLoaded && !node.childrenLoading) void this.refreshChildren(node.id);
  }
  openCreate(kind: TerritorialEntryKind, parentId: string | null): void { this.pageError.set(null); this.entryForm.reset(); this.editor.set({ mode: 'create', kind, parentId }); }
  openRename(node: TerritorialTreeNode): void {
    if (node.kind === 'country') return;
    this.pageError.set(null); this.entryForm.setValue({ name: node.name }); this.editor.set({ mode: 'rename', kind: node.kind, parentId: node.parentId, node });
  }
  cancelEditor(): void { this.entryForm.reset(); this.editor.set(null); }

  async save(): Promise<void> {
    const editor = this.editor();
    if (!editor) return;
    if (this.entryForm.invalid) { this.entryForm.markAllAsTouched(); return; }
    this.pageError.set(null);
    try {
      const name = this.entryForm.controls.name.value;
      if (editor.mode === 'create') {
        await this.service.create({ kind: editor.kind, parentId: editor.parentId, name });
        this.cancelEditor();
        await this.refreshChildren(editor.parentId ?? 'bulgaria', true);
      } else if (editor.node) {
        await this.service.update(editor.node.id, { kind: editor.kind, name, isActive: editor.node.isActive });
        this.updateNode(editor.node.id, (node) => ({ ...node, name }));
        this.cancelEditor();
      }
    } catch (error) { this.pageError.set(this.errorMessage(error, 'The item could not be saved. Its name must be unique at this level.')); }
  }

  async toggleActive(node: TerritorialTreeNode): Promise<void> {
    if (node.kind === 'country') return;
    this.pageError.set(null);
    try {
      const isActive = !node.isActive;
      await this.service.update(node.id, { kind: node.kind, name: node.name, isActive });
      this.updateNode(node.id, (current) => ({ ...current, isActive }));
    } catch (error) { this.pageError.set(this.errorMessage(error, 'The item could not be updated.')); }
  }

  async deleteNode(node: TerritorialTreeNode): Promise<void> {
    if (node.kind === 'country') return;
    const confirmed = await this.confirm({ title: `Delete ${this.typeLabel(node.kind)}`, subtitle: 'This action cannot be undone.', message: `Delete ${node.name}? This is available only when it has no child entries and is not used by an address.`, confirmLabel: 'Delete' });
    if (!confirmed) return;
    this.pageError.set(null);
    try {
      await this.service.delete(node.id, node.kind);
      const parentId = node.parentId ?? 'bulgaria';
      this.removeNode(node.id);
      this.selectedNodeId.set(parentId);
      this.cancelEditor();
    } catch (error) { this.pageError.set(this.errorMessage(error, 'The item could not be deleted. Deactivate it if it is still in use.')); }
  }

  typeLabel(kind: NodeKind): string {
    switch (kind) { case 'country': return 'Country / Държава'; case 0: return 'Municipality / Община'; case 1: return 'Settlement / Населено място'; case 2: return 'District / Район'; case 3: return 'Quarter / Квартал'; }
  }
  editorTitle(): string { const editor = this.editor(); return editor ? `${editor.mode === 'create' ? 'Add' : 'Rename'} ${this.typeLabel(editor.kind)}` : ''; }
  childActions(node: TerritorialTreeNode): readonly { label: string; kind: TerritorialEntryKind }[] {
    switch (node.kind) {
      case 'country': return [{ label: 'Add Municipality / Община', kind: 0 }];
      case 0: return [{ label: 'Add Settlement / Населено място', kind: 1 }];
      case 1: return [{ label: 'Add District / Район', kind: 2 }, { label: 'Add Quarter / Квартал', kind: 3 }];
      default: return [];
    }
  }

  private async refreshChildren(nodeId: string, expandAfterLoad = false): Promise<void> {
    const node = this.findNode(this.treeNodes(), nodeId);
    if (!node || !this.canHaveChildren(node.kind)) return;
    const requestVersion = (this.branchRequestVersions.get(nodeId) ?? 0) + 1;
    this.branchRequestVersions.set(nodeId, requestVersion);
    this.updateNode(nodeId, (current) => ({ ...current, childrenLoading: true }));
    this.pageError.set(null);

    try {
      const entries = await this.getChildEntries(node);
      if (this.branchRequestVersions.get(nodeId) !== requestVersion) return;
      const current = this.findNode(this.treeNodes(), nodeId);
      if (!current) return;
      const existing = new Map(current.children.map((child) => [`${child.kind}:${child.id}`, child]));
      const children = entries.map(({ entry, kind }) => this.node(entry, kind, existing.get(`${kind}:${entry.id}`)));
      this.updateNode(nodeId, (item) => ({ ...item, children, childrenLoaded: true, childrenLoading: false }));
      if (expandAfterLoad) {
        const refreshed = this.findNode(this.treeNodes(), nodeId);
        if (refreshed?.children.length) this.territorialTree?.expand(refreshed);
      }
    } catch (error) {
      if (this.branchRequestVersions.get(nodeId) !== requestVersion) return;
      this.updateNode(nodeId, (current) => ({ ...current, childrenLoading: false }));
      this.pageError.set(this.errorMessage(error, 'The territorial structure could not be loaded.'));
    }
  }

  private async getChildEntries(node: TerritorialTreeNode): Promise<readonly { entry: TerritorialEntry; kind: TerritorialEntryKind }[]> {
    if (node.kind === 'country') return (await this.service.getEntries(0, null, '', true)).map((entry) => ({ entry, kind: 0 as const }));
    if (node.kind === 0) return (await this.service.getEntries(1, node.id, '', true)).map((entry) => ({ entry, kind: 1 as const }));
    if (node.kind === 1) {
      const [districts, quarters] = await Promise.all([this.service.getEntries(2, node.id, '', true), this.service.getEntries(3, node.id, '', true)]);
      return [...districts.map((entry) => ({ entry, kind: 2 as const })), ...quarters.map((entry) => ({ entry, kind: 3 as const }))];
    }
    return [];
  }

  private countryNode(): TerritorialTreeNode {
    return { id: 'bulgaria', name: 'Bulgaria / България', isActive: true, parentId: null, kind: 'country', children: [], childrenLoaded: false, childrenLoading: false };
  }

  private node(entry: TerritorialEntry, kind: TerritorialEntryKind, existing?: TerritorialTreeNode): TerritorialTreeNode {
    return { ...entry, kind, children: existing?.children ?? [], childrenLoaded: existing?.childrenLoaded ?? false, childrenLoading: false };
  }

  private canHaveChildren(kind: NodeKind): boolean { return kind === 'country' || kind === 0 || kind === 1; }

  private findNode(nodes: readonly TerritorialTreeNode[], id: string): TerritorialTreeNode | null {
    for (const node of nodes) {
      if (node.id === id) return node;
      const found = this.findNode(node.children, id);
      if (found) return found;
    }
    return null;
  }

  private updateNode(id: string, update: (node: TerritorialTreeNode) => TerritorialTreeNode): void {
    this.treeNodes.update((nodes) => this.updateNodes(nodes, id, update));
  }

  private updateNodes(nodes: readonly TerritorialTreeNode[], id: string, update: (node: TerritorialTreeNode) => TerritorialTreeNode): TerritorialTreeNode[] {
    return nodes.map((node) => node.id === id ? update(node) : { ...node, children: this.updateNodes(node.children, id, update) });
  }

  private removeNode(id: string): void {
    this.treeNodes.update((nodes) => this.removeFromNodes(nodes, id));
  }

  private removeFromNodes(nodes: readonly TerritorialTreeNode[], id: string): TerritorialTreeNode[] {
    return nodes.filter((node) => node.id !== id).map((node) => ({ ...node, children: this.removeFromNodes(node.children, id) }));
  }

  private async confirm(data: CatalogConfirmDialogData): Promise<boolean> {
    const dialogRef = this.dialog.open<CatalogConfirmDialogComponent, CatalogConfirmDialogData, boolean>(CatalogConfirmDialogComponent, { autoFocus: false, width: '32rem', maxWidth: 'calc(100vw - 2rem)', data });
    return (await firstValueFrom(dialogRef.afterClosed())) ?? false;
  }

  private errorMessage(error: unknown, fallback: string): string {
    const detail = (error as { error?: { detail?: unknown } })?.error?.detail;
    return typeof detail === 'string' && detail.trim() ? detail : fallback;
  }
}
