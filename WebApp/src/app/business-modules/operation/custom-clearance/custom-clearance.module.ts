import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgProgressModule } from '@ngx-progressbar/core';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from 'src/app/shared/shared.module';
import { CustomClearanceAddnewComponent } from './addnew/custom-clearance-addnew.component';
import { CustomClearanceFormDetailComponent } from './components/form-detail-clearance/form-detail-clearance.component';
import { CustomClearanceFormSearchComponent } from './components/form-search-custom-clearance/form-search-custom-clearance.component';
import { CustomClearanceComponent } from './custom-clearance.component';
import { CustomClearanceEditComponent } from './detail/custom-clearance-edit.component';
import { CustomClearanceImportComponent } from './import/custom-clearance-import.component';

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
        ...LIB
    ],
    exports: [CustomClearanceFormDetailComponent],
    declarations: [
        CustomClearanceComponent,
        CustomClearanceAddnewComponent,
        CustomClearanceImportComponent,
        CustomClearanceEditComponent,
        CustomClearanceFormSearchComponent,
        CustomClearanceFormDetailComponent
    ],
    providers: [],
})
export class CustomClearanceModule { }
