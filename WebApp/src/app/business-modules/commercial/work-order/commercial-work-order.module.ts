import { NgModule } from '@angular/core';
import { CommercialWorkOrderComponent } from './commercial-work-order.component';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { NgSelectModule } from '@ng-select/ng-select';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { CommonModule } from '@angular/common';

const routing: Routes = [
    {
        path: '', data: { name: "" }, children: [
            {
                path: '', component: CommercialWorkOrderComponent
            },
        ]
    }
];

@NgModule({
    declarations: [
        CommercialWorkOrderComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        NgSelectModule,
        TabsModule.forRoot(),
        RouterModule.forChild(routing),
        PaginationModule.forRoot()
    ],
    exports: [],
    providers: [],
})
export class CommercialWorkOrderModule { }