import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalModule } from 'ngx-bootstrap';
import { FormsModule } from '@angular/forms';
import { SharedModule } from 'src/app/shared/shared.module';
import { CdNoteListComponent } from 'src/app/business-modules/share-business/components/cd-note/cd-note-list/cd-note-list.component';
import { CdNoteAddPopupComponent } from '../../../sea-fcl-import/components/popup/add-cd-note/add-cd-note.popup';
import { CdNoteDetailPopupComponent } from '../../../sea-fcl-import/components/popup/detail-cd-note/detail-cd-note.popup';
import { CdNoteAddRemainingChargePopupComponent } from '../../../sea-fcl-import/components/popup/add-remaining-charge/add-remaining-charge.popup';


@NgModule({
    imports: [
        CommonModule,
        ModalModule.forRoot(),
        FormsModule,
        SharedModule,
    ],
    exports: [],
    declarations: [
        CdNoteListComponent,
        // CdNoteAddPopupComponent,
        // CdNoteDetailPopupComponent,
        // CdNoteAddRemainingChargePopupComponent,
    ],
    providers: [],
    entryComponents: [
        CdNoteListComponent
    ]
})
export class SeaFCLExportCDNoteModule {
    static rootComponent = CdNoteListComponent;
}
