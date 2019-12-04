import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ModalModule } from 'ngx-bootstrap';
import { SharedModule } from 'src/app/shared/shared.module';
import { FormsModule } from '@angular/forms';
import { ShareBussinessCdNoteListComponent } from 'src/app/business-modules/share-business/components/cd-note/cd-note-list/cd-note-list.component';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';

@NgModule({
    imports: [
        CommonModule,
        ModalModule.forRoot(),
        SharedModule,
        FormsModule,
        ShareBussinessModule
    ],
    exports: [],
    declarations: [
    ],
    providers: [],
    entryComponents: [
        ShareBussinessCdNoteListComponent
    ]
})
export class SeaLCLImportCDNoteModule {
    static rootComponent = ShareBussinessCdNoteListComponent;
}
