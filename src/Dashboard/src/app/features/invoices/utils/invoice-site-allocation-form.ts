import {
  AbstractControl,
  FormArray,
  FormBuilder,
  FormControl,
  FormGroup,
  ValidationErrors,
  ValidatorFn,
  Validators
} from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';

import { DashboardSiteLookup } from '../../sites/models/dashboard-site-lookup.model';
import {
  DashboardInvoiceSiteAllocation,
  InvoiceSiteAllocationRequest,
  SitePaymentDirection
} from '../models/invoice-site-allocation.model';
import {
  decimalValidator,
  positiveDecimalValidator,
  uuidValidator
} from '../components/add-invoice-dialog/add-invoice-dialog.validators';

export interface InvoiceSiteAllocationFormControls {
  siteId: FormControl<string>;
  siteSearch: FormControl<string | DashboardSiteLookup | null>;
  amount: FormControl<number | null>;
  direction: FormControl<SitePaymentDirection>;
}

export type InvoiceSiteAllocationFormGroup = FormGroup<InvoiceSiteAllocationFormControls>;
export type InvoiceSiteAllocationsFormArray = FormArray<InvoiceSiteAllocationFormGroup>;

export function createInvoiceSiteAllocationForm(
  formBuilder: FormBuilder,
  allocation?: DashboardInvoiceSiteAllocation
): InvoiceSiteAllocationFormGroup {
  const selectedSite: DashboardSiteLookup | null = allocation
    ? {
        id: allocation.siteId,
        numberId: allocation.siteNumberId,
        name: allocation.siteName,
        address: ''
      }
    : null;

  return formBuilder.group({
    siteId: formBuilder.nonNullable.control(allocation?.siteId ?? '', {
      validators: [Validators.required, uuidValidator()]
    }),
    siteSearch: formBuilder.control<string | DashboardSiteLookup | null>(selectedSite, {
      validators: [Validators.required]
    }),
    amount: formBuilder.control<number | null>(allocation?.amount ?? null, {
      validators: [Validators.required, decimalValidator(), positiveDecimalValidator()]
    }),
    direction: formBuilder.nonNullable.control<SitePaymentDirection>(
      allocation?.direction ?? 'Out',
      { validators: [Validators.required] }
    )
  });
}

export function createInvoiceSiteAllocationsFormArray(
  totalValue: () => number | null,
  controls: InvoiceSiteAllocationFormGroup[] = []
): InvoiceSiteAllocationsFormArray {
  return new FormArray(controls, {
    validators: [invoiceSiteAllocationsValidator(totalValue)]
  });
}

export function toInvoiceSiteAllocationRequests(
  allocations: InvoiceSiteAllocationsFormArray
): readonly InvoiceSiteAllocationRequest[] {
  return allocations.controls.map((allocation) => {
    const value = allocation.getRawValue();
    return {
      siteId: value.siteId.trim(),
      amount: Number(value.amount),
      direction: value.direction
    };
  });
}

export function calculateAssignedSiteAmount(
  allocations: InvoiceSiteAllocationsFormArray
): number {
  const assignedAmount = allocations.controls.reduce((total, allocation) => {
    const amount = Number(allocation.controls.amount.value);
    return Number.isFinite(amount) && amount > 0 ? total + amount : total;
  }, 0);

  return Math.round((assignedAmount + Number.EPSILON) * 100) / 100;
}

export function getInvoiceSiteAllocationErrorMessage(error: unknown): string | null {
  if (!(error instanceof HttpErrorResponse)) {
    return null;
  }

  const details = error.error?.details as
    | readonly { field?: string; message?: string }[]
    | undefined;
  const allocationDetail = details?.find((detail) => {
    const field = detail.field?.toLowerCase() ?? '';
    return (
      field.includes('siteallocations') ||
      field.endsWith('siteid') ||
      field.endsWith('amount') ||
      field.endsWith('direction')
    );
  });

  if (allocationDetail?.message) {
    return allocationDetail.message;
  }

  return typeof error.error?.detail === 'string' ? error.error.detail : null;
}

function invoiceSiteAllocationsValidator(totalValue: () => number | null): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const allocations = control as InvoiceSiteAllocationsFormArray;
    const selectedSiteIds = allocations.controls
      .map((allocation) => allocation.controls.siteId.value.trim())
      .filter((siteId) => siteId.length > 0);

    if (new Set(selectedSiteIds).size !== selectedSiteIds.length) {
      return { duplicateSites: true };
    }

    const invoiceTotal = totalValue();
    if (invoiceTotal === null) {
      return null;
    }

    return calculateAssignedSiteAmount(allocations) > invoiceTotal
      ? { allocatedTotalExceeded: true }
      : null;
  };
}
