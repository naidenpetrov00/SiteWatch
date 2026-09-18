import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ViewChild,
  computed,
  effect,
  inject,
  input,
  output,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatTree, MatTreeModule } from '@angular/material/tree';
import { debounceTime, distinctUntilChanged, firstValueFrom } from 'rxjs';

import {
  AddOfferActivityDialogComponent,
  AddOfferActivityDialogData
} from '../add-offer-activity-dialog/add-offer-activity-dialog.component';
import {
  OfferActivityCatalogNode,
  OfferActivityCatalogTreeNode
} from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { getOfferError } from '../../utils/offer-error';

@Component({
  selector: 'app-offer-activity-browser',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatTreeModule
  ],
  templateUrl: './offer-activity-browser.component.html',
  styleUrl: './offer-activity-browser.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferActivityBrowserComponent {
  private readonly offersService = inject(OffersService);
  private readonly dialog = inject(MatDialog);
  private readonly destroyRef = inject(DestroyRef);
  private readonly formBuilder = inject(FormBuilder);

  @ViewChild('offerCatalogTree') private catalogTree?: MatTree<
    OfferActivityCatalogTreeNode,
    string
  >;

  readonly siteId = input.required<string>();
  readonly offerId = input.required<string>();
  readonly activityAdded = output<void>();
  readonly searchControl = this.formBuilder.nonNullable.control('', [
    Validators.maxLength(200)
  ]);
  readonly appliedSearch = signal('');
  readonly loadingActivityId = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly nodes = computed(
    () => this.offersService.activityCatalogQuery.data() ?? []
  );
  readonly treeNodes = computed(() => this.buildTree(this.nodes()));
  readonly childrenAccessor = (node: OfferActivityCatalogTreeNode) => node.children;
  readonly expansionKey = (node: OfferActivityCatalogTreeNode) => node.id;
  readonly trackBy = (_index: number, node: OfferActivityCatalogTreeNode) =>
    node.id;

  constructor() {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(300),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((value) => this.appliedSearch.set(value.trim()));

    effect(() =>
      this.offersService.configureActivityCatalog(
        this.siteId(),
        this.offerId(),
        this.appliedSearch()
      )
    );
  }

  isLoading(): boolean {
    return this.offersService.activityCatalogQuery.isPending();
  }

  hasLoadError(): boolean {
    return this.offersService.activityCatalogQuery.isError();
  }

  expandAll(): void {
    this.catalogTree?.expandAll();
  }

  collapseAll(): void {
    this.catalogTree?.collapseAll();
  }

  clearSearch(): void {
    this.searchControl.setValue('');
  }

  canAdd(node: OfferActivityCatalogNode): boolean {
    return (
      node.kind === 'activity' &&
      node.status === 'Active' &&
      !node.isSelected &&
      this.loadingActivityId() === null
    );
  }

  async openAddDialog(node: OfferActivityCatalogNode): Promise<void> {
    if (!this.canAdd(node)) return;

    this.errorMessage.set(null);
    this.loadingActivityId.set(node.id);
    try {
      const candidate = await this.offersService.getActivityCandidate(
        this.siteId(),
        this.offerId(),
        node.id
      );
      const dialogRef = this.dialog.open<
        AddOfferActivityDialogComponent,
        AddOfferActivityDialogData,
        boolean
      >(AddOfferActivityDialogComponent, {
        autoFocus: false,
        width: '46rem',
        maxWidth: 'calc(100vw - 2rem)',
        data: {
          siteId: this.siteId(),
          offerId: this.offerId(),
          candidate
        }
      });
      if (await firstValueFrom(dialogRef.afterClosed())) {
        this.activityAdded.emit();
      }
    } catch (error) {
      this.errorMessage.set(
        getOfferError(error, 'The activity could not be prepared for this Offer.')
      );
    } finally {
      this.loadingActivityId.set(null);
    }
  }

  private buildTree(
    nodes: readonly OfferActivityCatalogNode[]
  ): OfferActivityCatalogTreeNode[] {
    const treeNodes = new Map<string, OfferActivityCatalogTreeNode>(
      nodes.map((node) => [node.id, { ...node, children: [] }])
    );
    const roots: OfferActivityCatalogTreeNode[] = [];

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

    const sortNodes = (
      items: OfferActivityCatalogTreeNode[],
      visited: Set<string>
    ): void => {
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
}
