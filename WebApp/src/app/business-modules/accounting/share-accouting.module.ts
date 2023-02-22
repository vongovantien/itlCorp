import { NgModule } from '@angular/core';
import { EffectsModule } from '@ngrx/effects';
import { StoreModule } from '@ngrx/store';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TooltipModule } from 'ngx-bootstrap/tooltip';
import { SharedModule } from 'src/app/shared/shared.module';
import { effects, reducers } from './accounting-management/store';
import { AccoutingAttachFileListComponent } from './components/attach-file/attach-file-list.component';
import { AccountingSelectAttachFilePopupComponent } from './components/select-attach-file/select-attach-file.popup';
import { ShareAccountingManagementSelectRequesterPopupComponent } from './components/select-requester/select-requester.popup';

@NgModule({
    imports: [
        SharedModule,
        ModalModule.forRoot(),
        TooltipModule.forRoot(),
        StoreModule.forFeature('accounting-management', reducers),
        EffectsModule.forFeature(effects),
    ],
    exports: [TooltipModule, ShareAccountingManagementSelectRequesterPopupComponent, AccoutingAttachFileListComponent, AccountingSelectAttachFilePopupComponent],
    declarations: [ShareAccountingManagementSelectRequesterPopupComponent, AccoutingAttachFileListComponent, AccountingSelectAttachFilePopupComponent],
    providers: [],
})
export class ShareAccountingModule { }
