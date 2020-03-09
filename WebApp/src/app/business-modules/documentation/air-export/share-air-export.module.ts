import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShareAirExportOtherChargePopupComponent } from './share/other-charge/air-export-other-charge.popup';
import { CommonComponentModule } from 'src/app/shared/common/common.module';
import { ModalModule } from 'ngx-bootstrap';
import { DirectiveModule } from 'src/app/shared/directives/directive.module';

@NgModule({
    declarations: [
        ShareAirExportOtherChargePopupComponent
    ],
    imports: [
        CommonModule,
        CommonComponentModule,
        ModalModule,
        DirectiveModule
    ],
    exports: [
        ShareAirExportOtherChargePopupComponent
    ],
    providers: [],
})
export class ShareAirExportModule {
}
