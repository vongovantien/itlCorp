import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SaleReportFormSearchComponent } from './components/form-search-sale-report/form-search-sale-report.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { ShareModulesModule } from '../share-modules/share-modules.module';
@NgModule({
    declarations: [SaleReportFormSearchComponent],
    imports: [ 
        CommonModule,
        NgxDaterangepickerMd,
        SelectModule,
        ModalModule,
        SharedModule,
        ShareModulesModule
        
     ],
    exports: [
        SaleReportFormSearchComponent
    ],
    providers: [],
})
export class ShareReportModule {

}