import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalModule, BsDropdownModule } from 'ngx-bootstrap';
import { FormsModule } from '@angular/forms';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';
import { ShareBussinessBuyingChargeComponent } from './components';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { NgxCurrencyModule } from 'ngx-currency';
import { StoreModule } from '@ngrx/store';
import { reducers, effects } from './store';
import { EffectsModule } from '@ngrx/effects';
import { PipeModule } from 'src/app/shared/pipes/pipe.module';
import { ShareBussinessSellingChargeComponent } from './components/selling-charge/selling-charge.component';
import { ShareBussinessOBHChargeComponent } from './components/obh-charge/obh-charge.component';

const COMPONENTS = [
    ShareBussinessBuyingChargeComponent,
    ShareBussinessSellingChargeComponent,
    ShareBussinessOBHChargeComponent
];

const customCurrencyMaskConfig = {
    align: "right",
    allowNegative: false,
    allowZero: true,
    decimal: ".",
    precision: 0,
    prefix: "",
    suffix: "",
    thousands: ",",
    nullable: true
};

@NgModule({
    declarations: [
        ...COMPONENTS
    ],
    imports: [
        CommonModule,
        CommonComponentModule,
        FormsModule,
        ModalModule.forRoot(),
        NgxDaterangepickerMd.forRoot(),
        DirectiveModule,
        PipeModule,
        BsDropdownModule.forRoot(),
        NgxCurrencyModule.forRoot(customCurrencyMaskConfig),
        StoreModule.forFeature('share-bussiness', reducers),
        EffectsModule.forFeature(effects),

    ],
    exports: [
        ...COMPONENTS
    ],
    providers: [],
})
export class ShareBussinessModule {

}