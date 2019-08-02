import { NgModule } from '@angular/core';
import { CommodityComponent } from './commodity.component';
import { Routes, RouterModule } from '@angular/router';
import { CommodityImportComponent } from '../commodity-import/commodity-import.component';
import { CommodityGroupImportComponent } from '../commodity-group-import/commodity-group-import.component';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { SelectModule } from 'ng2-select';
import { FormsModule } from '@angular/forms';
import { NgProgressModule } from '@ngx-progressbar/core';

const routing: Routes = [
    { path: '', component: CommodityComponent, data: { name: "Commodity", level: 2 } },
    { path: 'commodity-import', component: CommodityImportComponent, data: { name: "Commodity Import", level: 3 } },
    { path: 'commodity-group-import', component: CommodityGroupImportComponent, data: { name: "Commodity Group Import", level: 3 } },
];
@NgModule({
    imports: [
        CommonModule,
        SharedModule,
        SelectModule,
        FormsModule,
        NgProgressModule,
        RouterModule.forChild(routing)
    ],
    exports: [],
    declarations: [
        CommodityComponent,
        CommodityImportComponent,
        CommodityGroupImportComponent
    ],
    providers: [],
})
export class CommondityModule { }
