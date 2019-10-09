import { NgModule } from '@angular/core';
import { JobManagementComponent } from './job-management.component';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';
import { PaginationModule } from 'ngx-bootstrap';
import { NgProgressModule } from '@ngx-progressbar/core';
import { JobManagementCreateJobComponent } from './create/create-job.component';
import { JobManagementDetailJobComponent } from './detail/detail-job.component';

import { JobManagementFormSearchComponent } from './components/form-search-job/form-search-job.component';


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
        JobManagementFormSearchComponent
    ],
    providers: [
    ],
    bootstrap: [
        JobManagementComponent
    ],
})
export class JobManagementModule { }
