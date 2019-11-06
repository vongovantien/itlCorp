import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SeaFCLImportShipmentGoodSummaryComponent } from './components/shipment-good-summary/shipment-good-summary.component';
import { SeaFCLImportContainerListPopupComponent } from './components/popup/container-list/container-list.popup';
import { FormsModule } from '@angular/forms';
import { ModalModule } from 'ngx-bootstrap';
import { SharedModule } from 'src/app/shared/shared.module';

const COMPONENTS = [
    SeaFCLImportShipmentGoodSummaryComponent,
    SeaFCLImportContainerListPopupComponent
];

@NgModule({
    declarations: [
        ...COMPONENTS
    ],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        ModalModule.forRoot(),
    ],
    exports: [
        ...COMPONENTS,
    ],
    providers: [],
})
export class FCLImportShareModule {

}