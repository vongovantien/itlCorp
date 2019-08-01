import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';

import { WarehouseComponent } from './warehouse.component';
import { WarehouseImportComponent } from '../warehouse-import/warehouse-import.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { NgProgressModule } from '@ngx-progressbar/core';
import { SelectModule } from 'ng2-select';


const routing: Routes = [
    { path: '', component: WarehouseComponent, data: { name: "Ware House", level: 2 } },
    { path: 'import', component: WarehouseImportComponent, data: { name: "Ware House Import", level: 3 } },

];
@NgModule({
    imports: [
        CommonModule,
        RouterModule.forChild(routing),
        SharedModule,
        FormsModule,
        NgProgressModule,
        SelectModule
    ],
    declarations: [
        WarehouseComponent,
        WarehouseImportComponent
    ],
    exports: [],
    bootstrap: [WarehouseComponent],
    providers: [],
})
export class WareHouseModule { 
}
