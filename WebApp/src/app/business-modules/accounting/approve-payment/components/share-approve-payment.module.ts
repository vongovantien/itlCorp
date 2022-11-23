import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { ModalModule } from 'ngx-bootstrap/modal';
import { SharedModule } from 'src/app/shared/shared.module';
import { HistoryDeniedPopupComponent } from './popup/history-denied/history-denied.popup';
import { ProcessApporveComponent } from './process-approve/process-approve.component';


@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        ModalModule.forRoot(),

    ],
    exports: [
        ProcessApporveComponent,
        HistoryDeniedPopupComponent
    ],
    declarations: [
        ProcessApporveComponent,
        HistoryDeniedPopupComponent
    ],
    providers: [],
})
export class ShareApprovePaymentModule { }
