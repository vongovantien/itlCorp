import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { AirExportComponent } from './air-export/air-export.component';
import { AirImportComponent } from './air-import/air-import.component';
import { InlandTruckingComponent } from './inland-trucking/inland-trucking.component';
import { SeaConsolExportComponent } from './sea-consol-export/sea-consol-export.component';
import { SeaConsolImportComponent } from './sea-consol-import/sea-consol-import.component';
import { SeaFCLExportComponent } from './sea-fcl-export/sea-fcl-export.component';
import { SeaLCLExportComponent } from './sea-lcl-export/sea-lcl-export.component';
import { SeaLCLImportComponent } from './sea-lcl-import/sea-lcl-import.component';
import { SeaFclExportCreateComponent } from './sea-fcl-export-create/sea-fcl-export-create.component';
import { ManifestComponent } from './sea-fcl-export-create/manifest/manifest.component';
import { ShippingInstructionComponent } from './sea-fcl-export-create/shipping-instruction/shipping-instruction.component';
import { SeaLclExportCreateComponent } from './sea-lcl-export-create/sea-lcl-export-create.component';
import { SeaLclExportHousebillAddnewComponent } from './sea-lcl-export-create/sea-lcl-export-housebill-addnew/sea-lcl-export-housebill-addnew.component';
import { SeaLclExportManifestComponent } from './sea-lcl-export-create/sea-lcl-export-manifest/sea-lcl-export-manifest.component';
import { SeaLclExportShippingInstructionComponent } from './sea-lcl-export-create/sea-lcl-export-shipping-instruction/sea-lcl-export-shipping-instruction.component';
import { SeaFCLImportManagementComponent } from './sea-fcl-import/sea-fcl-import-management/sea-fcl-import-management.component';

const routes: Routes = [

    {
        path: '',
        redirectTo: 'inland-trucking',
        pathMatch: 'full'
    },
    {
        path: 'inland-trucking',
        component: InlandTruckingComponent,
        data: {
            name: "Inland Trucking",
            level: 2
        },
    },
    {
        path: 'air-export',
        component: AirExportComponent,
        data: {
            name: "Air Export",
            level: 2
        }
    },
    {
        path: 'air-import',
        component: AirImportComponent,
        data: {
            name: "Air Import",
            level: 2
        }
    },
    {
        path: 'sea-consol-export',
        component: SeaConsolExportComponent,
        data: {
            name: "Sea Consol Export",
            level: 2
        }
    },
    {
        path: 'sea-consol-import',
        component: SeaConsolImportComponent,
        data: {
            name: "Sea Consol Import",
            level: 2
        }
    },
    {
        path: 'sea-fcl-export',
        component: SeaFCLExportComponent,
        data: {
            name: "Sea FCL Export",
            level: 2
        }
    },
    {
        path: 'sea-fcl-export-create',
        component: SeaFclExportCreateComponent,
        data: {
            name: "Sea FCL Create",
            level: 3
        }
    },
    {
        path: 'manifest',
        component: ManifestComponent,
        data: {
            name: "Manifest",
            level: 4
        }
    },
    {
        path: 'shipping-instruction',
        component: ShippingInstructionComponent,
        data: {
            name: "Shipping Instruction",
            level: 4
        }
    },
    {
        path: 'sea-fcl-import',
        component: SeaFCLImportManagementComponent,
        data: {
            name: "Sea FCL Import",
            level: 2
        }
    },
    {
        path: 'sea-lcl-export',
        component: SeaLCLExportComponent,
        data: {
            name: "Sea LCL Export",
            level: 2
        }
    },
    {
        path: 'sea-lcl-export-create',
        component: SeaLclExportCreateComponent,
        data: {
            name: "Sea LCL Export Create",
            level: 3
        }
    },
    {
        path: 'sea-lcl-export-manifest',
        component: SeaLclExportManifestComponent,
        data: {
            name: "Sea LCL Export Manifest",
            level: 4
        }
    },
    {
        path: 'sea-lcl-export-shipping-instruction',
        component: SeaLclExportShippingInstructionComponent,
        data: {
            name: "Sea LCL Export Shipping Instruction",
            level: 4
        }
    },

    {
        path: 'sea-lcl-export-housebill-addnew',
        component: SeaLclExportHousebillAddnewComponent
    },
    {
        path: 'sea-lcl-import',
        component: SeaLCLImportComponent,
        data: {
            name: "Sea LCL Import",
            level: 2
        }
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class DocumentationRoutingModule { }
