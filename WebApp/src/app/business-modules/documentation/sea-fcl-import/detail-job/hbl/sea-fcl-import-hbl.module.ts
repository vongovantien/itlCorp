import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SeaFCLImportHBLComponent } from './sea-fcl-import-hbl.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { TabsModule, ModalModule, PaginationModule } from 'ngx-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CreateHouseBillComponent } from './create/create-house-bill.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { FormAddHouseBillComponent } from './components/form-add-house-bill/form-add-house-bill.component';
import { FCLImportShareModule } from '../../share-fcl-import.module';
import { DetailHouseBillComponent } from './detail/detail-house-bill.component';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ImportHouseBillDetailComponent } from './popup/import-house-bill-detail/import-house-bill-detail.component';
import { SelectModule } from 'ng2-select';
import { FormSearchHouseBillComponent } from './components/form-search-house-bill/form-search-house-bill.component';

const routing: Routes = [
    {
        path: '', component: SeaFCLImportHBLComponent,
        data: { name: 'House Bill List', path: 'hbl', level: 4 }
    },
    {
        path: 'new', component: CreateHouseBillComponent,
        data: { name: 'New House Bill Detail', path: ':id', level: 5 }
    },
    {
        path: ':id', component: DetailHouseBillComponent,
        data: { name: 'House Bill Detail', path: ':id', level: 5 }
    }
];

@NgModule({
    declarations: [
        SeaFCLImportHBLComponent,
        CreateHouseBillComponent,
        FormAddHouseBillComponent,
        DetailHouseBillComponent,
        ImportHouseBillDetailComponent,
        FormSearchHouseBillComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        ShareBussinessModule,
        FormsModule,
        TabsModule.forRoot(),
        ReactiveFormsModule,
        RouterModule.forChild(routing),
        NgxDaterangepickerMd,
        FCLImportShareModule,
        PaginationModule.forRoot(),
        ModalModule.forRoot(),
        SelectModule

    ],
    exports: [],
    providers: [],
})
export class SeaFCLImportHBLModule { }