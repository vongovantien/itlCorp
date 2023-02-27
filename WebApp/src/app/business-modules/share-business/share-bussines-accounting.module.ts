import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { NgSelectModule } from '@ng-select/ng-select';
import { ModalModule } from 'ngx-bootstrap/modal';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareDocumentTypeAttachComponent } from './components/edoc/document-type-attach/document-type-attach.component';
import { ShareBussinessAttachFileV2Component } from './components/edoc/files-attach-v2/files-attach-v2.component';

@NgModule({
    declarations: [
        ShareBussinessAttachFileV2Component,
        ShareDocumentTypeAttachComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        ModalModule,
        NgSelectModule
    ],
    exports: [
        ShareBussinessAttachFileV2Component,
        ShareDocumentTypeAttachComponent
    ],
    providers: [],
})
export class ShareBussinessAccountingModule { }
