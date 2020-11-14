import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { SharedModule } from 'src/app/shared/shared.module';
import { EcusConnectionComponent } from './ecus-connection.component';
import { SelectModule } from 'ng2-select';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { EcusConnectionFormPopupComponent } from './form-ecus/form-ecus.component';


const routing: Routes = [
    {
        path: '', data: { name: "" }, children: [
            {
                path: '', component: EcusConnectionComponent
            },
        ]
    },
];

@NgModule({
    declarations: [
        EcusConnectionComponent,
        EcusConnectionFormPopupComponent
    ],
    imports: [
        RouterModule.forChild(routing),
        SharedModule,
        SelectModule,
        PaginationModule,
        ModalModule,
    ],
    exports: [],
    providers: [],
})
export class EcusConectionModule { }
