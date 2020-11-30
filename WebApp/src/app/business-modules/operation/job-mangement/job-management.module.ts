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


const routing: Routes = [
    {
        path: '', data: { name: "" }, children: [
            {
                path: '', component: JobManagementComponent
            },
            {
                path: "new",
                component: JobManagementCreateJobComponent,
                data: { name: "New", }
            },
            {
                path: "job-edit",
                loadChildren: () => import('./../job-edit/job-edit.module').then(m => m.JobEditModule),
                data: { name: 'Detail Job' }
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
];

@NgModule({
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        ...LIB
    ],
    exports: [],
    declarations: [
        JobManagementComponent,
        JobManagementCreateJobComponent,
        JobManagementFormSearchComponent,
        JobManagementFormCreateComponent
    ],
    providers: [
    ],
})
export class JobManagementModule { }
