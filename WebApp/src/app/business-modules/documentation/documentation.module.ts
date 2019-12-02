import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DocumentationRoutingModule } from './documentation-routing.module';
import { AirExportComponent } from './air-export/air-export.component';
import { AirImportComponent } from './air-import/air-import.component';
import { SeaLCLExportComponent } from './sea-lcl-export/sea-lcl-export.component';
import { SeaConsolExportComponent } from './sea-consol-export/sea-consol-export.component';
import { SeaConsolImportComponent } from './sea-consol-import/sea-consol-import.component';
import { InlandTruckingComponent } from './inland-trucking/inland-trucking.component';
import { SharedModule } from '../../shared/shared.module';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';
import { PerfectScrollbarModule } from 'ngx-perfect-scrollbar';

import { SeaLclExportCreateComponent } from './sea-lcl-export-create/sea-lcl-export-create.component';
import { SeaLclExportShipmentDetailComponent } from './sea-lcl-export-create/sea-lcl-export-shipment-detail/sea-lcl-export-shipment-detail.component';
import { SeaLclExportHousebillListComponent } from './sea-lcl-export-create/sea-lcl-export-housebill-list/sea-lcl-export-housebill-list.component';
import { SeaLclExportHousebillAddnewComponent } from './sea-lcl-export-create/sea-lcl-export-housebill-addnew/sea-lcl-export-housebill-addnew.component';
import { SeaLclExportDetailImportComponent } from './sea-lcl-export-create/sea-lcl-export-detail-import/sea-lcl-export-detail-import.component';
import { SeaLclExportHousebillDetailImportComponent } from './sea-lcl-export-create/sea-lcl-export-housebill-detail-import/sea-lcl-export-housebill-detail-import.component';
import { SeaLclExportCreditAndDebitNoteComponent } from './sea-lcl-export-create/sea-lcl-export-credit-and-debit-note/sea-lcl-export-credit-and-debit-note.component';
import { SeaLclExportCreditAndDebitNoteAddnewComponent } from './sea-lcl-export-create/sea-lcl-export-credit-and-debit-note/sea-lcl-export-credit-and-debit-note-addnew/sea-lcl-export-credit-and-debit-note-addnew.component';
import { SeaLclExportCreditAndDebitNoteRemainingChargeComponent } from './sea-lcl-export-create/sea-lcl-export-credit-and-debit-note/sea-lcl-export-credit-and-debit-note-remaining-charge/sea-lcl-export-credit-and-debit-note-remaining-charge.component';
import { SeaLclExportCreditAndDebitNoteDetailComponent } from './sea-lcl-export-create/sea-lcl-export-credit-and-debit-note/sea-lcl-export-credit-and-debit-note-detail/sea-lcl-export-credit-and-debit-note-detail.component';
import { SeaLclExportCreditAndDebitNoteEditComponent } from './sea-lcl-export-create/sea-lcl-export-credit-and-debit-note/sea-lcl-export-credit-and-debit-note-edit/sea-lcl-export-credit-and-debit-note-edit.component';
import { SeaLclExportManifestComponent } from './sea-lcl-export-create/sea-lcl-export-manifest/sea-lcl-export-manifest.component';
import { SeaLclExportShippingInstructionComponent } from './sea-lcl-export-create/sea-lcl-export-shipping-instruction/sea-lcl-export-shipping-instruction.component';
import { TabsModule } from 'ngx-bootstrap';


@NgModule({
  imports: [
    // CommonModule,
    DocumentationRoutingModule,
    ReactiveFormsModule.withConfig({ warnOnNgModelWithFormControl: 'never' }),
    SharedModule,
    NgxDaterangepickerMd,
    SelectModule,
    FormsModule,
    PerfectScrollbarModule,
    TabsModule.forRoot()// Scrollbar
  ],
  declarations: [
    AirExportComponent,
    AirImportComponent,
    SeaLCLExportComponent,
    SeaConsolExportComponent,
    SeaConsolImportComponent,
    InlandTruckingComponent,

    SeaLclExportCreateComponent, SeaLclExportShipmentDetailComponent, SeaLclExportHousebillListComponent, SeaLclExportHousebillAddnewComponent, SeaLclExportDetailImportComponent, SeaLclExportHousebillDetailImportComponent, SeaLclExportCreditAndDebitNoteComponent, SeaLclExportCreditAndDebitNoteAddnewComponent, SeaLclExportCreditAndDebitNoteRemainingChargeComponent, SeaLclExportCreditAndDebitNoteDetailComponent, SeaLclExportCreditAndDebitNoteEditComponent, SeaLclExportManifestComponent, SeaLclExportShippingInstructionComponent,
  ]
})
export class DocumentationModule { }
