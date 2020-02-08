import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { CommonComponentModule } from "src/app/shared/common/common.module";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { DirectiveModule } from "src/app/shared/directives/directive.module";
import { PipeModule } from "src/app/shared/pipes/pipe.module";
import { SelectModule } from "ng2-select";
import { EffectsModule } from "@ngrx/effects";
import { effects } from "../system/company/store";
import { CollapseModule, TabsModule } from "ngx-bootstrap";
import { ShareSystemAddUserComponent } from "./components/add-user/add-user.component";
import { ShareSystemDetailPermissionComponent } from "./components/permission/permission-detail.component";
import { PermissionFormCreateComponent } from "../system/permission/components/form-create-permission/form-create-permission.component";
const COMPONENTS = [
    ShareSystemAddUserComponent,
    ShareSystemDetailPermissionComponent,
    PermissionFormCreateComponent,

]
@NgModule({
    declarations: [
        ...COMPONENTS
    ],
    imports: [
        CommonModule,
        CommonComponentModule,
        FormsModule,
        ReactiveFormsModule,
        DirectiveModule,
        PipeModule,
        SelectModule,
        TabsModule.forRoot(),
        EffectsModule.forFeature(effects),
        CollapseModule.forRoot(),
    ],
    exports: [
        ...COMPONENTS
    ],
    providers: [],
})

export class ShareSystemModule {


}
