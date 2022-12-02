import { NgModule } from '@angular/core';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgxDaterangepickerMd } from 'ngx-daterangepicker-material';
import { SharedModule } from 'src/app/shared/shared.module';
import { CustomClearanceFormDetailComponent } from './custom-clearance/components/form-detail-clearance/form-detail-clearance.component';

@NgModule({
    declarations: [CustomClearanceFormDetailComponent],
    imports: [
        SharedModule,
        NgxDaterangepickerMd,
        NgSelectModule
    ],
    exports: [CustomClearanceFormDetailComponent]
})
export class SharedOperationModule { }
