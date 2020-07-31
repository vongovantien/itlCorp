import { NgModule } from '@angular/core';

import { DocumentationRoutingModule } from './documentation-routing.module';
import { InlandTruckingComponent } from './inland-trucking/inland-trucking.component';
import { SeaConsolImportComponent } from './sea-consol-import/sea-consol-import.component';

@NgModule({
  imports: [
    DocumentationRoutingModule,
  ],
  declarations: [
    InlandTruckingComponent,
    SeaConsolImportComponent

  ]
})
export class DocumentationModule { }
