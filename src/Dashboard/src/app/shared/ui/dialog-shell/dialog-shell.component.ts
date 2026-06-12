import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-dialog-shell',
  imports: [MatButtonModule, MatDialogModule, MatIconModule],
  templateUrl: './dialog-shell.component.html',
  styleUrl: './dialog-shell.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DialogShellComponent {}
