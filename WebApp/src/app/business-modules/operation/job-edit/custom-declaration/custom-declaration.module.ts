import { NgModule } from '@angular/core';
import { BillingCustomDeclarationComponent } from './billing-custom-declaration.component';

import { PaginationModule } from 'ngx-bootstrap/pagination';
import { ModalModule } from 'ngx-bootstrap/modal';
import { SharedModule } from 'src/app/shared/shared.module';
import { AddMoreModalComponent } from './add-more-modal/add-more-modal.component';
import { SearchMultipleComponent } from './components/search-multiple/search-multiple.component';
import { SharedOperationModule } from '../../shared-operation.module';
import { CustomClearanceAddNewModalComponent } from './components/custom-clearance-add-new-modal/custom-clearance-add-new-modal.component';

const LIB = [
    ModalModule.forRoot(),
    PaginationModule.forRoot(),
];

@NgModule({
    declarations: [
        BillingCustomDeclarationComponent,
        AddMoreModalComponent,
        SearchMultipleComponent,
        CustomClearanceAddNewModalComponent,
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
