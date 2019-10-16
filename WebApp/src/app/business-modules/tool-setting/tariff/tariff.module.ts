import { NgModule } from '@angular/core';
import { TariffComponent } from './tariff.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { TariffFormSearchComponent } from './components/form-search-tariff/form-search-tariff.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { TariffAddComponent } from './add/add-tariff.component';
import { TariffFormAddComponent } from './components/form-add-tariff/form-add-tariff.component';

const routing: Routes = [
    { path: '', component: TariffComponent, data: { name: "Tariff", level: 2 } },
    { path: 'new', component: TariffAddComponent, data: { name: "New", level: 3 } },

];
@NgModule({
    declarations: [
        TariffComponent,
        TariffAddComponent,
        TariffFormSearchComponent,
        TariffFormAddComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        TabsModule.forRoot(),
        NgxDaterangepickerMd.forRoot(),
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
})
export class TariffModule { }
