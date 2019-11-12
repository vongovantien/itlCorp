import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SeaFCLImportCDNoteComponent } from './sea-fcl-import-cd-note.component';
import { CdNoteAddPopupComponent } from '../../components/popup/add-cd-note/add-cd-note.popup';
import { CdNoteDetailPopupComponent } from '../../components/popup/detail-cd-note/detail-cd-note.popup';
import { CdNoteAddRemainingChargePopupComponent } from '../../components/popup/add-remaining-charge/add-remaining-charge.popup';
import { ModalModule } from 'ngx-bootstrap';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';


@NgModule({
    imports: [
        CommonModule,
        ModalModule.forRoot(),
        SharedModule,
        FormsModule

    ],
    exports: [],
    declarations: [
        SeaFCLImportCDNoteComponent,
        CdNoteAddPopupComponent,
        CdNoteDetailPopupComponent,
        CdNoteAddRemainingChargePopupComponent,

    ],
    providers: [],
    entryComponents: [
        SeaFCLImportCDNoteComponent
    ]
})
export class SeaFCLImportCDNoteModule {
    static rootComponent = SeaFCLImportCDNoteComponent;
}
