import { NgModule } from '@angular/core';

import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { AccountReceivableTabComponent } from './tab-account-receivable.component';
import { AccountReceivePayableModule } from '../../account-receivable-payable.module';



@NgModule({
    imports: [
        AccountReceivePayableModule,

    ],
    exports: [

    ],
    declarations: [

    ],
    providers: [],
    entryComponents: [
        AccountReceivableTabComponent
    ]
})
export class TabAccountReceivableModule {
    static rootComponent = AccountReceivableTabComponent;
}