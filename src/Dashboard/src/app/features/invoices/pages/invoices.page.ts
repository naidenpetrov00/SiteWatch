import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-invoices-page',
  template: '<main class="invoices-page"></main>',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoicesPage {}
