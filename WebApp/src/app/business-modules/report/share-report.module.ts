import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShareFormSearchReportComponent } from './components/share-form-search-report/share-form-search-report.component';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SelectModule } from 'ng2-select';
import { SharedModule } from 'src/app/shared/shared.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { ShareModulesModule } from '../share-modules/share-modules.module';
@NgModule({
    declarations: [ShareFormSearchReportComponent],
    imports: [ 
        CommonModule,
        NgxDaterangepickerMd,
        SelectModule,
        ModalModule,
        SharedModule,
        ShareModulesModule
        
     ],
    exports: [
        ShareFormSearchReportComponent
    ],
    providers: [],
})
export class ShareReportModule {

}
