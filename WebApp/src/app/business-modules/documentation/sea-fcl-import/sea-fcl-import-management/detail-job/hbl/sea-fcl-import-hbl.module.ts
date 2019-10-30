import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SeaFCLImportHBLComponent } from './sea-fcl-import-hbl.component';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { TabsModule } from 'ngx-bootstrap';

const routing: Routes = [
    {
        path: '', component: SeaFCLImportHBLComponent,
        data: { name: 'House Bill List', path: 'hbl', level: 4 }
    },
    {
        path: 'new', component: SeaFCLImportHBLComponent,
        data: { name: 'New House Bill Detail', path: ':id', level: 5 }
    },
    {
        path: ':id', component: SeaFCLImportHBLComponent,
        data: { name: 'House Bill Detail', path: ':id', level: 5 }
    }
];



const COMPONENTS = [

];

@NgModule({
    declarations: [
        SeaFCLImportHBLComponent,
        ...COMPONENTS
    ],
    imports: [
        CommonModule,
        SharedModule,
        TabsModule.forRoot(),
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
})
export class SeaFCLImportHBLModule { }