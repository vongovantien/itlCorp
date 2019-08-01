import { NgModule } from '@angular/core';
import { LocationImportComponent } from '../location-import/location-import.component';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { SelectModule } from 'ng2-select';
import { NgProgressModule } from '@ngx-progressbar/core';
import { FormsModule } from '@angular/forms';
import { LocationComponent } from './location.component';

const routing: Routes = [
    { path: '', component: LocationComponent, data: { name: "Location", level: 2 } },
    { path: 'location-import', component: LocationImportComponent, data: { name: "Location Import", level: 3 } },
];

@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        SelectModule,
        NgProgressModule,
        FormsModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        LocationComponent,
        LocationImportComponent
    ],
    providers: [],
})
export class LocationModule { }
