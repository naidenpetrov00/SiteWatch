import {
  ChangeDetectionStrategy,
  Component,
  computed,
  DestroyRef,
  inject,
  signal
} from '@angular/core';
import {
  AbstractControl,
  FormControl,
  ReactiveFormsModule,
  ValidationErrors,
  ValidatorFn
} from '@angular/forms';
import {
  MatAutocompleteModule,
  MatAutocompleteSelectedEvent,
  MatAutocompleteTrigger
} from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { firstValueFrom } from 'rxjs';

import { DialogActionBarComponent } from '../../../../shared/ui/dialog-action-bar/dialog-action-bar.component';
import { DialogShellComponent } from '../../../../shared/ui/dialog-shell/dialog-shell.component';
import {
  OfferProductCandidate,
  OfferProductRequirement
} from '../../models/offer.models';
import { OffersService } from '../../services/offers.service';
import { getOfferError } from '../../utils/offer-error';
import { formatOfferQuantity, packageUnitLabel } from '../../utils/offer-quantity';

export interface OfferProductSelectionDialogData {
  siteId: string;
  offerId: string;
  requirement: OfferProductRequirement;
  hasSelectedPrice: boolean;
}

interface ReplaceProductConfirmDialogData {
  currentProduct: string;
  replacementProduct: string;
}

