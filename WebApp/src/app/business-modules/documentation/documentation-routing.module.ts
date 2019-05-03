import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AirExportComponent } from './air-export/air-export.component';
import { AirImportComponent } from './air-import/air-import.component';
import { InlandTruckingComponent } from './inland-trucking/inland-trucking.component';
import { SeaConsolExportComponent } from './sea-consol-export/sea-consol-export.component';
import { SeaConsolImportComponent } from './sea-consol-import/sea-consol-import.component';
import { SeaFCLExportComponent } from './sea-fcl-export/sea-fcl-export.component';
import { SeaFCLImportComponent } from './sea-fcl-import/sea-fcl-import.component';
import { SeaLCLExportComponent } from './sea-lcl-export/sea-lcl-export.component';
import { SeaLCLImportComponent } from './sea-lcl-import/sea-lcl-import.component';
import { SeaFclExportCreateComponent } from './sea-fcl-export-create/sea-fcl-export-create.component';
import { ManifestComponent } from './sea-fcl-export-create/manifest/manifest.component';
import { ShippingInstructionComponent } from './sea-fcl-export-create/shipping-instruction/shipping-instruction.component';
import { SeaLclExportCreateComponent } from './sea-lcl-export-create/sea-lcl-export-create.component'; 
import { SeaLclExportHousebillAddnewComponent } from './sea-lcl-export-create/sea-lcl-export-housebill-addnew/sea-lcl-export-housebill-addnew.component'; 
import { SeaLclExportManifestComponent } from './sea-lcl-export-create/sea-lcl-export-manifest/sea-lcl-export-manifest.component';
import { SeaLclExportShippingInstructionComponent } from './sea-lcl-export-create/sea-lcl-export-shipping-instruction/sea-lcl-export-shipping-instruction.component';

const routes: Routes = [
  {
    path:'',
    redirectTo:'inland-trucking',
    pathMatch:'full'
  },
  {
    path:'inland-trucking',
    component:InlandTruckingComponent
  },
  {
    path:'air-export',
    component:AirExportComponent
  },
  {
    path:'air-import',
    component:AirImportComponent
  },
  {
    path:'sea-consol-export',
    component:SeaConsolExportComponent
  },
  {
    path:'sea-consol-import',
    component:SeaConsolImportComponent
  },
  {
    path:'sea-fcl-export',
    component:SeaFCLExportComponent
  },
  {
    path:'sea-fcl-export-create',
    component:SeaFclExportCreateComponent
  },
  {
    path:'manifest',
    component:ManifestComponent
  },
  {
    path:'shipping-instruction',
    component:ShippingInstructionComponent
  },
  {
    path:'sea-fcl-import',
    component:SeaFCLImportComponent
  },
  {
    path:'sea-lcl-export',
    component:SeaLCLExportComponent
  },
  {
    path:'sea-lcl-export-create',
    component:SeaLclExportCreateComponent
  },
  {
    path:'sea-lcl-export-manifest',
    component:SeaLclExportManifestComponent
  },
  {
    path:'sea-lcl-export-shipping-instruction',
    component:SeaLclExportShippingInstructionComponent
  },
  {
    path:'sea-lcl-export-housebill-addnew',
    component:SeaLclExportHousebillAddnewComponent
  },
  {
    path:'sea-lcl-import',
    component:SeaLCLImportComponent
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DocumentationRoutingModule { }
