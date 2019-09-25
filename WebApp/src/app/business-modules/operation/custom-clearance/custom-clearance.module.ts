import { NgModule } from '@angular/core';
import { CustomClearanceComponent } from './custom-clearance.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { TabsModule, ModalModule, PaginationModule } from 'ngx-bootstrap';
import { NgProgressModule } from '@ngx-progressbar/core';
import { CustomClearanceFormSearchComponent } from './components/form-search-custom-clearance/form-search-custom-clearance.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { SelectModule } from 'ng2-select';
import { CustomClearanceAddnewComponent } from './addnew/custom-clearance-addnew.component';
import { CustomClearanceEditComponent } from './detail/custom-clearance-edit.component';
import { CustomClearanceImportComponent } from './import/custom-clearance-import.component';
import { HttpClientModule } from '@angular/common/http';

const routing: Routes = [
    {
        path: "",
        component: CustomClearanceComponent,
        data: {
            name: "Custom Clearance",
            level: 2
        }
    },
    {
        path: "new",
        component: CustomClearanceAddnewComponent,
        data: {
            name: "Add Custom Clearance",
            level: 3
        }
    },
    {
        path: "detail",
        component: CustomClearanceEditComponent,
        data: {
            name: "Detail/Edit Custom Clearance",
            level: 3
        }
    },
    {
        path: "import",
        component: CustomClearanceImportComponent,
        data: {
            name: "Import Custom Clearance",
            level: 3
        }
    },
];

const LIB = [
    NgxDaterangepickerMd,
    TabsModule.forRoot(),
    ModalModule.forRoot(),
    NgProgressModule,
    PaginationModule.forRoot(),
    SelectModule
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing),
        FormsModule,
        ReactiveFormsModule,
        HttpClientModule,
        ...LIB
    ],
    exports: [],
    declarations: [
        CustomClearanceComponent,
        CustomClearanceAddnewComponent,
        CustomClearanceImportComponent,
        CustomClearanceEditComponent,
        CustomClearanceFormSearchComponent
    ],
    providers: [],
    bootstrap: [
        CustomClearanceComponent
    ]
})
export class CustomClearanceModule { }
