import { NgModule } from '@angular/core';
import { ShareBussinessCdNoteListSeaComponent } from 'src/app/business-modules/share-business/components/cd-note-sea/cd-note-list/cd-note-list.component';
import { ShareBussinessModule } from 'src/app/business-modules/share-business/share-bussines.module';

@NgModule({
    imports: [
        ShareBussinessModule
    ],
    exports: [],
    declarations: [
    ],
    providers: [],
    entryComponents: [
        ShareBussinessCdNoteListSeaComponent
    ]
})
export class SeaLCLExportCDNoteModule {
    static rootComponent = ShareBussinessCdNoteListSeaComponent;
}
