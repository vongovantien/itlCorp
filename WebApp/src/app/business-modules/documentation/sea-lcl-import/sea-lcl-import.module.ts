import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { TabsModule, PaginationModule } from 'ngx-bootstrap';

import { SeaLCLImportComponent } from './sea-lcl-import.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';
import { SeaLCLImportCreateJobComponent } from './create-job/create-job-lcl-import.component';
import { SeaLCLImportDetailJobComponent } from './detail-job/detail-job-lcl-import.component';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { SeaLCLImportLazyLoadModule } from './sea-lcl-import-lazy-load.module';


const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: SeaLCLImportComponent,
        data: { name: "Sea LCL Import", path: "sea-lcl-import", level: 2 }
    },
    {
        path: 'new', component: SeaLCLImportCreateJobComponent,
        data: { name: "Create New Job", path: "new", level: 3 }
    },
    {
        path: ':jobId', component: SeaLCLImportDetailJobComponent,
        data: { name: "Job Detail", path: ":id", level: 3, transactionType: CommonEnum.TransactionTypeEnum.SeaLCLImport },
    },
    {
        path: ':jobId/hbl', loadChildren: () => import('./detail-job/hbl/sea-lcl-import-hbl.module').then(m => m.SeaLCLImportHBLModule),
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
        SeaLCLImportLazyLoadModule, // ?  Lazy loading module with  tab component (CD Note).
    ],
    exports: [],
    declarations: [
        SeaLCLImportComponent,
        SeaLCLImportCreateJobComponent,
        SeaLCLImportDetailJobComponent,
    ],
    providers: [],
})
export class SeaLCLImportModule { }
