import { NgModule, LOCALE_ID } from '@angular/core';
import { JobManagementComponent } from './job-management.component';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule, registerLocaleData } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';
import { ModalModule, PaginationModule } from 'ngx-bootstrap';
import { NgProgressModule } from '@ngx-progressbar/core';
import { JobManagementCreateJobComponent } from './create/create-job.component';
import { JobManagementDetailJobComponent } from './detail/detail-job.component';

import { JobManagementFormSearchComponent } from './components/form-search-job/form-search-job.component';
import localeVi from '@angular/common/locales/vi';

registerLocaleData(localeVi, 'vi');

const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: JobManagementComponent, data: {
            name: "Job Management",
            level: 2
        }
    },
    {
        path: "job-create",
        component: JobManagementCreateJobComponent,
        data: {
            name: "Job Create",
            level: 3
        }
    },
    {
        path: "job-edit",
        loadChildren: () => import('./../job-edit/job-edit.module').then(m => m.JobEditModule),
    },
];


const LIB = [
    NgxDaterangepickerMd,
    SelectModule,
    NgProgressModule,
    PaginationModule.forRoot(),
    ModalModule.forRoot()
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        ...LIB
    ],
    exports: [],
    declarations: [
        JobManagementComponent,
        JobManagementCreateJobComponent,
        JobManagementDetailJobComponent,
        JobManagementFormSearchComponent
    ],
    providers: [
        { provide: LOCALE_ID, useValue: 'vi' },
    ],
    bootstrap: [
        JobManagementComponent
    ],
})
export class JobManagementModule { }
