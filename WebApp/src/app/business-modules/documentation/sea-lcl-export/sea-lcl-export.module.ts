import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { TabsModule, PaginationModule } from 'ngx-bootstrap';

import { SeaLCLExportComponent } from './sea-lcl-export.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { SeaLCLExportCreateJobComponent } from './create-job/create-job-lcl-export.component';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { SeaLCLExportDetailJobComponent } from './detail-job/detail-job-lcl-export.component';

const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: SeaLCLExportComponent,
        data: { name: "Sea LCL Export", path: "sea-lcl-export", level: 2 }
    },
    {
        path: 'new', component: SeaLCLExportCreateJobComponent,
        data: { name: "Create New Job", path: "new", level: 3 }
    },
    {
        path: ':jobId', component: SeaLCLExportDetailJobComponent,
        data: { name: "Job Detail", path: ":id", level: 3, transactionType: CommonEnum.TransactionTypeEnum.SeaLCLExport },
    },
    {
        path: ':jobId/hbl', loadChildren: () => import('./detail-job/hbl/sea-lcl-export-hbl.module').then(m => m.SeaLCLExportHBLModule),
    }
];

const LIBS = [
    TabsModule.forRoot(),
    PaginationModule.forRoot(),
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
        SeaLCLExportComponent,
        SeaLCLExportCreateJobComponent,
        SeaLCLExportDetailJobComponent
    ],
    providers: [],
})
export class SeaLCLExportModule { }
