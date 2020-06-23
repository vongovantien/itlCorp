import { PipeModule } from "src/app/shared/pipes/pipe.module";
import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { DirectiveModule } from "src/app/shared/directives/directive.module";
import { SelectModule } from "ng2-select";
import { TabsModule, CollapseModule, ModalModule } from "ngx-bootstrap";
import { CommonComponentModule } from "src/app/shared/common/common.module";
import { NgxDaterangepickerMd } from "ngx-daterangepicker-material";
import { FormContractCommercialPopupComponent } from "./components/form-contract-commercial-catalogue.popup";
const COMPONENTS = [
    FormContractCommercialPopupComponent
]
@NgModule({
    declarations: [
        ...COMPONENTS
    ],
    imports: [
        NgxDaterangepickerMd,
        ModalModule.forRoot(),
        CommonComponentModule,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        DirectiveModule,
        PipeModule,
        SelectModule,
        TabsModule.forRoot(),
        CollapseModule.forRoot(),
        NgxDaterangepickerMd,
    ],
    exports: [
        ...COMPONENTS
    ],
    providers: [],
})

export class ShareCommercialCatalogueModule {

}
