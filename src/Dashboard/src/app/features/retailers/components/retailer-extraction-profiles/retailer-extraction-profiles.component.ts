import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import {
  FormField,
  debounce,
  form,
  maxLength,
  required,
  submit,
  validate
} from '@angular/forms/signals';
import { MatButtonModule } from '@angular/material/button';
import {
  MatChipInputEvent,
  MatChipsModule
} from '@angular/material/chips';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { injectQuery } from '@tanstack/angular-query-experimental';
import { firstValueFrom } from 'rxjs';

import {
  ExtractionProfileConfirmDialogComponent,
  ExtractionProfileConfirmDialogData
} from '../extraction-profile-confirm-dialog/extraction-profile-confirm-dialog.component';
import { ExtractionRuleDialogComponent } from '../extraction-rule-dialog/extraction-rule-dialog.component';
import {
  RetailerExtractionProfileDetails,
  RetailerExtractionProfileSummary,
  RetailerExtractionRule,
  RetailerExtractionTestResult,
  RetailerExtractionTestSourceType,
  SaveRetailerExtractionRuleRequest
} from '../../models/retailer-extraction-profile.models';
import {
  RetailerExtractionProfilesService,
  retailerExtractionProfileKeys
} from '../../services/retailer-extraction-profiles.service';
import { getRetailerError } from '../../utils/retailer-error';
import {
  RetailerListingsService,
  retailerListingKeys
} from '../../../retailer-listings/services/retailer-listings.service';
import { RetailerPriceCollectionsComponent } from '../retailer-price-collections/retailer-price-collections.component';

