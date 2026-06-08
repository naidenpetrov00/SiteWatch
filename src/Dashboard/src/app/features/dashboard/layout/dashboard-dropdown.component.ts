import {
  ChangeDetectionStrategy,
  Component,
  OnDestroy,
  inject,
  input,
  viewChild
} from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { RouterLinkActive } from '@angular/router';

import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule, MatMenuTrigger } from '@angular/material/menu';

@Component({
  selector: 'app-dashboard-dropdown',
  imports: [RouterLinkActive, MatButtonModule, MatIconModule, MatMenuModule],
  templateUrl: './dashboard-dropdown.component.html',
  styleUrl: './dashboard-dropdown.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardDropdownComponent implements OnDestroy {
  readonly triggerLabel = input.required<string>();
  readonly triggerAriaLabel = input.required<string>();
  readonly menuXPosition = input<'before' | 'after'>('after');

  private readonly document = inject(DOCUMENT);
  private readonly menuTrigger = viewChild.required(MatMenuTrigger);
  private panelElement: HTMLElement | null = null;
  private closeTimeoutId: ReturnType<typeof setTimeout> | null = null;

  ngOnDestroy(): void {
    this.detachPanelHoverListeners();
  }

  openMenu(): void {
    this.cancelClose();
    const trigger = this.menuTrigger();

    if (!trigger.menuOpen) {
      trigger.openMenu();
    }
  }

  readonly scheduleClose = (): void => {
    this.clearCloseTimeout();
    this.closeTimeoutId = setTimeout(() => {
      this.menuTrigger().closeMenu();
    }, 240);
  };

  readonly cancelClose = (): void => {
    this.clearCloseTimeout();
  };

  attachPanelHoverListeners(): void {
    const trigger = this.menuTrigger();
    const panelId = trigger.menu?.panelId;

    if (!panelId) {
      return;
    }

    const panel = this.document.getElementById(panelId);

    if (!panel || panel === this.panelElement) {
      return;
    }

    this.detachPanelHoverListeners();

    this.panelElement = panel;
    this.panelElement.addEventListener('mouseenter', this.cancelClose);
    this.panelElement.addEventListener('mouseleave', this.scheduleClose);
  }

  detachPanelHoverListeners(): void {
    if (!this.panelElement) {
      this.clearCloseTimeout();
      return;
    }

    this.panelElement.removeEventListener('mouseenter', this.cancelClose);
    this.panelElement.removeEventListener('mouseleave', this.scheduleClose);
    this.panelElement = null;
    this.clearCloseTimeout();
  }

  private clearCloseTimeout(): void {
    if (this.closeTimeoutId === null) {
      return;
    }

    clearTimeout(this.closeTimeoutId);
    this.closeTimeoutId = null;
  }
}
