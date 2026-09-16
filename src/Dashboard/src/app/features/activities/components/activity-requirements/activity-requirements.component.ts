import {
  ChangeDetectionStrategy,
  Component,
  inject,
  input,
  signal
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import {
  CatalogConfirmDialogComponent,
  CatalogConfirmDialogData
} from '../catalog-confirm-dialog/catalog-confirm-dialog.component';
import {
  ProductRequirementDialogComponent,
  ProductRequirementDialogData
} from '../product-requirement-dialog/product-requirement-dialog.component';
import {
  RequirementSectionDialogComponent,
  RequirementSectionDialogData
} from '../requirement-section-dialog/requirement-section-dialog.component';
import {
  ACTIVITY_MEASUREMENT_UNIT_OPTIONS,
  ActivityDetails,
  ActivityProductRequirement,
  ActivityRequirementSection
} from '../../models/activity-catalog.models';
import { PRODUCT_PACKAGE_UNIT_OPTIONS } from '../../../products/models/dashboard-product.models';
import { ActivityCatalogService } from '../../services/activity-catalog.service';
import { getActivityCatalogError } from '../../utils/activity-catalog-error';

@Component({
  selector: 'app-activity-requirements',
  imports: [MatButtonModule, MatDialogModule, MatIconModule],
  templateUrl: './activity-requirements.component.html',
  styleUrl: './activity-requirements.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActivityRequirementsComponent {
  private readonly catalogService = inject(ActivityCatalogService);
  private readonly dialog = inject(MatDialog);

  readonly selectedActivityId = input<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly detailsQuery = injectQuery<ActivityDetails>(() => {
    const activityId = this.selectedActivityId();
    return {
      queryKey: ['activity-catalog', 'activity-details', activityId] as const,
      enabled: Boolean(activityId),
      queryFn: () => {
        if (!activityId) throw new Error('An activity ID is required.');
        return this.catalogService.getActivity(activityId);
      }
    };
  });

  openSectionDialog(
    activity: ActivityDetails,
    section?: ActivityRequirementSection
  ): void {
    this.dialog.open<
      RequirementSectionDialogComponent,
      RequirementSectionDialogData,
      boolean
    >(RequirementSectionDialogComponent, {
      autoFocus: false,
      width: '42rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: { activityId: activity.id, section }
    });
  }

  openProductDialog(
    activity: ActivityDetails,
    section: ActivityRequirementSection,
    requirement?: ActivityProductRequirement
  ): void {
    this.dialog.open<
      ProductRequirementDialogComponent,
      ProductRequirementDialogData,
      boolean
    >(ProductRequirementDialogComponent, {
      autoFocus: false,
      width: '46rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: {
        activityId: activity.id,
        sectionId: section.id,
        excludedProductIds: section.productRequirements.map(
          (current) => current.productId
        ),
        requirement
      }
    });
  }

  async moveSection(
    activity: ActivityDetails,
    section: ActivityRequirementSection,
    direction: -1 | 1
  ): Promise<void> {
    const currentIndex = activity.requirementSections.findIndex(
      (current) => current.id === section.id
    );
    const targetIndex = currentIndex + direction;
    if (
      currentIndex < 0 ||
      targetIndex < 0 ||
      targetIndex >= activity.requirementSections.length
    ) {
      return;
    }

    await this.runMutation(() =>
      this.catalogService.moveRequirementSection(activity.id, section.id, {
        targetIndex
      })
    );
  }

  async moveProduct(
    activity: ActivityDetails,
    section: ActivityRequirementSection,
    requirement: ActivityProductRequirement,
    direction: -1 | 1
  ): Promise<void> {
    const currentIndex = section.productRequirements.findIndex(
      (current) => current.id === requirement.id
    );
    const targetIndex = currentIndex + direction;
    if (
      currentIndex < 0 ||
      targetIndex < 0 ||
      targetIndex >= section.productRequirements.length
    ) {
      return;
    }

    await this.runMutation(() =>
      this.catalogService.moveProductRequirement(
        activity.id,
        section.id,
        requirement.id,
        { targetIndex }
      )
    );
  }

  async deleteSection(
    activity: ActivityDetails,
    section: ActivityRequirementSection
  ): Promise<void> {
    const count = section.productRequirements.length;
    const confirmed = await this.confirm({
      title: 'Delete Requirement Section',
      subtitle: `${count} product ${count === 1 ? 'relationship' : 'relationships'} will be removed.`,
      message: `Delete ${this.sectionHeading(section)}? Product Catalog entries will not be deleted.`,
      confirmLabel: 'Delete Section'
    });
    if (!confirmed) return;

    await this.runMutation(() =>
      this.catalogService.deleteRequirementSection(activity.id, section.id)
    );
  }

  async deleteProduct(
    activity: ActivityDetails,
    section: ActivityRequirementSection,
    requirement: ActivityProductRequirement
  ): Promise<void> {
    const confirmed = await this.confirm({
      title: 'Remove Product Requirement',
      subtitle: 'The Product Catalog entry will be retained.',
      message: `Remove #${requirement.productNumberId} — ${requirement.productTitle} from this section?`,
      confirmLabel: 'Remove Product'
    });
    if (!confirmed) return;

    await this.runMutation(() =>
      this.catalogService.deleteProductRequirement(
        activity.id,
        section.id,
        requirement.id
      )
    );
  }

  sectionHeading(section: ActivityRequirementSection): string {
    return `For ${section.basisQuantity} ${this.measurementSymbol(
      section.measurementUnit,
      section.basisQuantity
    )}`;
  }

  productIdentity(requirement: ActivityProductRequirement): string {
    return [requirement.brand, requirement.model].filter(Boolean).join(' · ');
  }

  packageDescription(requirement: ActivityProductRequirement): string {
    if (
      requirement.packageQuantity === null ||
      requirement.packageUnit === null
    ) {
      return 'Package not specified';
    }
    const unit = PRODUCT_PACKAGE_UNIT_OPTIONS.find(
      (option) => option.value === requirement.packageUnit
    );
    return `${requirement.packageQuantity} ${unit?.label ?? requirement.packageUnit}`;
  }

  private measurementSymbol(unitCode: string, quantity: number): string {
    if (unitCode === 'piece') return quantity === 1 ? 'piece' : 'pieces';
    return (
      ACTIVITY_MEASUREMENT_UNIT_OPTIONS.find(
        (option) => option.value === unitCode
      )?.symbol ?? unitCode
    );
  }

  private async runMutation(mutation: () => Promise<unknown>): Promise<void> {
    this.errorMessage.set(null);
    try {
      await mutation();
    } catch (error) {
      this.errorMessage.set(getActivityCatalogError(error));
    }
  }

  private async confirm(data: CatalogConfirmDialogData): Promise<boolean> {
    const dialogRef = this.dialog.open<
      CatalogConfirmDialogComponent,
      CatalogConfirmDialogData,
      boolean
    >(CatalogConfirmDialogComponent, {
      autoFocus: false,
      width: '34rem',
      maxWidth: 'calc(100vw - 2rem)',
      data
    });
    return (await firstValueFrom(dialogRef.afterClosed())) ?? false;
  }
}
