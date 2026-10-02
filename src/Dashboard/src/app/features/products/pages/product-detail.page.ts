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

import { ActionButtonComponent } from '../../../shared/ui/action-button/action-button.component';
import { RetailerListingDialogComponent } from '../../retailer-listings/components/retailer-listing-dialog/retailer-listing-dialog.component';
import { RetailerListingsTableComponent } from '../../retailer-listings/components/retailer-listings-table/retailer-listings-table.component';
import {
  RetailerListing,
  RetailerListingDialogData
} from '../../retailer-listings/models/retailer-listing.models';
import { ProductDialogComponent } from '../components/product-dialog/product-dialog.component';
import { DashboardProductsService } from '../services/dashboard-products.service';

@Component({
  selector: 'app-product-detail-page',
  imports: [
    RouterLink,
    MatButtonModule,
    ActionButtonComponent,
    RetailerListingsTableComponent
  ],
  templateUrl: './product-detail.page.html',
  styleUrl: './catalog-detail.page.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProductDetailPage {
  private readonly productsService = inject(DashboardProductsService);
  private readonly dialog = inject(MatDialog);

  readonly productId = input.required<string>();
  readonly productQuery = injectQuery(() => {
    const productId = this.productId();
    return {
      queryKey: ['products', 'detail', productId] as const,
      queryFn: () => this.productsService.getProductById(productId),
      enabled: productId.length > 0
    };
  });

  async editProduct(): Promise<void> {
    const product = this.productQuery.data();
    if (!product) return;
    const dialogRef = this.dialog.open<ProductDialogComponent, typeof product, boolean>(
      ProductDialogComponent,
      {
        autoFocus: false,
        restoreFocus: true,
        width: '72rem',
        maxWidth: 'calc(100vw - 2rem)',
        data: product
      }
    );
    if (await firstValueFrom(dialogRef.afterClosed())) {
      await this.productQuery.refetch();
    }
  }

  openAddListing(): void {
    const product = this.productQuery.data();
    if (!product || product.status !== 'Active') return;
    this.dialog.open<
      RetailerListingDialogComponent,
      RetailerListingDialogData,
      RetailerListing | null
    >(RetailerListingDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      width: '52rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { listing: null, fixedProduct: product }
    });
  }

  packageLabel(): string {
    const product = this.productQuery.data();
    return product && product.packageQuantity !== null && product.packageUnit
      ? product.packageQuantity + ' ' + product.packageUnit
      : 'Individual item';
  }
}
