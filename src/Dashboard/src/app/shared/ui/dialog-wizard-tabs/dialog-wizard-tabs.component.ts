import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';

import { DialogWizardTabDefinition } from './dialog-wizard-tabs.types';

@Component({
  selector: 'app-dialog-wizard-tabs',
  templateUrl: './dialog-wizard-tabs.component.html',
  styleUrl: './dialog-wizard-tabs.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DialogWizardTabsComponent {
  readonly tabs = input.required<readonly DialogWizardTabDefinition[]>();
  readonly selectedTabId = input.required<string>();
  readonly ariaLabel = input('Dialog sections');

  readonly selectedTabIdChange = output<string>();

  selectTab(tabId: string): void {
    const tab = this.tabs().find((candidate) => candidate.id === tabId);

    if (!tab || tab.disabled) {
      return;
    }

    this.selectedTabIdChange.emit(tabId);
  }
}
