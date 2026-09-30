import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  input,
  signal
} from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
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
  SaveRetailerExtractionRuleRequest
} from '../../models/retailer-extraction-profile.models';
import {
  RetailerExtractionProfilesService,
  retailerExtractionProfileKeys
} from '../../services/retailer-extraction-profiles.service';
import { getRetailerError } from '../../utils/retailer-error';

@Component({
  selector: 'app-retailer-extraction-profiles',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './retailer-extraction-profiles.component.html',
  styleUrl: './retailer-extraction-profiles.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RetailerExtractionProfilesComponent {
  private readonly service = inject(RetailerExtractionProfilesService);
  private readonly dialog = inject(MatDialog);
  private readonly formBuilder = inject(FormBuilder);
  private loadedProfileId: string | null = null;

  readonly retailerId = input.required<string>();
  readonly selectedProfileId = signal<string | null>(null);
  readonly operationPending = signal(false);
  readonly feedback = signal<string | null>(null);
  readonly error = signal<string | null>(null);
  readonly hostsForm = this.formBuilder.group({
    allowedHosts: this.formBuilder.nonNullable.control<string[]>([]),
    hostInput: this.formBuilder.nonNullable.control('')
  });
  readonly hostInput = this.hostsForm.controls.hostInput;
  readonly hostSeparatorKeyCodes = [ENTER, COMMA] as const;

  readonly versionsQuery = injectQuery(() => {
    const retailerId = this.retailerId();
    return {
      queryKey: retailerExtractionProfileKeys.versions(retailerId),
      queryFn: () => this.service.getVersions(retailerId),
      enabled: retailerId.length > 0
    };
  });
  readonly currentQuery = injectQuery(() => {
    const retailerId = this.retailerId();
    return {
      queryKey: retailerExtractionProfileKeys.current(retailerId),
      queryFn: () => this.service.getCurrent(retailerId),
      enabled: retailerId.length > 0
    };
  });
  readonly detailQuery = injectQuery(() => {
    const retailerId = this.retailerId();
    const profileId = this.selectedProfileId();
    return {
      queryKey: retailerExtractionProfileKeys.detail(retailerId, profileId ?? ''),
      queryFn: () => this.service.getById(retailerId, profileId!),
      enabled: retailerId.length > 0 && profileId !== null
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
      : this.activeProfile()?.summary.id ?? null;
    await this.runMutation(
      () => this.service.createDraft(this.retailerId(), sourceId),
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
      await this.service.deleteDraft(this.retailerId(), profile.summary.id);
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
        this.retailerId(),
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
        ? this.service.updateRule(this.retailerId(), profile.summary.id, rule.id, request)
        : this.service.addRule(this.retailerId(), profile.summary.id, request),
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
      await this.service.deleteRule(this.retailerId(), profile.summary.id, rule.id);
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
        this.retailerId(),
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
      () => this.service.reorderRules(this.retailerId(), profile.summary.id, ids),
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
    if (!(await this.confirm({
      eyebrow: 'Publish Extraction Profile',
      title: `Publish Version ${profile.summary.version}`,
      subtitle: 'Published configuration is immutable.',
      message: 'This version will become active and replace the currently active profile.',
      confirmLabel: 'Publish Profile'
    }))) return;
    await this.runMutation(
      () => this.service.publish(this.retailerId(), profile.summary.id),
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
      () => this.service.activate(this.retailerId(), version.id),
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
