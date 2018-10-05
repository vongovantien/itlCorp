import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DocumentationRoutingModule } from './documentation-routing.module';
import { AirExportComponent } from './air-export/air-export.component';
import { AirImportComponent } from './air-import/air-import.component';
import { SeaFCLExportComponent } from './sea-fcl-export/sea-fcl-export.component';
import { SeaLCLExportComponent } from './sea-lcl-export/sea-lcl-export.component';
import { SeaConsolExportComponent } from './sea-consol-export/sea-consol-export.component';
import { SeaFCLImportComponent } from './sea-fcl-import/sea-fcl-import.component';
import { SeaLCLImportComponent } from './sea-lcl-import/sea-lcl-import.component';
import { SeaConsolImportComponent } from './sea-consol-import/sea-consol-import.component';
import { InlandTruckingComponent } from './inland-trucking/inland-trucking.component';

@NgModule({
  imports: [
    CommonModule,
    DocumentationRoutingModule
  ],
  declarations: [AirExportComponent, AirImportComponent, SeaFCLExportComponent, SeaLCLExportComponent, SeaConsolExportComponent, SeaFCLImportComponent, SeaLCLImportComponent, SeaConsolImportComponent, InlandTruckingComponent]
})
export class DocumentationModule { }
