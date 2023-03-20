import { NgModule } from '@angular/core';
import { CustomClearanceComponent } from './custom-clearance.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { NgProgressModule } from '@ngx-progressbar/core';
import { CustomClearanceFormSearchComponent } from './components/form-search-custom-clearance/form-search-custom-clearance.component';
import { CustomClearanceAddnewComponent } from './addnew/custom-clearance-addnew.component';
import { CustomClearanceEditComponent } from './detail/custom-clearance-edit.component';
import { CustomClearanceImportComponent } from './import/custom-clearance-import.component';
import { CustomClearanceFromEcus } from './getecus/get-custom-clearance-from-Ecus.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { SharedOperationModule } from '../shared-operation.module';


const routing: Routes = [
    {
        path: "", data: { name: "", }, children: [
            { path: '', component: CustomClearanceComponent },
            { path: "new", component: CustomClearanceAddnewComponent, data: { name: "New", } },
            { path: "detail/:id", component: CustomClearanceEditComponent, data: { name: "Detail", } },
            { path: "import", component: CustomClearanceImportComponent, data: { name: "Import", } },
        ]
    },

];

const LIB = [
    NgxDaterangepickerMd,
    TabsModule.forRoot(),
    ModalModule.forRoot(),
    NgProgressModule,
    PaginationModule.forRoot(),
    NgSelectModule
];

@NgModule({
    imports: [
        SharedModule,
        RouterModule.forChild(routing),
        ...LIB,
        SharedOperationModule
    ],
    exports: [],
    declarations: [
        CustomClearanceComponent,
        CustomClearanceAddnewComponent,
        CustomClearanceImportComponent,
        CustomClearanceEditComponent,
        CustomClearanceFormSearchComponent,
        CustomClearanceFromEcus,
    ],
    providers: [],
})
export class CustomClearanceModule { }