@Component({
  selector: 'app-company-extraction-profiles',
  imports: [
    ReactiveFormsModule,
    FormField,
    MatButtonModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    RetailerPriceCollectionsComponent
  ],
  templateUrl: './retailer-extraction-profiles.component.html',
  styleUrl: './retailer-extraction-profiles.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CompanyExtractionProfilesComponent {
  private readonly service = inject(RetailerExtractionProfilesService);
  private readonly dialog = inject(MatDialog);
  private readonly formBuilder = inject(FormBuilder);
  private readonly listingsService = inject(RetailerListingsService);
  private loadedProfileId: string | null = null;
  private loadedTestConfigurationKey: string | null = null;

  readonly companyPersonId = input.required<string>();
  readonly selectedProfileId = signal<string | null>(null);
  readonly operationPending = signal(false);
  readonly feedback = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly testPending = signal(false);
  readonly testResult = signal<RetailerExtractionTestResult | null>(null);
  readonly testError = signal<string | null>(null);
  readonly hostsForm = this.formBuilder.group({
    allowedHosts: this.formBuilder.nonNullable.control<string[]>([]),
    hostInput: this.formBuilder.nonNullable.control('')
  });
  readonly hostInput = this.hostsForm.controls.hostInput;
  readonly hostSeparatorKeyCodes = [ENTER, COMMA] as const;
  readonly testModel = signal<{
    sourceType: RetailerExtractionTestSourceType;
    retailerListingId: string;
    manualUrl: string;
    listingSearch: string;
  }>({
    sourceType: 'retailerListing',
    retailerListingId: '',
    manualUrl: '',
    listingSearch: ''
  });
  readonly testForm = form(this.testModel, (path) => {
    debounce(path.listingSearch, 300);
    maxLength(path.listingSearch, 200, { message: 'Search is limited to 200 characters.' });
    maxLength(path.manualUrl, 2048, { message: 'The URL is limited to 2048 characters.' });
    required(path.retailerListingId, {
      when: ({ valueOf }) => valueOf(path.sourceType) === 'retailerListing',
      message: 'Select a retailer listing.'
    });
    required(path.manualUrl, {
      when: ({ valueOf }) => valueOf(path.sourceType) === 'manualUrl',
      message: 'Enter a product URL.'
    });
    validate(path.manualUrl, ({ value, valueOf }) => {
      if (valueOf(path.sourceType) !== 'manualUrl' || value().length === 0) {
        return undefined;
      }
      try {
        const url = new URL(value());
        return url.protocol === 'https:'
          ? undefined
          : { kind: 'https', message: 'Enter an absolute HTTPS URL.' };
      } catch {
        return { kind: 'url', message: 'Enter a valid absolute HTTPS URL.' };
      }
    });
  });

  readonly versionsQuery = injectQuery(() => {
    const companyPersonId = this.companyPersonId();
    return {
      queryKey: retailerExtractionProfileKeys.versions(companyPersonId),
      queryFn: () => this.service.getVersions(companyPersonId),
      enabled: companyPersonId.length > 0
    };
  });
  readonly currentQuery = injectQuery(() => {
    const companyPersonId = this.companyPersonId();
    return {
      queryKey: retailerExtractionProfileKeys.current(companyPersonId),
      queryFn: () => this.service.getCurrent(companyPersonId),
      enabled: companyPersonId.length > 0
    };
  });
  readonly detailQuery = injectQuery(() => {
    const companyPersonId = this.companyPersonId();
    const profileId = this.selectedProfileId();
    return {
      queryKey: retailerExtractionProfileKeys.detail(companyPersonId, profileId ?? ''),
      queryFn: () => this.service.getById(companyPersonId, profileId!),
      enabled: companyPersonId.length > 0 && profileId !== null
    };
  });
  readonly listingsQuery = injectQuery(() => {
    const companyPersonId = this.companyPersonId();
    const model = this.testModel();
    const state = {
      pageIndex: 0,
      pageSize: 25,
      sortActive: 'product',
      sortDirection: 'asc',
      searchTerm: model.listingSearch,
      includeInactive: true,
      isActive: null
    };
    return {
      queryKey: retailerListingKeys.company(companyPersonId, state),
      queryFn: () => this.listingsService.getForCompany(companyPersonId, state),
      enabled: companyPersonId.length > 0 &&
        this.detailQuery.data()?.summary.status === 'draft' &&
        model.sourceType === 'retailerListing'
    };
  });

  readonly selectedProfile = computed(() => this.detailQuery.data());
  readonly currentDraft = computed(() => this.currentQuery.data()?.draft ?? null);
  readonly activeProfile = computed(() => this.currentQuery.data()?.active ?? null);
  readonly canEditSelected = computed(
    () => this.selectedProfile()?.summary.status === 'draft'
  );

  constructor() {
    effect(() => {
      const versions = this.versionsQuery.data();
      const current = this.currentQuery.data();
      if (!versions || !current) return;

      const selectedId = this.selectedProfileId();
      if (!selectedId || !versions.some((version) => version.id === selectedId)) {
        this.selectedProfileId.set(
          current.draft?.summary.id ??
            current.active?.summary.id ??
            versions[0]?.id ??
            null
        );
      }
    });

    effect(() => {
      const profile = this.selectedProfile();
      if (!profile || profile.summary.id !== this.selectedProfileId()) return;

      const profileChanged = this.loadedProfileId !== profile.summary.id;
      if (profileChanged || !this.hostsForm.dirty || profile.summary.status !== 'draft') {
        this.hostsForm.reset(
          { allowedHosts: [...profile.allowedHosts], hostInput: '' },
          { emitEvent: false }
        );
      }
      this.loadedProfileId = profile.summary.id;
      profile.summary.status === 'draft'
        ? this.hostsForm.enable({ emitEvent: false })
        : this.hostsForm.disable({ emitEvent: false });

      const testConfigurationKey =
        `${profile.summary.id}:${profile.summary.configurationRevision}`;
      if (this.loadedTestConfigurationKey !== testConfigurationKey) {
        this.testResult.set(null);
        this.testError.set(null);
        this.testForm().reset();
      }
      this.loadedTestConfigurationKey = testConfigurationKey;
    });
  }

  async selectVersion(version: RetailerExtractionProfileSummary): Promise<void> {
    if (version.id === this.selectedProfileId()) return;
    if (!(await this.confirmUnsavedHostLoss())) return;
    this.clearFeedback();
    this.hostsForm.markAsPristine();
    this.selectedProfileId.set(version.id);
  }

  async createDraft(source?: RetailerExtractionProfileSummary): Promise<void> {
    if (this.currentDraft() || this.operationPending()) return;
    const sourceId = source?.status === 'published'
      ? source.id
      : this.activeProfile()?.summary.id;
    await this.runMutation(
      () => this.service.createDraft(this.companyPersonId(), sourceId),
      (profile) => {
        this.selectedProfileId.set(profile.summary.id);
        return sourceId
          ? `Draft version ${profile.summary.version} was created from the published profile.`
          : `Draft version ${profile.summary.version} was created.`;
      }
    );
  }

  async discardDraft(): Promise<void> {
    const profile = this.selectedProfile();
    if (!profile || profile.summary.status !== 'draft' || this.operationPending()) return;
    const confirmed = await this.confirm({
      eyebrow: 'Price Extraction',
      title: `Discard Draft v${profile.summary.version}`,
      subtitle: 'Only draft profiles can be deleted.',
      message: this.hostsForm.dirty
        ? 'This permanently removes the saved draft and all unsaved host changes.'
        : 'This permanently removes the draft, its hosts, and all extraction rules.',
      confirmLabel: 'Discard Draft'
    });
    if (!confirmed) return;

    this.operationPending.set(true);
    this.clearFeedback();
    try {
      await this.service.deleteDraft(this.companyPersonId(), profile.summary.id);
      this.hostsForm.reset(
        { allowedHosts: [], hostInput: '' },
        { emitEvent: false }
      );
      this.loadedProfileId = null;
      await Promise.all([this.versionsQuery.refetch(), this.currentQuery.refetch()]);
      const current = this.currentQuery.data();
      const versions = this.versionsQuery.data() ?? [];
      this.selectedProfileId.set(
        current?.active?.summary.id ?? versions[0]?.id ?? null
      );
      this.feedback.set(`Draft version ${profile.summary.version} was discarded.`);
    } catch (error) {
      this.error.set(getRetailerError(error, 'The draft could not be discarded.'));
    } finally {
      this.operationPending.set(false);
    }
  }

  addHost(event: MatChipInputEvent): void {
    const raw = event.value.trim().replace(/\.$/, '').toLowerCase();
    event.chipInput?.clear();
    this.hostInput.setValue('');
    if (!raw) return;
    if (!isPlausibleHost(raw)) {
      this.error.set('Enter an exact DNS hostname without a scheme, port, path, wildcard, or IP address.');
      return;
    }
    const hosts = this.hostsForm.controls.allowedHosts.value;
    if (hosts.includes(raw)) {
      this.error.set('That allowed host is already present.');
      return;
    }
    this.error.set(null);
    this.hostsForm.controls.allowedHosts.setValue([...hosts, raw]);
    this.hostsForm.controls.allowedHosts.markAsDirty();
  }

  removeHost(host: string): void {
    if (!this.canEditSelected()) return;
    this.hostsForm.controls.allowedHosts.setValue(
      this.hostsForm.controls.allowedHosts.value.filter((item) => item !== host)
    );
    this.hostsForm.controls.allowedHosts.markAsDirty();
  }

  discardHostChanges(): void {
    const profile = this.selectedProfile();
    if (!profile || profile.summary.status !== 'draft' || this.operationPending()) return;
    this.hostsForm.reset(
      { allowedHosts: [...profile.allowedHosts], hostInput: '' },
      { emitEvent: false }
    );
    this.error.set(null);
  }

  async saveHosts(): Promise<void> {
    const profile = this.selectedProfile();
    if (
      !profile ||
      profile.summary.status !== 'draft' ||
      !this.hostsForm.dirty ||
      this.operationPending()
    ) return;
    if (this.hostInput.value.trim()) {
      this.error.set('Add the pending hostname with Enter or discard it before saving.');
      return;
    }
    await this.runMutation(
      () => this.service.updateAllowedHosts(
        this.companyPersonId(),
        profile.summary.id,
        this.hostsForm.controls.allowedHosts.value
      ),
      (savedProfile) => {
        this.hostsForm.reset(
          { allowedHosts: [...savedProfile.allowedHosts], hostInput: '' },
          { emitEvent: false }
        );
        return 'Allowed hosts were saved.';
      }
    );
  }

  async openRuleDialog(rule: RetailerExtractionRule | null): Promise<void> {
    const profile = this.selectedProfile();
    if (!profile || profile.summary.status !== 'draft' || this.operationPending()) return;
    const dialogRef = this.dialog.open<
      ExtractionRuleDialogComponent,
      RetailerExtractionRule | null,
      SaveRetailerExtractionRuleRequest | null
    >(ExtractionRuleDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      width: '52rem',
      maxWidth: 'calc(100vw - 2rem)',
      data: rule
    });
    const request = await firstValueFrom(dialogRef.afterClosed());
    if (!request) return;

    await this.runMutation(
      () => rule
        ? this.service.updateRule(this.companyPersonId(), profile.summary.id, rule.id, request)
        : this.service.addRule(this.companyPersonId(), profile.summary.id, request),
      () => rule ? 'Extraction rule was updated.' : 'Extraction rule was added.'
    );
  }

  async deleteRule(rule: RetailerExtractionRule): Promise<void> {
    const profile = this.selectedProfile();
    if (!profile || profile.summary.status !== 'draft' || this.operationPending()) return;
    if (!(await this.confirm({
      eyebrow: 'Extraction Rule',
      title: `Remove ${rule.name}`,
      subtitle: 'The remaining rules will be reordered automatically.',
      message: 'Remove this rule from the current draft?',
      confirmLabel: 'Remove Rule'
    }))) return;

    this.operationPending.set(true);
    this.clearFeedback();
    try {
      await this.service.deleteRule(this.companyPersonId(), profile.summary.id, rule.id);
      this.feedback.set('Extraction rule was removed.');
    } catch (error) {
      this.error.set(getRetailerError(error, 'The extraction rule could not be removed.'));
    } finally {
      this.operationPending.set(false);
    }
  }

  async setRuleEnabled(rule: RetailerExtractionRule): Promise<void> {
    const profile = this.selectedProfile();
    if (!profile || profile.summary.status !== 'draft' || this.operationPending()) return;
    await this.runMutation(
      () => this.service.setRuleEnabled(
        this.companyPersonId(),
        profile.summary.id,
        rule.id,
        !rule.isEnabled
      ),
      () => `Extraction rule was ${rule.isEnabled ? 'disabled' : 'enabled'}.`
    );
  }

  async moveRule(index: number, direction: -1 | 1): Promise<void> {
    const profile = this.selectedProfile();
    const target = index + direction;
    if (
      !profile ||
      profile.summary.status !== 'draft' ||
      target < 0 ||
      target >= profile.rules.length ||
      this.operationPending()
    ) return;
    const ids = profile.rules.map((rule) => rule.id);
    [ids[index], ids[target]] = [ids[target], ids[index]];
    await this.runMutation(
      () => this.service.reorderRules(this.companyPersonId(), profile.summary.id, ids),
      () => 'Extraction rule order was updated.'
    );
  }

  async publish(): Promise<void> {
    const profile = this.selectedProfile();
    if (!profile || profile.summary.status !== 'draft' || this.operationPending()) return;
    if (this.hostsForm.dirty) {
      this.error.set('Save or discard the unsaved host changes before publishing.');
      return;
    }
    if (!profile.summary.isCurrentConfigurationValidated) {
      this.error.set('Run a successful test against the current saved configuration before publishing.');
      return;
    }
    if (!(await this.confirm({
      eyebrow: 'Publish Extraction Profile',
      title: `Publish Version ${profile.summary.version}`,
      subtitle: 'Published configuration is immutable.',
      message: 'This version will become active and replace the currently active profile.',
      confirmLabel: 'Publish Profile'
    }))) return;
    await this.runMutation(
      () => this.service.publish(this.companyPersonId(), profile.summary.id),
      () => `Extraction profile version ${profile.summary.version} was published and activated.`
    );
  }

  async activate(version: RetailerExtractionProfileSummary): Promise<void> {
    if (version.status !== 'published' || version.isActive || this.operationPending()) return;
    if (!(await this.confirm({
      eyebrow: 'Activate Extraction Profile',
      title: `Activate Version ${version.version}`,
      subtitle: 'The profile configuration remains immutable.',
      message: 'This version will replace the currently active extraction profile.',
      confirmLabel: 'Activate Version'
    }))) return;
    await this.runMutation(
      () => this.service.activate(this.companyPersonId(), version.id),
      () => `Extraction profile version ${version.version} is now active.`,
      false
    );
  }

  formatDate(value: string | null): string {
    return value
      ? new Intl.DateTimeFormat(undefined, {
          dateStyle: 'medium',
          timeStyle: 'short'
        }).format(new Date(value))
      : '—';
  }

  ruleTypeLabel(rule: RetailerExtractionRule): string {
    return rule.ruleType === 'jsonLd' ? 'JSON-LD' : 'CSS selector';
  }

  setTestSource(sourceType: RetailerExtractionTestSourceType): void {
    this.testModel.update((model) => ({
      ...model,
      sourceType,
      retailerListingId: sourceType === 'retailerListing'
        ? model.retailerListingId
        : '',
      manualUrl: sourceType === 'manualUrl' ? model.manualUrl : ''
    }));
    this.testResult.set(null);
    this.testError.set(null);
  }

  runTest(): void {
    const profile = this.selectedProfile();
    if (!profile || profile.summary.status !== 'draft' || this.testPending()) return;
    if (this.hasUnsavedHostChanges()) {
      this.testError.set(
        'Save or discard host changes, including the pending hostname, before testing.'
      );
      return;
    }

    submit(this.testForm, async () => {
      this.testPending.set(true);
      this.testResult.set(null);
      this.testError.set(null);
      const model = this.testModel();
      try {
        const result = await this.service.test(
          this.companyPersonId(),
          profile.summary.id,
          {
            sourceType: model.sourceType,
            retailerListingId: model.sourceType === 'retailerListing'
              ? model.retailerListingId
              : null,
            manualUrl: model.sourceType === 'manualUrl'
              ? model.manualUrl.trim()
              : null
          }
        );
        this.testResult.set(result);
        if (!result.success) {
          this.testError.set(
            result.isCurrentConfigurationValidated
              ? 'No rule matched this page. An earlier successful test still validates the unchanged draft.'
              : 'No enabled extraction rule produced a valid EUR price.'
          );
        }
      } catch (error) {
        const message = getExtractionTestError(error);
        this.testError.set(
          profile.summary.isCurrentConfigurationValidated
            ? `${message} The earlier successful test still validates this unchanged revision.`
            : message
        );
      } finally {
        this.testPending.set(false);
      }
    });
  }

  hasUnsavedHostChanges(): boolean {
    return this.hostsForm.dirty || this.hostInput.value.trim().length > 0;
  }

  diagnosticLabel(outcome: string): string {
    return outcome
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, (value) => value.toUpperCase());
  }

  formatAmount(amount: number): string {
    return amount.toFixed(2);
  }

  private async runMutation(
    operation: () => Promise<RetailerExtractionProfileDetails>,
    successMessage: (profile: RetailerExtractionProfileDetails) => string,
    selectResult = true
  ): Promise<void> {
    this.operationPending.set(true);
    this.clearFeedback();
    try {
      const profile = await operation();
      if (selectResult) {
        this.selectedProfileId.set(profile.summary.id);
      }
      this.feedback.set(successMessage(profile));
    } catch (error) {
      this.error.set(getRetailerError(error, 'The extraction profile could not be updated.'));
    } finally {
      this.operationPending.set(false);
    }
  }

  private async confirmUnsavedHostLoss(): Promise<boolean> {
    if (!this.hostsForm.dirty || !this.canEditSelected()) return true;
    return this.confirm({
      eyebrow: 'Unsaved Changes',
      title: 'Discard Unsaved Host Changes',
      subtitle: 'The saved draft will remain available.',
      message: 'Switch versions and discard the unsaved allowed-host changes?',
      confirmLabel: 'Discard Changes'
    });
  }

  private async confirm(data: ExtractionProfileConfirmDialogData): Promise<boolean> {
    const dialogRef = this.dialog.open<
      ExtractionProfileConfirmDialogComponent,
      ExtractionProfileConfirmDialogData,
      boolean
    >(ExtractionProfileConfirmDialogComponent, {
      autoFocus: false,
      restoreFocus: true,
      width: '34rem',
      maxWidth: 'calc(100vw - 2rem)',
      data
    });
    return (await firstValueFrom(dialogRef.afterClosed())) === true;
  }

  private clearFeedback(): void {
    this.feedback.set(null);
    this.error.set(null);
  }
}

