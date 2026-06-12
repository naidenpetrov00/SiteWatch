import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';

export type ActionButtonVariant = 'primary' | 'secondary';

@Component({
  selector: 'app-action-button',
  imports: [MatButtonModule],
  templateUrl: './action-button.component.html',
  styleUrl: './action-button.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ActionButtonComponent {
  readonly label = input.required<string>();
  readonly variant = input<ActionButtonVariant>('secondary');
  readonly disabled = input(false);

  readonly clicked = output<void>();

  handleClick(): void {
    if (this.disabled()) {
      return;
    }

    this.clicked.emit();
  }
}
