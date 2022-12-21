import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NgSelectModule } from '@ng-select/ng-select';
import { CollapseModule } from 'ngx-bootstrap/collapse';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from 'src/app/shared/shared.module';
import { AccountantFileManagementComponent } from './accountant-file-management/accountant-file-management.component';
import { FormSearchFileManagementComponent } from './components/form-search-file-management/form-search-file-management.component';
import { ListFileManagementComponent } from './components/list-file-management/list-file-management.component';
import { GeneralFileManagementComponent } from './general-file-management/general-file-management.component';

const routing: Routes = [

    {
        path: "",
        data: { name: "General", title: 'General File Management' }, redirectTo: 'general',
    },
    {
        path: 'general', component: GeneralFileManagementComponent,
        data: { name: "General", title: 'General File Management' },
    },
    {
        path: 'accountant', component: AccountantFileManagementComponent,
        data: { name: "Accountant", title: 'Accountant File Management' },
    },

    {
        path: 'user-attach-file', loadChildren: () => import('./user-attach-file-management/user-attach-file-management.module').then(m => m.UserAttachFileManagementModule),
        data: { name: 'Attach file', title: 'Attach file' }
    }
]

@NgModule({
    declarations: [
        ListFileManagementComponent,
        FormSearchFileManagementComponent,
        GeneralFileManagementComponent,
        AccountantFileManagementComponent
    ],
    exports: [],
    imports: [
        RouterModule.forChild(routing),
        TabsModule.forRoot(),
        PaginationModule.forRoot(),
        NgxDaterangepickerMd.forRoot(),
        CollapseModule.forRoot(),
        SharedModule,
        //ShareFileManagementModule,
        NgSelectModule,
    ],
    providers: [

    ]
})
export class FilesManagementModule { static routing = routing; }
