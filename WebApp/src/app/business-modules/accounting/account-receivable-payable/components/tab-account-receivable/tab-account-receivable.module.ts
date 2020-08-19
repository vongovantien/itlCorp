import { NgModule } from '@angular/core';

import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { AccountReceivableTabComponent } from './tab-account-receivable.component';
import { AccountReceivePayableModule } from '../../account-receivable-payable.module';
import { AccountReceivableListTrialOfficialComponent } from '../list-trial-official/list-trial-official-account-receivable.component';
import { AccountReceivableListGuaranteedComponent } from '../list-guaranteed/list-guaranteed-account-receivable.component';
import { AccountReceivableListOtherComponent } from '../list-other/list-other-account-receivable.component';
import { AccountReceivableFormSearchComponent } from '../form-search/account-receivable/form-search-account-receivable.component';
import { TabsModule } from 'ngx-bootstrap';
import { SharedModule } from 'src/app/shared/shared.module';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';



@NgModule({
    imports: [
        TabsModule.forRoot(),
        SharedModule,
        CommonModule,
        ReactiveFormsModule,
        SelectModule,
    ],
    exports: [

    ],
    declarations: [
        AccountReceivableTabComponent,
        AccountReceivableListTrialOfficialComponent,
        AccountReceivableListGuaranteedComponent,
        AccountReceivableListOtherComponent,
        AccountReceivableFormSearchComponent,
    ],
    providers: [],
    entryComponents: [
        AccountReceivableTabComponent
    ]
})
export class TabAccountReceivableModule {
    static rootComponent = AccountReceivableTabComponent;
}