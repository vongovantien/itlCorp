import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommercialCreateComponent } from './create/create-commercial.component';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { CommercialFormCreateComponent } from './components/form-create/form-create-commercial.component';
import { CommercialContractListComponent } from './components/contract/commercial-contract-list.component';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';
import { ReactiveFormsModule } from '@angular/forms';
import { CommercialDetailComponent } from './detail/detail-commercial.component';
import { PipeModule } from 'src/app/shared/pipes/pipe.module';
import { ShareCommercialCatalogueModule } from '../share-commercial-catalogue/share-commercial-catalogue.module';

@NgModule({
    declarations: [
        CommercialCreateComponent,
        CommercialFormCreateComponent,
        //CommercialContractListComponent,
        CommercialDetailComponent
    ],
    imports: [
        CommonModule,
        CommonComponentModule,
        DirectiveModule,
        ReactiveFormsModule,
        PipeModule,
        ShareCommercialCatalogueModule
    ],
    exports: [
        CommercialCreateComponent,
        CommercialFormCreateComponent,
        CommercialContractListComponent,
        CommercialDetailComponent,

    ],
    providers: [],
})
export class ShareCommercialModule { }
