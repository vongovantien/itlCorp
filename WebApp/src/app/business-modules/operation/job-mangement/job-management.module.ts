import { NgModule } from '@angular/core';
import { JobManagementComponent } from './job-management.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { CollapseModule } from 'ngx-bootstrap/collapse';

import { NgProgressModule } from '@ngx-progressbar/core';
import { JobManagementCreateJobComponent } from './create/create-job.component';
import { JobManagementFormSearchComponent } from './components/form-search-job/form-search-job.component';
import { JobManagementFormCreateComponent } from './components/form-create/form-create-job.component';
import { NgSelectModule } from '@ng-select/ng-select';
import { JobManagementChargeImportComponent } from './job-management-import/job-charge-import.component';
import { JobManagementLinkFeeComponent } from '../job-management-link-fee/job-management-link-fee.component';
import { LinkChargeJobRepPopupComponent } from './components/popup/link-charge-from-jobRep-popup/link-charge-from-job-rep.popup';
import { ModalModule } from 'ngx-bootstrap/modal';
import { ShareBussinessModule } from '../../share-business/share-bussines.module';


const routing: Routes = [
    {
        path: '', data: { name: "" }, children: [
            {
                path: '', component: JobManagementComponent
            },

            {
                path: "new",
                component: JobManagementCreateJobComponent,
                data: { name: "New", transactionType: null }
            },
            {
                path: "new-trucking-inland",
                component: JobManagementCreateJobComponent,
                data: { name: "New", transationType: 'TKI' }
            },
            {
                path: 'import-charge', component: JobManagementChargeImportComponent, data: { name: "Import Logistics Charge", transactionType: null }
            },
            {
                path: 'import-charge-trucking-inland', component: JobManagementChargeImportComponent, data: { name: "Import Trucking Inland Charge", transactionType: 'TK' }
            },
            {
                path: "job-edit",
                loadChildren: () => import('./../job-edit/job-edit.module').then(m => m.JobEditModule),
                data: { name: 'Detail Job', tabSurcharge: 'BUY', allowLinkFee: true, transactionType: null }
            },
            {
                path: "trucking-inland-edit",
                loadChildren: () => import('./../job-edit/job-edit.module').then(m => m.JobEditModule),
                data: { name: 'Detail Job', tabSurcharge: 'BUY', allowLinkFee: true, transactionType: 'TKI' }
            },
            {
                path: "job-edit-link-fee",
                loadChildren: () => import('../job-edit/job-edit.module').then(m => m.JobEditModule),
                data: { name: 'Detail Job', tabSurcharge: 'SELL', allowLinkFee: true }
            },

        ]
    },


];

const LIB = [
    NgxDaterangepickerMd,
    NgSelectModule,
    NgProgressModule,
    PaginationModule.forRoot(),
    TabsModule.forRoot(),
    CollapseModule.forRoot(),
    ModalModule,
];

@NgModule({
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        ShareBussinessModule,
        ...LIB
    ],
    exports: [],
    declarations: [
        JobManagementChargeImportComponent,
        JobManagementComponent,
        JobManagementCreateJobComponent,
        JobManagementFormSearchComponent,
        JobManagementFormCreateComponent,
        JobManagementLinkFeeComponent,
        LinkChargeJobRepPopupComponent
    ],
    providers: [
    ],
})
export class JobManagementModule { }
