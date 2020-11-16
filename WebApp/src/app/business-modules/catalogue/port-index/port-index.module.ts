import { NgModule } from '@angular/core';

import { PortIndexComponent } from './port-index.component';
import { Routes, RouterModule } from '@angular/router';
import { PortIndexImportComponent } from './port-index-import/port-index-import.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { SelectModule } from 'ng2-select';
import { NgProgressModule } from '@ngx-progressbar/core';
import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { FormPortIndexComponent } from './components/form-port-index.component';

const routing: Routes = [
    {
        path: '', data: { name: "", title: 'eFMS PortIndex' },
        children: [
            {
                path: '', component: PortIndexComponent
            },
            {
                path: 'import', component: PortIndexImportComponent, data: { name: "Import" }
            },
        ]
    },
];

@NgModule({
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        SelectModule,
        NgProgressModule,
        ModalModule.forRoot(),
        PaginationModule.forRoot(),
    ],
    exports: [],
    bootstrap: [PortIndexComponent],
    declarations: [
        PortIndexComponent,
        PortIndexImportComponent,
        FormPortIndexComponent
    ],
    providers: [],
})
export class PortIndexModule { }
