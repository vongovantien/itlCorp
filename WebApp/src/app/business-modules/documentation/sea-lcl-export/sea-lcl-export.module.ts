import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { TabsModule, PaginationModule } from 'ngx-bootstrap';

import { SeaLCLExportComponent } from './sea-lcl-export.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';

const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: SeaLCLExportComponent,
        data: { name: "Sea LCL Export", path: "sea-lcl-export", level: 2 }
    },
    // {
    //     path: 'new', component: SeaLCLImportCreateJobComponent,
    //     data: { name: "Create New Job", path: "new", level: 3 }
    // },
    // {
    //     path: ':jobId', component: SeaLCLImportDetailJobComponent,
    //     data: { name: "Job Detail", path: ":id", level: 3, transactionType: CommonEnum.TransactionTypeEnum.SeaLCLImport },
    // },
    // {
    //     path: ':jobId/hbl', loadChildren: () => import('./detail-job/hbl/sea-lcl-import-hbl.module').then(m => m.SeaLCLImportHBLModule),
    // }
];

const LIBS = [
    TabsModule.forRoot(),
    PaginationModule.forRoot(),
    PerfectScrollbarModule,
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing),
        FormsModule,
        ReactiveFormsModule,
        ShareBussinessModule,
        ...LIBS,
    ],
    exports: [],
    declarations: [
        SeaLCLExportComponent
    ],
    providers: [],
})
export class SeaLCLExportModule { }
