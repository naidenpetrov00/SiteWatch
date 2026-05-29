import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-scan-invoice-page',
  template: '<main class="scan-invoice-page"><h1>Scan Invoice</h1></main>',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ScanInvoicePage {}
