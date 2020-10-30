import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SharedModule } from 'src/app/shared/shared.module';
import { OpsCDNoteComponent } from './ops-cd-note-list.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { OpsCdNoteDetailPopupComponent } from '../components/popup/ops-cd-note-detail/ops-cd-note-detail.popup';
import { OpsCdNoteAddPopupComponent } from '../components/popup/ops-cd-note-add/ops-cd-note-add.popup';
import { OpsCdNoteAddRemainingChargePopupComponent } from '../components/popup/ops-cd-note-add-remaining-charge/ops-cd-note-add-remaining-charge.popup';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';
import { ModalModule } from 'ngx-bootstrap/modal';
import { SelectModule } from 'ng2-select';


@NgModule({
    declarations: [
        OpsCDNoteComponent,
        OpsCdNoteDetailPopupComponent,
        OpsCdNoteAddPopupComponent,
        OpsCdNoteAddRemainingChargePopupComponent,
    ],
    imports: [
        SharedModule,
        FormsModule,
        ShareBussinessModule,
        ModalModule,
        FormsModule,
        SelectModule,
        ReactiveFormsModule,
        CommonModule
    ],
    exports: [],
    providers: [],
    entryComponents: [
        OpsCDNoteComponent
    ]
})
export class OpsCDNoteModule {
    static rootComponent = OpsCDNoteComponent;
}