function isPlausibleHost(value: string): boolean {
  return value.length <= 253 &&
    !/[\s/:*?@#]/.test(value) &&
    !/^\d{1,3}(\.\d{1,3}){3}$/.test(value) &&
    value.split('.').every((label) =>
      label.length > 0 &&
      label.length <= 63 &&
      !label.startsWith('-') &&
      !label.endsWith('-')
    );
}

function getExtractionTestError(error: unknown): string {
  if (!(error instanceof HttpErrorResponse)) {
    return 'The extraction test could not be completed.';
  }
  const detail = error.error?.detail;
  if (typeof detail === 'string' && detail.trim()) return detail;
  const validationDetail = Array.isArray(error.error?.details)
    ? error.error.details.find((item: { message?: unknown }) =>
        typeof item?.message === 'string' && item.message.trim().length > 0
      )
    : null;
  if (typeof validationDetail?.message === 'string') return validationDetail.message;
  const category = error.error?.category;
  if (category === 'security') {
    return 'The URL or one of its redirects was rejected for security reasons.';
  }
  if (category === 'network') return 'The remote page could not be reached successfully.';
  if (category === 'content') return 'The remote response was not suitable bounded HTML.';
  if (category === 'timeout') return 'The extraction test exceeded its time limit.';
  if (error.status === 400) return 'Choose exactly one valid test source.';
  if (error.status === 404) return 'The selected profile or retailer listing was not found.';
  if (error.status === 409) return 'The draft cannot be tested because its saved state changed or is not eligible.';
  return 'The extraction test could not be completed.';
}
