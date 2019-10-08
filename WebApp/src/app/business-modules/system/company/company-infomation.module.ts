import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { ComanyInfomationComponent } from './company-infomation.component';
import { CompanyInfomationFormSearchComponent } from './components/form-search-company/form-search-company.component';
import { FormsModule } from '@angular/forms';
import { PaginationModule } from 'ngx-bootstrap';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';
import { CompanyInfomationDetailComponent } from './detail/detail-company-infomation.component';

const routing: Routes = [
    {
        path: "", component: ComanyInfomationComponent, pathMatch: 'full',
        data: { name: "Company Infomation", path: "company-infomation", level: 2 }
    },
    {
        path: "new", component: ComanyInfomationComponent,
        data: { name: "New", path: "New", level: 3 }
    },
    {
        path: ":id", component: CompanyInfomationDetailComponent,
        data: { name: "Detail", path: "Detail", level: 3 }
    }
];

@NgModule({
    declarations: [
        ComanyInfomationComponent,
        CompanyInfomationFormSearchComponent,
        CompanyInfomationDetailComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        PaginationModule,
        PerfectScrollbarModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
    bootstrap: [
        ComanyInfomationComponent
    ]
})
export class CompanyInfomationModule { }
