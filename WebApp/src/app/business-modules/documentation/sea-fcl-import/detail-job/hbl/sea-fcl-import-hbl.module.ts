import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SeaFCLImportHBLComponent } from './sea-fcl-import-hbl.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { TabsModule, ModalModule, PaginationModule, BsDropdownModule } from 'ngx-bootstrap';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CreateHouseBillComponent } from './create/create-house-bill.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { FormAddHouseBillComponent } from './components/form-add-house-bill/form-add-house-bill.component';
import { DetailHouseBillComponent } from './detail/detail-house-bill.component';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ImportHouseBillDetailComponent } from './popup/import-house-bill-detail/import-house-bill-detail.component';
import { SelectModule } from 'ng2-select';
import { FormSearchHouseBillComponent } from './components/form-search-house-bill/form-search-house-bill.component';
import { SeaFClImportArrivalNoteComponent } from './components/arrival-note/arrival-note.component';
import { FroalaViewModule, FroalaEditorModule } from 'angular-froala-wysiwyg';
import { SeaFClImportDeliveryOrderComponent } from './components/delivery-order/delivery-order.component';
import { ChargeConstants } from 'src/constants/charge.const';

const routing: Routes = [
    {
        path: '', component: SeaFCLImportHBLComponent,
        data: <CommonInterface.IDataParam>{ name: 'House Bill List', path: 'hbl', level: 4, serviceId: ChargeConstants.SFI_CODE }
    },
    {
        path: 'new', component: CreateHouseBillComponent,
        data: { name: 'New House Bill Detail', path: ':id', level: 5 }
    },
    {
        path: ':hblId', component: DetailHouseBillComponent,
        data: { name: 'House Bill Detail', path: ':id', level: 5 }
    }
];

const LIB = [
    PaginationModule.forRoot(),
    ModalModule.forRoot(),
    SelectModule,
    FroalaEditorModule.forRoot(),
    FroalaViewModule.forRoot(),
    NgxDaterangepickerMd,
    TabsModule.forRoot(),
    BsDropdownModule.forRoot()
];


@NgModule({
    declarations: [
        SeaFCLImportHBLComponent,
        CreateHouseBillComponent,
        FormAddHouseBillComponent,
        DetailHouseBillComponent,
        ImportHouseBillDetailComponent,
        FormSearchHouseBillComponent,
        SeaFClImportArrivalNoteComponent,
        SeaFClImportDeliveryOrderComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        ShareBussinessModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule.forChild(routing),
        ...LIB

    ],
    exports: [],
    providers: [],
    bootstrap: [
        SeaFCLImportHBLComponent
    ]
})
export class SeaFCLImportHBLModule { }
