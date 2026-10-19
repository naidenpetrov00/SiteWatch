import { ChangeDetectionStrategy, Component, effect, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute } from '@angular/router';
import { injectQuery } from '@tanstack/angular-query-experimental';
import * as QRCode from 'qrcode';
import { AssetTrackingService } from '../services/asset-tracking.service';

@Component({selector:'app-asset-label-page',imports:[MatButtonModule],template:`
<main class="label-page">@if(assetQuery.data();as asset){<section class="label" aria-label="Printable asset label"><strong>{{asset.assetCode}}</strong><span>{{asset.catalogName}}</span>@if(qrDataUrl()){<img [src]="qrDataUrl()" [alt]="'QR code for '+asset.assetCode" />}<small>{{asset.qrIdentifier}}</small></section><button class="no-print" mat-flat-button type="button" (click)="print()">Print label</button>}@else{<p>Loading label…</p>}</main>`,styles:`
:host{display:block}.label-page{display:grid;place-items:center;gap:1rem}.label{width:320px;padding:20px;border:1px solid #111;display:grid;place-items:center;gap:8px;background:#fff;color:#000}.label strong{font-size:1.6rem}.label img{width:220px;height:220px}.label small{overflow-wrap:anywhere;text-align:center}@media print{.no-print{display:none!important}:host{position:fixed;inset:0;background:#fff}.label-page{height:100%}.label{border:0;break-inside:avoid}}@page{margin:8mm}`,changeDetection:ChangeDetectionStrategy.OnPush})
export class AssetLabelPage{
 private readonly route=inject(ActivatedRoute);private readonly service=inject(AssetTrackingService);readonly id=this.route.snapshot.paramMap.get('id')!;readonly qrDataUrl=signal('');readonly assetQuery=injectQuery(()=>({queryKey:['asset-tracking','asset',this.id] as const,queryFn:()=>this.service.getAsset(this.id)}));
 constructor(){effect(()=>{const asset=this.assetQuery.data();if(asset)void this.renderQr(asset.qrIdentifier);});}
 private async renderQr(qrIdentifier:string):Promise<void>{this.qrDataUrl.set(await QRCode.toDataURL(qrIdentifier,{errorCorrectionLevel:'M',margin:1,width:512}));}
 print():void{globalThis.print();}
}
