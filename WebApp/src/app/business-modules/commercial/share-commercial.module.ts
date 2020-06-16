import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CommercialCreateComponent } from './create/create-commercial.component';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { CommercialFormCreateComponent } from './components/form-create/form-create-commercial.component';
import { CommercialContractListComponent } from './components/contract/commercial-contract-list.component';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';

@NgModule({
    declarations: [
        CommercialCreateComponent,
        CommercialFormCreateComponent,
        CommercialContractListComponent
    ],
    imports: [
        CommonModule,
        CommonComponentModule,
        DirectiveModule
    ],
    exports: [
        CommercialCreateComponent,
        CommercialFormCreateComponent,
        CommercialContractListComponent
    ],
    providers: [],
})
export class ShareCommercialModule { }
