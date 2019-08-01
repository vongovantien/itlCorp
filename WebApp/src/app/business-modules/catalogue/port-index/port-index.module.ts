import { NgModule } from '@angular/core';

import { PortIndexComponent } from './port-index.component';
import { CommonModule } from '@angular/common';
import { Routes, RouterModule } from '@angular/router';
import { PortIndexImportComponent } from '../port-index-import/port-index-import.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { NgProgressModule } from '@ngx-progressbar/core';

const routing: Routes = [
    { path: '', component: PortIndexComponent, data: { name: "Port Index", level: 2 } },
    { path: 'import', component: PortIndexImportComponent, data: { name: "Port Index Import", level: 3 } },
];

@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        SharedModule,
        FormsModule,
        SelectModule,
        NgProgressModule,

    ],
    exports: [],
    bootstrap: [PortIndexComponent],
    declarations: [
        PortIndexComponent,
        PortIndexImportComponent
    ],
    providers: [],
})
export class PortIndexModule { }
