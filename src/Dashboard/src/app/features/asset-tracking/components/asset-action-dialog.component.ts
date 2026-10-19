import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ASSET_CONDITIONS, AssetActionRequest, AssetDetails, REGISTRATION_STATUSES } from '../models/asset-tracking.models';
import { AssetTrackingService } from '../services/asset-tracking.service';

export type AssetActionKind='registration-review'|'catalog-association'|'manufacturer-serial-number'|'site'|'condition'|'operational-status'|'missing'|'retirement'|'responsibility'|'qr-identifier';
export interface AssetActionDialogData{asset:AssetDetails;kind:AssetActionKind;title:string;accepted?:boolean;}

@Component({selector:'app-asset-action-dialog',imports:[ReactiveFormsModule,MatDialogModule,MatButtonModule,MatFormFieldModule,MatInputModule,MatSelectModule],template:`
<h2 mat-dialog-title>{{data.title}}</h2><mat-dialog-content><form class="dialog-form" [formGroup]="form">
@if(data.kind==='catalog-association'){<mat-form-field><mat-label>Catalog item ID</mat-label><input matInput formControlName="catalogItemId" /></mat-form-field>}
@if(data.kind==='manufacturer-serial-number'){<mat-form-field><mat-label>Manufacturer serial number</mat-label><input matInput formControlName="manufacturerSerialNumber" /></mat-form-field>}
@if(data.kind==='site'){<mat-form-field><mat-label>Site ID</mat-label><input matInput formControlName="siteId" /></mat-form-field>}
@if(data.kind==='condition'){<mat-form-field><mat-label>Condition</mat-label><mat-select formControlName="condition">@for(v of conditions;track v){<mat-option [value]="v">{{v}}</mat-option>}</mat-select></mat-form-field>}
@if(data.kind==='operational-status'){<mat-form-field><mat-label>Status</mat-label><mat-select formControlName="operationalStatus">@for(v of statuses;track v){<mat-option [value]="v">{{v}}</mat-option>}</mat-select></mat-form-field>}
@if(data.kind==='responsibility'){<mat-form-field><mat-label>Worker ID (empty removes)</mat-label><input matInput formControlName="responsibleWorkerId" /></mat-form-field><mat-form-field><mat-label>Reason (required)</mat-label><textarea matInput formControlName="reason"></textarea></mat-form-field>}
@if(data.kind!=='responsibility'){<mat-form-field><mat-label>{{data.kind==='registration-review'&&!data.accepted?'Rejection reason (optional)':'Note (optional)'}}</mat-label><textarea matInput formControlName="note"></textarea></mat-form-field>}
@if(conflict()){<p class="conflict" role="alert">The record changed. Your input is preserved and current state has been refreshed. Review it, then confirm again.</p>}@if(error()){<p class="error" role="alert">{{error()}}</p>}
</form></mat-dialog-content><mat-dialog-actions align="end"><button mat-button mat-dialog-close>Cancel</button><button mat-flat-button (click)="confirm()" [disabled]="saving()">Confirm</button></mat-dialog-actions>`,styles:`.dialog-form{display:grid;gap:.5rem;min-width:min(32rem,75vw);padding-top:.5rem}.conflict{color:var(--mat-sys-tertiary)}.error{color:var(--mat-sys-error)}`,changeDetection:ChangeDetectionStrategy.OnPush})
export class AssetActionDialogComponent{
 readonly data=inject<AssetActionDialogData>(MAT_DIALOG_DATA);private readonly ref=inject(MatDialogRef<AssetActionDialogComponent>);private readonly service=inject(AssetTrackingService);readonly conditions=ASSET_CONDITIONS;readonly statuses=REGISTRATION_STATUSES;readonly saving=signal(false);readonly conflict=signal(false);readonly error=signal('');
 readonly form=new FormGroup({catalogItemId:new FormControl(''),manufacturerSerialNumber:new FormControl(''),siteId:new FormControl(''),condition:new FormControl('Good'),operationalStatus:new FormControl('Available'),responsibleWorkerId:new FormControl(''),reason:new FormControl(''),note:new FormControl('')});
 async confirm():Promise<void>{if(this.data.kind==='responsibility'&&!this.form.controls.reason.value?.trim()){this.error.set('A responsibility change requires a reason.');return;}this.saving.set(true);this.error.set('');try{const v=this.form.getRawValue();const body:AssetActionRequest={concurrencyToken:this.data.asset.concurrencyToken,note:v.note||null};if(this.data.kind==='registration-review')body['accepted']=this.data.accepted===true;if(this.data.kind==='catalog-association')body['catalogItemId']=v.catalogItemId;if(this.data.kind==='manufacturer-serial-number')body['manufacturerSerialNumber']=v.manufacturerSerialNumber||null;if(this.data.kind==='site')body['siteId']=v.siteId;if(this.data.kind==='condition')body['condition']=v.condition;if(this.data.kind==='operational-status')body['operationalStatus']=v.operationalStatus;if(this.data.kind==='responsibility'){body['responsibleWorkerId']=v.responsibleWorkerId||null;body['reason']=v.reason;}const updated=await this.service.mutate(this.data.asset.id,this.data.kind,body);await this.service.invalidate(this.data.asset.id);this.ref.close(updated);}catch(e){if(this.service.isConcurrencyConflict(e)){this.conflict.set(true);await this.service.invalidate(this.data.asset.id);this.data.asset=await this.service.getAsset(this.data.asset.id);}else this.error.set('The action was rejected. Review the current state and supplied values.');}finally{this.saving.set(false);}}
}
