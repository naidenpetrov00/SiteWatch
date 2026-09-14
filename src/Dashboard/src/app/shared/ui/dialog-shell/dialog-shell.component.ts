import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  input
} from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialogModule
} from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

import { DialogShellContent } from './dialog-shell.types';

@Component({
  selector: 'app-dialog-shell',
  imports: [MatButtonModule, MatDialogModule, MatIconModule],
  templateUrl: './dialog-shell.component.html',
  styleUrl: './dialog-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DialogShellComponent {
  private readonly dialogData = inject(MAT_DIALOG_DATA, { optional: true }) as
    | DialogShellContent
    | null;

  readonly eyebrow = input<string | null>(null);
  readonly title = input<string | null>(null);
  readonly subtitle = input<string | null>(null);
  readonly closeAriaLabel = input<string | null>(null);
  readonly closeDisabled = input(false);

  readonly resolvedEyebrow = computed(
    () => this.eyebrow() ?? this.dialogData?.eyebrow ?? null
  );
  readonly resolvedTitle = computed(() => this.title() ?? this.dialogData?.title ?? null);
  readonly resolvedSubtitle = computed(
    () => this.subtitle() ?? this.dialogData?.subtitle ?? null
  );
  readonly resolvedCloseAriaLabel = computed(
    () => this.closeAriaLabel() ?? this.dialogData?.closeAriaLabel ?? 'Close dialog'
  );
}
