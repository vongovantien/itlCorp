import { NgModule } from '@angular/core';
import { CommercialCreateComponent } from './create/create-commercial.component';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { CommercialFormCreateComponent } from './components/form-create/form-create-commercial.component';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';
import { ReactiveFormsModule } from '@angular/forms';
import { CommercialDetailComponent } from './detail/detail-commercial.component';
import { PipeModule } from 'src/app/shared/pipes/pipe.module';
import { ShareCommercialCatalogueModule } from '../share-commercial-catalogue/share-commercial-catalogue.module';
import { SelectModule } from 'ng2-select';

import { ModalModule } from 'ngx-bootstrap/modal';
import { PaginationModule } from 'ngx-bootstrap/pagination';

import { CustomerAgentImportComponent } from './components/customer-agent-import/customer-agent-import.component';
import { ContractImportComponent } from './components/contract/import/contract-import.component';
import { TabsModule } from 'ngx-bootstrap/tabs';

@NgModule({
    declarations: [
        CommercialCreateComponent,
        CommercialFormCreateComponent,
        CommercialDetailComponent,
        CustomerAgentImportComponent,
        ContractImportComponent,

    ],
    imports: [
        CommonComponentModule,
        DirectiveModule,
        ReactiveFormsModule,
        PipeModule,
        ShareCommercialCatalogueModule,
        ModalModule.forRoot(),
        PaginationModule.forRoot(),
        SelectModule,
        TabsModule.forRoot(),
    ],
    exports: [
        CommercialCreateComponent,
        CommercialFormCreateComponent,
        CommercialDetailComponent,


    ],
    providers: [],
})
export class ShareCommercialModule { }
