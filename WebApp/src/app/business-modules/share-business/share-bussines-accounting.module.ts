import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ShareDocumentTypeAttachComponent } from './components/document-type-attach/document-type-attach.component';
import { SharedModule } from 'src/app/shared/shared.module';
import { ShareBussinessAttachFileV2Component } from './components/files-attach-v2/files-attach-v2.component';
import { ModalModule } from 'ngx-bootstrap/modal';

@NgModule({
    declarations: [
        ShareBussinessAttachFileV2Component,
        ShareDocumentTypeAttachComponent
    ],
    imports: [
        CommonModule,
        SharedModule,
        ModalModule,
    ],
    exports: [
        ShareBussinessAttachFileV2Component,
        ShareDocumentTypeAttachComponent
    ],
    providers: [],
})
export class ShareBussinessAccountingModule { }