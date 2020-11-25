import { NgModule } from '@angular/core';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareSeaServiceFormCreateHouseBillSeaExportComponent } from './components/form-create-hbl-sea-export/form-create-hbl-sea-export.component';
import { ShareSeaServiceFormCreateSeaExportComponent } from './components/form-create-sea-export/form-create-sea-export.component';
import { ShareSeaServiceFormCreateSeaImportComponent } from './components/form-create-sea-import/form-create-sea-import.component';
import { ShareSeaServiceFormSISeaExportComponent } from './components/form-si-sea-export/form-si-sea-export.component';


const COMPONENTS = [
    ShareSeaServiceFormCreateSeaExportComponent,
    ShareSeaServiceFormCreateSeaImportComponent,
    ShareSeaServiceFormSISeaExportComponent,
    ShareSeaServiceFormCreateHouseBillSeaExportComponent
];

@NgModule({
    declarations: [
        ...COMPONENTS
    ],
    imports: [
        SharedModule,
        NgxDaterangepickerMd,
        NgSelectModule
    ],
    exports: [
        ...COMPONENTS
    ],
    providers: [],
})
export class ShareSeaServiceModule { }
