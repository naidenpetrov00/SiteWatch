import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-dialog-action-bar',
  imports: [MatButtonModule],
  templateUrl: './dialog-action-bar.component.html',
  styleUrl: './dialog-action-bar.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DialogActionBarComponent {
  readonly cancelLabel = input('Cancel');
  readonly submitLabel = input('Submit');
  readonly secondaryLabel = input<string | null>(null);
  readonly submitDisabled = input(false);
  readonly secondaryDisabled = input(false);
  readonly showCancel = input(true);
  readonly showSubmit = input(true);
  readonly showSecondary = input(false);
  readonly submitFormId = input<string | null>(null);
  readonly submitButtonType = input<'button' | 'submit'>('button');
  readonly secondaryButtonType = input<'button' | 'submit'>('button');

  readonly cancel = output<void>();
  readonly submit = output<void>();
  readonly secondarySubmit = output<void>();

  handleCancel(): void {
    this.cancel.emit();
  }

  handleSubmit(): void {
    this.submit.emit();
  }

  handleSecondarySubmit(): void {
    this.secondarySubmit.emit();
  }
}
