import { NgModule } from '@angular/core';

import { SeaFCLExportComponent } from './sea-fcl-export.component';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'ng2-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';

const routing: Routes = [
    {
        path: '', pathMatch: 'full', component: SeaFCLExportComponent,
        data: { name: "Sea FCL Export", path: "sea-fcl-export", level: 2 }
    },

];

const LIB = [
    SelectModule,
    NgxDaterangepickerMd.forRoot(),
    PerfectScrollbarModule
];

const COMPONENTS = [

]

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        RouterModule.forChild(routing),
        FormsModule,
        ...LIB
    ],
    exports: [],
    declarations: [
        SeaFCLExportComponent
    ],
    providers: [],
    bootstrap: [
        SeaFCLExportComponent
    ]
})
export class SeaFCLExportModule { }
