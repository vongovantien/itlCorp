import { NgModule } from '@angular/core';
import { BillingCustomDeclarationComponent } from './billing-custom-declaration.component';

import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { SharedModule } from 'src/app/shared/shared.module';
import { SharedOperationModule } from '../../shared-operation.module';
import { AddMoreModalComponent } from './add-more-modal/add-more-modal.component';
import { AddNewModalComponent } from './add-new-modal/add-new-modal.component';
import { SearchMultipleComponent } from './components/search-multiple/search-multiple.component';


const LIB = [
    ModalModule.forRoot(),
    PaginationModule.forRoot(),
];

@NgModule({
    declarations: [
        BillingCustomDeclarationComponent,
        AddMoreModalComponent,
        SearchMultipleComponent,
        AddNewModalComponent,
    ],
    exports: [],
    providers: [],
    entryComponents: [
        BillingCustomDeclarationComponent
    ],
    imports: [
        SharedModule,
        SharedOperationModule,
        ...LIB,
    ]
})
export class CustomDeclarationModule {
    static rootComponent = BillingCustomDeclarationComponent;
}
