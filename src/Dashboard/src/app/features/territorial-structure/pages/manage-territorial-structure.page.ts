import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';

import { CatalogConfirmDialogComponent, CatalogConfirmDialogData } from '../../activities/components/catalog-confirm-dialog/catalog-confirm-dialog.component';
import { TerritorialEntry, TerritorialEntryKind } from '../../../shared/territorial-structure/territorial-structure.models';
import { TerritorialStructureService } from '../../../shared/territorial-structure/territorial-structure.service';

type NodeKind = TerritorialEntryKind | 'country';
interface TerritorialTreeNode extends TerritorialEntry { readonly kind: NodeKind; readonly children: readonly TerritorialTreeNode[]; }
interface EditorState { readonly mode: 'create' | 'rename'; readonly kind: TerritorialEntryKind; readonly parentId: string | null; readonly node?: TerritorialTreeNode; }

@Component({
  selector: 'app-manage-territorial-structure-page',
  imports: [NgTemplateOutlet, ReactiveFormsModule, MatButtonModule, MatDialogModule, MatFormFieldModule, MatIconModule, MatInputModule],
  templateUrl: './manage-territorial-structure.page.html',
  styleUrl: './manage-territorial-structure.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ManageTerritorialStructurePage implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly service = inject(TerritorialStructureService);
  private readonly dialog = inject(MatDialog);

  readonly root = signal<TerritorialTreeNode | null>(null);
  readonly selectedNodeId = signal<string>('bulgaria');
  readonly expandedNodeIds = signal<ReadonlySet<string>>(new Set(['bulgaria']));
  readonly pageError = signal<string | null>(null);
  readonly editor = signal<EditorState | null>(null);
  readonly selectedNode = computed(() => this.findNode(this.root(), this.selectedNodeId()));
  readonly entryForm = this.formBuilder.nonNullable.group({ name: ['', [Validators.required, Validators.maxLength(100)]] });

  async ngOnInit(): Promise<void> { await this.reload(); }

  selectNode(node: TerritorialTreeNode): void { this.selectedNodeId.set(node.id); this.pageError.set(null); }
  toggleExpanded(node: TerritorialTreeNode): void {
    if (!node.children.length) return;
    this.expandedNodeIds.update((expanded) => { const next = new Set(expanded); if (next.has(node.id)) next.delete(node.id); else next.add(node.id); return next; });
  }
  isExpanded(node: TerritorialTreeNode): boolean { return this.expandedNodeIds().has(node.id); }
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
      if (editor.mode === 'create') {
        await this.service.create({ kind: editor.kind, parentId: editor.parentId, name: this.entryForm.controls.name.value });
        this.expandParent(editor.parentId);
      } else if (editor.node) {
        await this.service.update(editor.node.id, { kind: editor.kind, name: this.entryForm.controls.name.value, isActive: editor.node.isActive });
      }
      this.cancelEditor(); await this.reload();
    } catch (error) { this.pageError.set(this.errorMessage(error, 'The item could not be saved. Its name must be unique at this level.')); }
  }

  async toggleActive(node: TerritorialTreeNode): Promise<void> {
    if (node.kind === 'country') return;
    this.pageError.set(null);
    try {
      await this.service.update(node.id, { kind: node.kind, name: node.name, isActive: !node.isActive });
      await this.reload();
    } catch (error) { this.pageError.set(this.errorMessage(error, 'The item could not be updated.')); }
  }

  async deleteNode(node: TerritorialTreeNode): Promise<void> {
    if (node.kind === 'country') return;
    const confirmed = await this.confirm({ title: `Delete ${this.typeLabel(node.kind)}`, subtitle: 'This action cannot be undone.', message: `Delete ${node.name}? This is available only when it has no child entries and is not used by an address.`, confirmLabel: 'Delete' });
    if (!confirmed) return;
    this.pageError.set(null);
    try {
      await this.service.delete(node.id, node.kind);
      this.selectedNodeId.set(node.parentId ?? 'bulgaria'); this.cancelEditor(); await this.reload();
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

  private async reload(): Promise<void> {
    this.pageError.set(null);
    try {
      const municipalities = await this.service.getEntries(0, null, '', true);
      const municipalityNodes = await Promise.all(municipalities.map(async (municipality) => {
        const settlements = await this.service.getEntries(1, municipality.id, '', true);
        const settlementNodes = await Promise.all(settlements.map(async (settlement) => {
          const [districts, quarters] = await Promise.all([this.service.getEntries(2, settlement.id, '', true), this.service.getEntries(3, settlement.id, '', true)]);
          return this.node(settlement, 1, [...districts.map((district) => this.node(district, 2, [])), ...quarters.map((quarter) => this.node(quarter, 3, []))]);
        }));
        return this.node(municipality, 0, settlementNodes);
      }));
      this.root.set({ id: 'bulgaria', name: 'Bulgaria / България', isActive: true, parentId: null, kind: 'country', children: municipalityNodes });
      if (!this.selectedNode()) this.selectedNodeId.set('bulgaria');
    } catch (error) { this.pageError.set(this.errorMessage(error, 'The territorial structure could not be loaded.')); }
  }

  private node(entry: TerritorialEntry, kind: TerritorialEntryKind, children: readonly TerritorialTreeNode[]): TerritorialTreeNode { return { ...entry, kind, children }; }
  private findNode(node: TerritorialTreeNode | null, id: string): TerritorialTreeNode | null {
    if (!node) return null; if (node.id === id) return node;
    for (const child of node.children) { const found = this.findNode(child, id); if (found) return found; }
    return null;
  }
  private expandParent(parentId: string | null): void { if (parentId) this.expandedNodeIds.update((expanded) => new Set(expanded).add(parentId)); }
  private async confirm(data: CatalogConfirmDialogData): Promise<boolean> {
    const dialogRef = this.dialog.open<CatalogConfirmDialogComponent, CatalogConfirmDialogData, boolean>(CatalogConfirmDialogComponent, { autoFocus: false, width: '32rem', maxWidth: 'calc(100vw - 2rem)', data });
    return (await firstValueFrom(dialogRef.afterClosed())) ?? false;
  }
  private errorMessage(error: unknown, fallback: string): string {
    const detail = (error as { error?: { detail?: unknown } })?.error?.detail;
    return typeof detail === 'string' && detail.trim() ? detail : fallback;
  }
}
