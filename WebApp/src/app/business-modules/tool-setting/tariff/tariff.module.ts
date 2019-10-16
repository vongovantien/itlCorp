import { NgModule } from '@angular/core';
import { TariffComponent } from './tariff.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { Routes, RouterModule } from '@angular/router';
import { TariffFormSearchComponent } from './components/form-search-tariff/form-search-tariff.component';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';

const routing: Routes = [
    {
        path: '', component: TariffComponent, data: {
            name: "Tariff",
            level: 2
        }
    }
];
@NgModule({
    declarations: [
        TariffComponent,
        TariffFormSearchComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        NgxDaterangepickerMd.forRoot(),
        RouterModule.forChild(routing)
    ],
    exports: [],
    providers: [],
})
export class TariffModule { }
