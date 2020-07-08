import { NgModule } from '@angular/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareAccountingInputShipmentPopupComponent } from './components/input-shipment/input-shipment.popup';
import { CommonModule } from '@angular/common';
import { ModalModule } from 'ngx-bootstrap';
import { FormsModule } from '@angular/forms';
import { ShareAccountingManagementSelectRequesterPopupComponent } from './components/select-requester/select-requester.popup';


@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        ModalModule.forRoot(),

    ],
    exports: [ShareAccountingInputShipmentPopupComponent, ShareAccountingManagementSelectRequesterPopupComponent],
    declarations: [ShareAccountingInputShipmentPopupComponent, ShareAccountingManagementSelectRequesterPopupComponent],
    providers: [],
})
export class ShareAccountingModule { }
