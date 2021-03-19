import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ShareBusinessAdvanceSettlementInforComponent } from 'src/app/business-modules/share-business/components/advance-settlement-info/advance-settlement-info.component';

@NgModule({
    imports: [
        CommonModule,
        ShareBussinessModule,
    ],
    exports: [],
    declarations: [
    ],
    providers: [],
    entryComponents: [
        ShareBusinessAdvanceSettlementInforComponent
    ]
})
export class ShareAdvanceSettleModule {
    static rootComponent = ShareBusinessAdvanceSettlementInforComponent;
}
