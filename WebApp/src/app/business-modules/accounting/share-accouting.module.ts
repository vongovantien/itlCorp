import { NgModule } from '@angular/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareAccountingInputShipmentPopupComponent } from './components/input-shipment/input-shipment.popup';
import { CommonModule } from '@angular/common';
import { ModalModule } from 'ngx-bootstrap/modal';
import { FormsModule } from '@angular/forms';
import { ShareAccountingManagementSelectRequesterPopupComponent } from './components/select-requester/select-requester.popup';
import { StoreModule } from '@ngrx/store';
import { reducers, effects } from './accounting-management/store';
import { EffectsModule } from '@ngrx/effects';


@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        ModalModule.forRoot(),
        StoreModule.forFeature('accounting-management', reducers),
        EffectsModule.forFeature(effects),


    ],
    exports: [ShareAccountingInputShipmentPopupComponent, ShareAccountingManagementSelectRequesterPopupComponent],
    declarations: [ShareAccountingInputShipmentPopupComponent, ShareAccountingManagementSelectRequesterPopupComponent],
    providers: [],
})
export class ShareAccountingModule { }
