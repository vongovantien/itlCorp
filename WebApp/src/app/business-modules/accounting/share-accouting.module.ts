import { NgModule } from '@angular/core';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { ShareAccountingManagementSelectRequesterPopupComponent } from './components/select-requester/select-requester.popup';
import { StoreModule } from '@ngrx/store';
import { reducers, effects } from './accounting-management/store';
import { EffectsModule } from '@ngrx/effects';


@NgModule({
    imports: [
        SharedModule,
        ModalModule.forRoot(),
        StoreModule.forFeature('accounting-management', reducers),
        EffectsModule.forFeature(effects),
    ],
    exports: [ShareAccountingManagementSelectRequesterPopupComponent],
    declarations: [ShareAccountingManagementSelectRequesterPopupComponent],
    providers: [],
})
export class ShareAccountingModule { }
