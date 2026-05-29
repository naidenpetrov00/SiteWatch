import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-invoices-page',
  template: '<main class="invoices-page"><h1>Invoices</h1></main>',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class InvoicesPage {}
