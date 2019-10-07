import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { ComanyInfomationComponent } from './company-infomation.component';

const routing: Routes = [
    {
        path: "", component: ComanyInfomationComponent, pathMatch: 'full',
        data: { name: "Company Infomation", path: "company-infomation", level: 2 }
    },
    {
        path: "new", component: ComanyInfomationComponent,
        data: { name: "New", path: "New", level: 3 }
    },
];

@NgModule({
    declarations: [
        ComanyInfomationComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
    bootstrap: [
        ComanyInfomationComponent
    ]
})
export class CompanyInfomationModule { }
