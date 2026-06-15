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
  readonly submitDisabled = input(false);
  readonly showCancel = input(true);
  readonly showSubmit = input(true);
  readonly submitFormId = input<string | null>(null);
  readonly submitButtonType = input<'button' | 'submit'>('button');

  readonly cancel = output<void>();
  readonly submit = output<void>();

  handleCancel(): void {
    this.cancel.emit();
  }

  handleSubmit(): void {
    this.submit.emit();
  }
}
