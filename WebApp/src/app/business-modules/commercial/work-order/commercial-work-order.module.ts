import { NgModule } from '@angular/core';
import { CommercialWorkOrderComponent } from './commercial-work-order.component';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { NgSelectModule } from '@ng-select/ng-select';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { CommonModule } from '@angular/common';
import { CommercialFormSearchWorkOrderComponent } from './components/form-search/form-search-work-order.component';
import { CommercialCreateWorkOrderComponent } from './create-work-order/create-work-order.component';
import { CommercialFormCreateWorkOrderComponent } from './components/form-create/form-create-work-order.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { CommercialPriceListWorkOrderComponent } from './components/price-list/price-list-work-order.component';
import { CommercialPriceItemWorkOrderPopupComponent } from './components/popup/price-item/price-item-work-order.component';
import { ModalModule } from 'ngx-bootstrap/modal';
import { CommercialSurchargeListWorkOrderComponent } from './components/surcharge-list/surcharge-list-work-order.component';
import { StoreModule } from '@ngrx/store';
import { reducers } from './store/reducers';
import { TooltipModule } from 'ngx-bootstrap/tooltip';
import { EffectsModule } from '@ngrx/effects';
import { effects } from './store';

const routing: Routes = [
    {
        path: '', data: { name: "" }, children: [
            {
                path: '', component: CommercialWorkOrderComponent,
            },
            { path: 'new/:transactionType', component: CommercialCreateWorkOrderComponent, data: { name: 'New' } },
        ]
    }
];

@NgModule({
    declarations: [
        CommercialWorkOrderComponent,
        CommercialCreateWorkOrderComponent,
        CommercialPriceListWorkOrderComponent,
        CommercialFormSearchWorkOrderComponent,
        CommercialFormCreateWorkOrderComponent,
        CommercialSurchargeListWorkOrderComponent,
        CommercialPriceItemWorkOrderPopupComponent,
    ],
    imports: [
        CommonModule,
        SharedModule,
        NgSelectModule,
        TabsModule.forRoot(),
        TooltipModule.forRoot(),
        RouterModule.forChild(routing),
        PaginationModule.forRoot(),
        NgxDaterangepickerMd.forRoot(),
        NgSelectModule,
        ModalModule.forChild(),
        StoreModule.forFeature('work-order', reducers),
        EffectsModule.forFeature(effects),

    ],
    exports: [],
    providers: [],
})
export class CommercialWorkOrderModule { }