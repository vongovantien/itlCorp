import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule} from '@angular/forms';
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
import { SharedModule } from '../../shared/shared.module';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';
import { PerfectScrollbarModule, PerfectScrollbarConfigInterface, PERFECT_SCROLLBAR_CONFIG } from 'ngx-perfect-scrollbar';
import { SeaFclExportCreateComponent } from './sea-fcl-export-create/sea-fcl-export-create.component';
import { MasterBillComponent } from './sea-fcl-export-create/master-bill/master-bill.component';
import { HousebillListComponent } from './sea-fcl-export-create/housebill-list/housebill-list.component';
import { HousebillAddnewComponent } from './sea-fcl-export-create/housebill-addnew/housebill-addnew.component';
import { CreditAndDebitNoteComponent } from './sea-fcl-export-create/credit-and-debit-note/credit-and-debit-note.component';
import { SeaFclExportDetailImportComponent } from './sea-fcl-export-create/sea-fcl-export-detail-import/sea-fcl-export-detail-import.component';
import { HousebillImportDetailComponent } from './sea-fcl-export-create/housebill-import-detail/housebill-import-detail.component';
import { CreditAndDebitNoteAddnewComponent } from './sea-fcl-export-create/credit-and-debit-note/credit-and-debit-note-addnew/credit-and-debit-note-addnew.component';
import { CreditAndDebitNoteDetailComponent } from './sea-fcl-export-create/credit-and-debit-note/credit-and-debit-note-detail/credit-and-debit-note-detail.component';
import { CreditAndDebitNoteEditComponent } from './sea-fcl-export-create/credit-and-debit-note/credit-and-debit-note-edit/credit-and-debit-note-edit.component';
import { CreditAndDebitNoteRemainingChargeComponent } from './sea-fcl-export-create/credit-and-debit-note/credit-and-debit-note-remaining-charge/credit-and-debit-note-remaining-charge.component';
import { ManifestComponent } from './sea-fcl-export-create/manifest/manifest.component';
import { ShippingInstructionComponent } from './sea-fcl-export-create/shipping-instruction/shipping-instruction.component';

@NgModule({
  imports: [
    CommonModule,
    DocumentationRoutingModule,
    ReactiveFormsModule.withConfig({warnOnNgModelWithFormControl: 'never'}),
    SharedModule,
    NgxDaterangepickerMd,
    SelectModule,
    FormsModule,
    PerfectScrollbarModule, // Scrollbar
  ],
  declarations: [   
    // TwoDigitDecimaNumberDirective,
    AirExportComponent, 
    AirImportComponent, 
    SeaFCLExportComponent, 
    SeaLCLExportComponent, 
    SeaConsolExportComponent, 
    SeaFCLImportComponent, 
    SeaLCLImportComponent, 
    SeaConsolImportComponent, 
    InlandTruckingComponent, 
    SeaFclExportCreateComponent, 
    MasterBillComponent, 
    HousebillListComponent, 
    HousebillAddnewComponent,     
    CreditAndDebitNoteComponent, 
    SeaFclExportDetailImportComponent, 
    HousebillImportDetailComponent, 
    CreditAndDebitNoteAddnewComponent, 
    CreditAndDebitNoteDetailComponent, 
    CreditAndDebitNoteEditComponent, 
    CreditAndDebitNoteRemainingChargeComponent, 
    ManifestComponent, 
    ShippingInstructionComponent]
})
export class DocumentationModule { }
