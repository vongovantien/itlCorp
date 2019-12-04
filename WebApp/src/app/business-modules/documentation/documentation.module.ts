import { NgModule } from '@angular/core';

import { DocumentationRoutingModule } from './documentation-routing.module';
import { AirExportComponent } from './air-export/air-export.component';
import { AirImportComponent } from './air-import/air-import.component';
import { SeaConsolExportComponent } from './sea-consol-export/sea-consol-export.component';
import { SeaConsolImportComponent } from './sea-consol-import/sea-consol-import.component';
import { InlandTruckingComponent } from './inland-trucking/inland-trucking.component';



@NgModule({
  imports: [
    DocumentationRoutingModule
  ],
  declarations: [
    AirExportComponent,
    AirImportComponent,
    SeaConsolExportComponent,
    SeaConsolImportComponent,
    InlandTruckingComponent,
  ]
})
export class DocumentationModule { }
