import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { PaginationModule, TabsModule } from 'ngx-bootstrap';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { ComanyInformationComponent } from './company-information.component';
import { CompanyInformationAddComponent } from './add/add-company-information.componnt';
import { CompanyInformationDetailComponent } from './detail/detail-company-information.component';
import { CompanyInformationFormSearchComponent } from './components/form-search-company/form-search-company.component';
import { CompanyInformationFormAddComponent } from './components/form-add-company/form-add-company.component';
import { FroalaEditorModule, FroalaViewModule } from 'angular-froala-wysiwyg';

const routing: Routes = [
    {
        path: "", component: ComanyInformationComponent, pathMatch: 'full',
        data: { name: "Company Information", path: "company-information", level: 2 }
    },
    {
        path: "new", component: CompanyInformationAddComponent,
        data: { name: "New", path: "New", level: 3 }
    },
    {
        path: ":id", component: CompanyInformationDetailComponent,
        data: { name: "Detail", path: "Detail", level: 3 }
    }
];

@NgModule({
    declarations: [
        ComanyInformationComponent,
        CompanyInformationFormSearchComponent,
        CompanyInformationFormAddComponent,
        CompanyInformationDetailComponent,
        CompanyInformationAddComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        TabsModule.forRoot(),
        PaginationModule.forRoot(),
        ReactiveFormsModule,
        PerfectScrollbarModule,
        RouterModule.forChild(routing),
        FroalaEditorModule.forRoot(),
        FroalaViewModule.forRoot(),
    ],
    exports: [],
    providers: [],
    bootstrap: [
        ComanyInformationComponent
    ]
})
export class CompanyInformationModule { }
