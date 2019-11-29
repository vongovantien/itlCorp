import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";

import { RepositoryModule } from "./repositories/repository.module";

import { CommonComponentModule } from "./common/common.module";
import { DirectiveModule } from "./directives/directive.module";
import { PipeModule } from "./pipes/pipe.module";
import { FormValidators } from "./validators";


const APP_MODULES = [
  DirectiveModule,
  CommonComponentModule,
  PipeModule,

];
@NgModule({
  imports: [
    CommonModule,
    RepositoryModule,
    FormsModule,
    ...APP_MODULES
  ],
  declarations: [
  ],
  exports: [
    ...APP_MODULES
  ],
  providers: [
    FormValidators
  ]
})
export class SharedModule { }