@Component({
  selector: 'app-replace-offer-product-confirm-dialog',
  imports: [MatButtonModule, MatDialogModule],
  template: `
    <h2 mat-dialog-title>Replace Product and Remove Price?</h2>
    <mat-dialog-content>
      <p>
        Replacing {{ data.currentProduct }} with {{ data.replacementProduct }}
        will remove this requirement's selected retailer price.
      </p>
      <p>The replacement Product can be priced independently afterward.</p>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button type="button" [mat-dialog-close]="false">Cancel</button>
      <button mat-flat-button color="warn" type="button" [mat-dialog-close]="true">
        Replace and Remove Price
      </button>
    </mat-dialog-actions>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
class ReplaceOfferProductConfirmDialogComponent {
  readonly data = inject<ReplaceProductConfirmDialogData>(MAT_DIALOG_DATA);
}

@Component({
  selector: 'app-offer-product-selection-dialog',
  imports: [
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    DialogActionBarComponent,
    DialogShellComponent
  ],
  templateUrl: './offer-product-selection-dialog.component.html',
  styleUrl: './offer-product-selection-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OfferProductSelectionDialogComponent {
  readonly data = inject<OfferProductSelectionDialogData>(MAT_DIALOG_DATA);
  private readonly dialogRef = inject(
    MatDialogRef<OfferProductSelectionDialogComponent, boolean>
  );
  private readonly dialog = inject(MatDialog);
  private readonly offersService = inject(OffersService);
  private readonly destroyRef = inject(DestroyRef);
  private searchTimer: ReturnType<typeof setTimeout> | null = null;
  private searchRevision = 0;

  readonly formId = 'offer-product-selection-form';
  readonly results = signal<readonly OfferProductCandidate[]>([]);
  readonly loading = signal(false);
  readonly hasLoaded = signal(false);
  readonly lookupError = signal<string | null>(null);
  readonly mutationError = signal<string | null>(null);
  readonly submitting = signal(false);
  readonly currentProduct = this.toCandidate(this.data.requirement);
  readonly productControl = new FormControl<
    string | OfferProductCandidate | null
  >(this.currentProduct, { validators: [catalogSelection()] });
  readonly title = computed(() =>
    this.data.requirement.productLine ? 'Replace Exact Product' : 'Select Exact Product'
  );
  readonly submitLabel = computed(() => {
    if (this.submitting()) return 'Saving…';
    return this.data.requirement.productLine ? 'Save Product' : 'Select Product';
  });
  readonly displayProduct = (
    value: string | OfferProductCandidate | null
  ): string => {
    if (typeof value === 'string' || value === null) return value ?? '';
    return `#${value.numberId} · ${value.title}`;
  };

  constructor() {
    this.dialogRef.disableClose = true;
    this.destroyRef.onDestroy(() => {
      this.clearSearchTimer();
      this.searchRevision += 1;
    });
  }

  onInput(value: string, trigger: MatAutocompleteTrigger): void {
    this.clearSearchTimer();
    this.searchRevision += 1;
    this.results.set([]);
    this.lookupError.set(null);
    this.hasLoaded.set(false);
    this.productControl.setValue(value, { emitEvent: false });
    this.productControl.markAsDirty();
    this.searchTimer = setTimeout(() => {
      void this.loadCandidates(value, trigger);
    }, 250);
  }

  onFocus(trigger: MatAutocompleteTrigger): void {
    if (!this.loading() && !this.hasLoaded()) {
      void this.loadCandidates('', trigger);
    }
  }

  onOptionSelected(event: MatAutocompleteSelectedEvent): void {
    this.clearSearchTimer();
    this.searchRevision += 1;
    this.productControl.setValue(event.option.value as OfferProductCandidate, {
      emitEvent: false
    });
    this.productControl.markAsDirty();
    this.productControl.updateValueAndValidity();
  }

  retry(trigger: MatAutocompleteTrigger): void {
    const value = this.productControl.value;
    void this.loadCandidates(typeof value === 'string' ? value : '', trigger);
  }

  productIdentity(product: OfferProductCandidate): string {
    return [product.brand, product.model].filter(Boolean).join(' · ');
  }

  packageDescription(product: OfferProductCandidate): string {
    if (product.packageQuantity === null || product.packageUnit === null) {
      return 'Individual item';
    }
    return `${formatOfferQuantity(product.packageQuantity)} ${packageUnitLabel(product.packageUnit)}`;
  }

  close(): void {
    if (!this.submitting()) this.dialogRef.close(false);
  }

  async submit(): Promise<void> {
    if (this.productControl.invalid || this.submitting()) {
      this.productControl.markAsTouched();
      return;
    }

    const product = this.productControl.value as OfferProductCandidate;
    const current = this.data.requirement.productLine;
    if (
      current &&
      current.productId !== product.id &&
      this.data.hasSelectedPrice &&
      !(await this.confirmPriceRemoval(current, product))
    ) {
      return;
    }

    this.mutationError.set(null);
    this.submitting.set(true);
    try {
      await this.offersService.selectRequirementProduct({
        siteId: this.data.siteId,
        offerId: this.data.offerId,
        offerRequirementId: this.data.requirement.id,
        productId: product.id
      });
      this.dialogRef.close(true);
    } catch (error) {
      this.mutationError.set(
        getOfferError(error, 'The Product could not be selected for this requirement.')
      );
    } finally {
      this.submitting.set(false);
    }
  }

  private async loadCandidates(
    searchTerm: string,
    trigger?: MatAutocompleteTrigger
  ): Promise<void> {
    this.clearSearchTimer();
    const revision = ++this.searchRevision;
    this.results.set([]);
    this.lookupError.set(null);
    this.loading.set(true);
    this.hasLoaded.set(true);
    trigger?.openPanel();
    try {
      const candidates = await this.offersService.getRequirementProductCandidates(
        this.data.siteId,
        this.data.offerId,
        this.data.requirement.id,
        searchTerm
      );
      if (revision !== this.searchRevision) return;
      this.results.set(candidates);
      trigger?.openPanel();
    } catch {
      if (revision !== this.searchRevision) return;
      this.lookupError.set('Product candidates could not be loaded. Try again.');
    } finally {
      if (revision === this.searchRevision) this.loading.set(false);
    }
  }

  private async confirmPriceRemoval(
    current: NonNullable<OfferProductRequirement['productLine']>,
    replacement: OfferProductCandidate
  ): Promise<boolean> {
    const dialogRef = this.dialog.open<
      ReplaceOfferProductConfirmDialogComponent,
      ReplaceProductConfirmDialogData,
      boolean
    >(ReplaceOfferProductConfirmDialogComponent, {
      autoFocus: 'dialog',
      restoreFocus: true,
      width: '34rem',
      maxWidth: 'calc(100vw - 2rem)',
      ariaLabel: 'Confirm Product replacement and selected price removal',
      data: {
        currentProduct: `#${current.productNumberId} · ${current.title}`,
        replacementProduct: `#${replacement.numberId} · ${replacement.title}`
      }
    });
    return (await firstValueFrom(dialogRef.afterClosed())) === true;
  }

  private toCandidate(
    requirement: OfferProductRequirement
  ): OfferProductCandidate | null {
    const line = requirement.productLine;
    if (!line) return null;
    return {
      id: line.productId,
      numberId: line.productNumberId,
      title: line.title,
      productTypeId: line.productTypeId,
      productTypeName: line.productTypeName,
      categoryId: line.categoryId,
      categoryName: line.categoryName,
      brand: line.brand,
      model: line.model,
      packageQuantity: line.packageQuantity,
      packageUnit: line.packageUnit
    };
  }

  private clearSearchTimer(): void {
    if (this.searchTimer !== null) {
      clearTimeout(this.searchTimer);
      this.searchTimer = null;
    }
  }
}

function catalogSelection(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const value = control.value;
    return value && typeof value === 'object' && 'id' in value
      ? null
      : { requireSelection: true };
  };
}
